using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Receipt;
using System.Text.Json;
using tbank_back_web.Controllers.Finder.Filtration;
using tbank_back_web.Controllers.Finder.Models.Dto;
using tbank_back_web.Controllers.Finder.Models.FindReceiptsForDay;
using tbank_back_web.Controllers.Finder.Models.FindReceiptsForMonth;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Core.Identity.User;
using tbank_back_web.Infrastructure.DbContext;
using tbank_back_web.Infrastructure.Services;
using static tbank_back_web.Controllers.Finder.FinderController;
using static tbank_back_web.Core.Data_Entities.Business.IngredientEntity;

namespace tbank_back_web.Controllers.Finder
{


	[ApiController]
	[Route("")]
	public class FinderController : ControllerBase
	{

		static async Task<bool> GetReceiptAsync(ReceiptService.ReceiptServiceClient client, string query, ApplicationDbContext db, NutrientsSummarizerService summator)
		{
			try
			{
				Console.WriteLine($"🔍 Поиск рецепта: {query}");

				var request = new ReceiptRequest { ReceiptQuery = query };
				var response = await client.GetReceiptInfoAsync(request);

				// Парсинг JSON для красивого вывода
				var ingredientsData = JsonSerializer.Deserialize<List<IngredientResponseModel>>(response.IngredientsJson);
				var recipeData = JsonSerializer.Deserialize<ReceiptResponseModel>(response.ReceiptJson);

				foreach (var ing in ingredientsData)
				{
					if (!db.Ingredients.Any(e => e.Title.ToLower().Trim() == ing.title.ToLower().Trim()))
						await db.Ingredients.AddAsync(new IngredientEntity
						{
							Carbs = ing.carbs,
							Fat = ing.fat,
							Kcal = ing.kcal,
							MeasurementUnit = (IngredientMeasurementUnits)Enum.Parse(typeof(IngredientMeasurementUnits), ing.measurementUnit),
							Protein = ing.protein,
							Title = ing.title,
						});

				}
				await db.SaveChangesAsync();

				ReceiptEntity rec = new();
				rec.IngredientsAmount = new Dictionary<string, int>();

				foreach (var ing in recipeData.ingridients)
				{
					rec.IngredientsAmount.Add(ing.title, ing.amount);
				}
				rec.TotalKcal = summator.GetKcalSumm(rec);
				rec.TotalProtein = summator.GetProteinSumm(rec);
				rec.TotalFat = summator.GetFatSumm(rec);
				rec.TotalCarbs = summator.GetCarbsSumm(rec);
				rec.CreatedAt = DateTime.Now.ToFileTimeUtc();
				rec.Instructions = recipeData.instructions;
				rec.Title = recipeData.title;

					if (!db.Receipts.Any(e => e.Title.ToLower().Trim() == rec.Title.ToLower().Trim()))
					await db.Receipts.AddAsync(rec);
				await db.SaveChangesAsync();

				Console.WriteLine(new string('-', 50));
			}
			catch (RpcException ex)
			{
				Console.WriteLine($"❌ Ошибка при запросе '{query}': {ex.Status.Detail}");
				return false;
			}
			catch (JsonException ex)
			{
				Console.WriteLine("❌ Ошибка парсинга JSON ответа" + ex.Message);
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
			return true;
		}


		[AllowAnonymous]
		[HttpPost("/create-recipe")]
		public async Task<IActionResult> CreateRecipeAI(
			[FromQuery] string recipeQuery,
			[FromServices] ApplicationDbContext db,
			[FromServices] NutrientsSummarizerService summator
			)
		{
			var channel = GrpcChannel.ForAddress("http://micser-2:5000");
			var client = new ReceiptService.ReceiptServiceClient(channel);
			return await GetReceiptAsync(client, recipeQuery, db, summator) ?
				Ok() : BadRequest();
		}

		[HttpPost("/plan-day")]
		public async Task<IActionResult> FindReceips(
			[FromServices] UserManager<BaseApplicationUser> userManager,
			FindReceipsRequestModel avaibableProducts,
			[FromServices] PlannerService planner,
			[FromServices] ApplicationDbContext db)
		{

			var currentUser = await userManager.GetUserAsync(User);
			if (currentUser == null)
			{
				return Unauthorized("User not found");
			}
			var nres = NutritionCalculator.CalculateDailyNutrition(currentUser);

			var res = await planner.FindRecipesStochasticAsync(avaibableProducts.Titles, new PlannerService.NutritionTarget
			{
				TargetCarbs = nres.TargetCarbs,
				TargetFat = nres.TargetFat,
				TargetKcal = nres.TargetKcal,
				TargetProtein = nres.TargetProtein
			});

			if (res == null)
				return BadRequest(null);

			List<ReceiptResponseModel> receipts = res.Select(e => new ReceiptResponseModel
			{
				ingridients = e.IngredientsAmount.ToReceiptComponents(),
				instructions = e.Instructions,
				title = e.Title
			}).ToList();

			return Ok(new FindReceipsResponseModel
			{
				ingridientsToBuy = res.Select(e => new IngredientResponseModel
				{
					fat = 0,
					carbs = 0,
					kcal = 0,
					protein = 0,
					title = null,
				}).ToList(),
				brekfast = receipts[0],
				lunch = receipts[1],
				dinner = receipts[2],
				snack = receipts[3],
			});
		}

		[AllowAnonymous]
		[HttpGet("/get-ingridients")]
		public async Task<IActionResult> GetAllIngridients(
			[FromServices] ApplicationDbContext db, int page = 1)
		{
			return Ok(db.Ingredients.Select(e => new IngredientResponseModel
			{
				carbs = e.Carbs,
				kcal = e.Kcal,
				protein = e.Protein,
				title = e.Title,
				fat = e.Fat,
				measurementUnit = e.MeasurementUnit.ToString(),
			}).Skip((page - 1) * 50).Take(50).ToList());
		}

		[AllowAnonymous]
		[HttpGet("/search-recipes")]
		public async Task<IActionResult> SearchRecipes(
			[FromQuery] SearchRecipesRequestModel search,
			[FromServices] ApplicationDbContext db,
			[FromServices] NutrientsSummarizerService summarizerService)
		{
			List<ReceiptResponseModel> resultRecepts;

			resultRecepts = db.Receipts.ApplySearchFilters(search, summarizerService)
			.OrderByDescending(e => e.CreatedAt)
			.Select(e => new ReceiptResponseModel
			{
				ingridients = e.IngredientsAmount.ToReceiptComponents(),
				instructions = e.Instructions,
				title = e.Title
			}).ToList();
			return Ok(resultRecepts);
		}
	}
}

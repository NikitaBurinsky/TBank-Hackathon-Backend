using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tbank_back_web.Controllers.Finder.Filtration;
using tbank_back_web.Controllers.Finder.Models;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Core.Identity.User;
using tbank_back_web.Infrastructure.DbContext;
using tbank_back_web.Infrastructure.Services;

namespace tbank_back_web.Controllers.Finder
{
	[ApiController]
	[Route("")]
	public class FinderController : ControllerBase
	{
		public class FindReceipsRequestModel
		{
			public List<string> Titles { get; set; }
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

			var res = await planner.FindReceipts(avaibableProducts.Titles,nres.TargetProtein,nres.TargetFat, nres.TargetCarbs, nres.TargetKcal);

			List<ReceiptResponseModel> receipts = res.Item1.Select(e => new ReceiptResponseModel
			{
				ingridients = e.IngredientsAmount.ToReceiptComponents(),
				instructions = e.Instructions,
				title = e.Title
			}).ToList();

			return Ok(new FindReceipsResponseModel
			{
				ingridientsToBuy = res.Item2.Select(e => new IngredientResponseModel
				{
					fat = e.Fat,
					carbs = e.Carbs,
					kcal = e.Kcal,
					protein = e.Protein,
					title = e.Title,
				}).ToList(),
				brekfast = receipts[0],
				lunch = receipts[1],
				dinner = receipts[2],
				snack = receipts[3],
			});
		}
		public class FindReceipsResponseModel
		{
			public ReceiptResponseModel brekfast { get; set; }
			public ReceiptResponseModel lunch { get; set; }
			public ReceiptResponseModel dinner { get; set; }
			public ReceiptResponseModel snack { get; set; }
			public List<IngredientResponseModel> ingridientsToBuy { get; set; }
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
		[HttpPost("/search-recipes")]
		public async Task<IActionResult> SearchRecipes(SearchRecipesRequestModel search,
			[FromServices] ApplicationDbContext db,
			[FromServices] NutrientsSummarizerService summarizerService) 
		{
			List<ReceiptResponseModel> resultRecepts;

			resultRecepts = db.Receipts.ApplySearchFilters(search, summarizerService).Select(e => new ReceiptResponseModel
			{
				ingridients = e.IngredientsAmount.ToReceiptComponents(),
				instructions = e.Instructions,
				title = e.Title
			}).ToList();
			return Ok(resultRecepts);
		}
		public class SearchRecipesRequestModel
		{
			public int? kcalMax { get; set; }
			public int? kcalMin { get; set; }
			public float? proteinPercMax { get; set; }
			public float? proteinPercMin { get; set; }
			public float? fatPercMax { get; set; }
			public float? fatPercMin { get; set; }
			public string? search { get; set; }
		}


	}
}

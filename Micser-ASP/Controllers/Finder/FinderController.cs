using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
		[HttpPost("/plan-day")]
		public async Task<IActionResult> FindReceips(
			[FromServices] UserManager<BaseApplicationUser> userManager,
			List<string> avaibableProducts,
			[FromServices] PlannerService planner,
			[FromServices] NutritionCalculator calculator,
			[FromServices] ApplicationDbContext db) 
		{	

			var currentUser = await userManager.GetUserAsync(User);
			var nres = NutritionCalculator.CalculateDailyNutrition(currentUser);

			var res = await planner.FindReceipts(avaibableProducts,nres.TargetProtein,nres.TargetFat, nres.TargetCarbs, nres.TargetKcal);

			List<ReceiptResponseModel> receipts = res.Item1.Select(e => new ReceiptResponseModel
			{
				components = e.IngredientsAmount.ToReceiptComponents(),
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
			}).Skip((page - 1) * 15).Take(15).ToList());
		}



	}
}

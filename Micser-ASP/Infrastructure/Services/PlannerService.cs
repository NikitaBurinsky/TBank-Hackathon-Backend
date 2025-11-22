using Microsoft.EntityFrameworkCore;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Infrastructure.DbContext;
using static tbank_back_web.Infrastructure.Services.NutritionCalculator;

public class PlannerService
{
		private readonly ApplicationDbContext _context;
		private readonly Random _random;
		NutrientsSummarizerService NutrientsSummarizerService;

		public PlannerService(ApplicationDbContext context,
			NutrientsSummarizerService nutrientsSummarizerService)
		{
			_context = context;
			NutrientsSummarizerService = nutrientsSummarizerService;
			_random = new Random();
		}
		private class Recipe
		{
			public double Carbs { get; set; }
			public double Kcal { get; set; }
			public double Protein { get; set; }
			public double Fat { get; set; }
		}

		public class NutritionTarget
		{
			public double TargetCarbs { get; set; }
			public double TargetKcal { get; set; }
			public double TargetProtein { get; set; }
			public double TargetFat { get; set; }
			public double Tolerance { get; set; } = 5; // 15%
		}


		public async Task<List<ReceiptEntity>> FindRecipesStochasticAsync(List<string> avaibableProducts ,NutritionTarget target, int maxAttempts = 1000)
		{
			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				var recipes = await GetRandomRecipesAsync(4);

				if (IsCombinationValid(recipes, target))
					return recipes;
			}

			return null;
		}

		private async Task<List<ReceiptEntity>> GetRandomRecipesAsync(int count)
		{
			// Получаем случайные рецепты из БД
			var totalCount = await _context.Receipts.CountAsync();
			var skip = _random.Next(0, Math.Max(0, totalCount - count));

			return await _context.Receipts
				.Skip(skip)
				.Take(count)
				.ToListAsync();
		}

		private bool IsCombinationValid(List<ReceiptEntity> combination, NutritionTarget target)
		{
			var totalCarbs = combination.Sum(NutrientsSummarizerService.GetCarbsSumm);
			var totalKcal = combination.Sum(NutrientsSummarizerService.GetKcalSumm);
			var totalProtein = combination.Sum(NutrientsSummarizerService.GetProteinSumm);
			var totalFat = combination.Sum(NutrientsSummarizerService.GetFatSumm);

			return IsWithinTolerance(totalCarbs, target.TargetCarbs, target.Tolerance) &&
				   IsWithinTolerance(totalKcal, target.TargetKcal, target.Tolerance) &&
				   IsWithinTolerance(totalProtein, target.TargetProtein, target.Tolerance) &&
				   IsWithinTolerance(totalFat, target.TargetFat, target.Tolerance);
		}

		private bool IsWithinTolerance(double actual, double target, double tolerance)
		{
			return actual >= target * (1 - tolerance) &&
				   actual <= target * (1 + tolerance);
		}
	}

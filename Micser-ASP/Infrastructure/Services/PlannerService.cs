using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Infrastructure.DbContext;
using static tbank_back_web.Infrastructure.Services.NutritionCalculator;

public class PlannerService
{
	private readonly ApplicationDbContext _context;
	private readonly Random _random;
	IServiceProvider _serviceProvider;
	NutrientsSummarizerService NutrientsSummarizerService;

	public PlannerService(ApplicationDbContext context,
		NutrientsSummarizerService nutrientsSummarizerService,
		IServiceProvider serviceProvider)
	{
		_context = context;
		NutrientsSummarizerService = nutrientsSummarizerService;
		_random = new Random();
		_serviceProvider = serviceProvider;
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
		public double Tolerance { get; set; } = 0.2; // 15%
	}


		public ReceiptEntity FindMatchingRecipeParallel(NutritionTarget target, NutritionTarget tolerances)
	{
		using var scope = _serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		int totalCount = Math.Min(context.Receipts.Count(), 40);
		int threadCount = 3;
		int batchSize = (int)Math.Ceiling((double)totalCount / threadCount);

		var result = new ConcurrentBag<ReceiptEntity>();

		Parallel.For(0, threadCount, threadIndex =>
		{
			// Создаем новый scope для каждого потока
			using var threadScope = _serviceProvider.CreateScope();
			var threadContext = threadScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

			int start = threadIndex * batchSize;
			int end = Math.Min(start + batchSize, totalCount);

			for (int i = start; i < end; i++)
			{
				var recipe = threadContext.Receipts
					.Skip(i)
					.FirstOrDefault();

				if (recipe != null &&
					Math.Abs(recipe.TotalCarbs - target.TargetCarbs) <= tolerances.TargetCarbs &&
					Math.Abs(recipe.TotalFat - target.TargetFat) <= tolerances.TargetFat &&
					Math.Abs(recipe.TotalProtein - target.TargetProtein) <= tolerances.TargetProtein &&
					Math.Abs(recipe.TotalKcal - target.TargetKcal) <= tolerances.TargetKcal)
				{
					result.Add(recipe);
					return;
				}
			}
		});

		return result.FirstOrDefault();
	}
	private ReceiptEntity FindLastWithParameters(NutritionTarget target, NutritionTarget tolerances)
	{
		int count = Math.Min(_context.Receipts.Count(), 50);
		for (int i = 0; i < count; ++i)
		{
			ReceiptEntity? e = _context.Receipts.Skip(i).FirstOrDefault(e => true);
			if (Math.Abs(e.TotalCarbs - target.TargetCarbs) <= tolerances.TargetCarbs &&
			   Math.Abs(e.TotalFat - target.TargetFat) <= tolerances.TargetFat &&
			   Math.Abs(e.TotalProtein - target.TargetProtein) <= tolerances.TargetProtein &&
			   Math.Abs(e.TotalKcal - target.TargetKcal) <= tolerances.TargetKcal)
				return e;
		}
		return _context.Receipts.Skip(count - 1).FirstOrDefault();
	}

	private NutritionTarget GetLastProductDelta(List<ReceiptEntity> combination, NutritionTarget target)
	{
		float totalCarbs = combination.Sum(e => e.TotalCarbs);
		int totalKcal = combination.Sum(e => e.TotalKcal);
		float totalProtein = combination.Sum(e => e.TotalProtein);
		float totalFat = combination.Sum(e => e.TotalFat);

		return new NutritionTarget
		{
			TargetCarbs = target.TargetCarbs - totalCarbs,
			TargetFat = target.TargetCarbs - totalCarbs,
			TargetKcal = target.TargetKcal - totalKcal,
			TargetProtein = target.TargetProtein - totalProtein,
			Tolerance = target.Tolerance
		};
	}


	public async Task<List<ReceiptEntity>> FindRecipesStochasticAsync(List<string> avaibableProducts, NutritionTarget target, int maxAttempts = 100)
	{
		for (int attempt = 0; attempt < maxAttempts; attempt++)
		{
			var recipes = await GetRandomRecipesAsync(3);
			var lastDelta = GetLastProductDelta(recipes, target);
			NutritionTarget tolerances = new NutritionTarget
			{
				TargetCarbs = target.TargetCarbs * (target.Tolerance),
				TargetProtein = target.TargetProtein * (target.Tolerance),
				TargetFat = target.TargetFat * (target.Tolerance),
				TargetKcal = target.TargetKcal * (target.Tolerance),
			};


			if (lastDelta.TargetKcal + tolerances.TargetKcal < 0 ||
			lastDelta.TargetFat + tolerances.TargetFat < 0 ||
			lastDelta.TargetProtein + tolerances.TargetProtein < 0 ||
			lastDelta.TargetCarbs + tolerances.TargetCarbs < 0)
				continue;

			var lastRecipe = FindMatchingRecipeParallel(lastDelta, tolerances);
			if (lastRecipe == null)
				continue;
			recipes.Add(lastRecipe);
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

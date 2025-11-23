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
		public double Tolerance { get; set; } = 2;
	}

	private async Task<ReceiptEntity> FindLastWithParameters(NutritionTarget target, NutritionTarget tolerances)
	{
		return await _context.Receipts.Where(e => e.TotalCarbs <= target.TargetCarbs + tolerances.TargetCarbs  &&
		e.TotalKcal <= target.TargetKcal + tolerances.TargetKcal &&
		e.TotalProtein <= target.TargetProtein + tolerances.TargetProtein &&
		e.TotalFat <= target.TargetFat + tolerances.TargetFat).FirstOrDefaultAsync();
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


	public async Task<List<ReceiptEntity>> FindRecipesStochasticAsync(List<string> avaibableProducts, NutritionTarget target, int maxAttempts = 150)
	{
		List<List<ReceiptEntity>> frt = new List<List<ReceiptEntity>>();
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

			var lastRecipe = await FindLastWithParameters(lastDelta, tolerances);
			if (lastRecipe == null)
				continue;
			recipes.Add(lastRecipe);
			if (IsCombinationValid(recipes, target))
				frt.Add(recipes);
		}
		if (frt.Count == 0)
			return null;
		List<ReceiptEntity> minimalToBuyList = null;
		int minimalToBuyCount = 100;
		foreach(var recipes in frt )
		{
			int curHowManyBuy = CalculateHowManyToBuy(recipes, avaibableProducts);
			if(curHowManyBuy < minimalToBuyCount)
			{
				minimalToBuyCount = curHowManyBuy;
				minimalToBuyList = recipes;
			}	
		}
		return minimalToBuyList;
	}

	private int CalculateHowManyToBuy(List<ReceiptEntity> receipts, List<string> avaibableProducts)
	{
		if (receipts == null || !receipts.Any() || avaibableProducts == null)
			return 0;

		// Создаем словарь для суммарного количества каждого ингредиента
		var totalIngredientsNeeded = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		// Суммируем количество каждого ингредиента из всех рецептов
		foreach (var receipt in receipts)
		{
			if (receipt?.IngredientsAmount == null)
				continue;

			foreach (var ingredient in receipt.IngredientsAmount)
			{
				var ingredientName = ingredient.Key;
				var amount = ingredient.Value;

				if (totalIngredientsNeeded.ContainsKey(ingredientName))
				{
					totalIngredientsNeeded[ingredientName] += amount;
				}
				else
				{
					totalIngredientsNeeded[ingredientName] = amount;
				}
			}
		}

		// Подсчитываем недостающие ингредиенты
		int missingIngredientsCount = 0;

		foreach (var ingredient in totalIngredientsNeeded)
		{
			var ingredientName = ingredient.Key;
			var totalAmountNeeded = ingredient.Value;

			// Проверяем, есть ли этот ингредиент в доступных продуктах
			if (!avaibableProducts.Any(ap =>
				string.Equals(ap, ingredientName, StringComparison.OrdinalIgnoreCase)))
			{
				missingIngredientsCount++;
			}
		}

		return missingIngredientsCount;
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

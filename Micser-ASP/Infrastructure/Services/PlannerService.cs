using Microsoft.EntityFrameworkCore;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Infrastructure.DbContext;

public class PlannerService
{
	private readonly ApplicationDbContext _context;
	NutrientsSummarizerService NutrientsSummarizer;
	private class NutrientsResult
	{
		public int totalKcal { get; set; }
		public float totaltFat { get; set; }
		public float totalCarbs { get; set; }
		public float totalProtein { get; set; }
	}

	public PlannerService(ApplicationDbContext context, NutrientsSummarizerService nutrients)
	{
		NutrientsSummarizer = nutrients;
		_context = context;
	}
	private IQueryable<IngredientEntity> GetReceiptIngredients(ReceiptEntity receipt)
	{
		return _context.Ingredients.Where(e => receipt.IngredientsAmount.Keys.Contains(e.Title)).AsQueryable();
	}




	public async Task<(List<ReceiptEntity>, List<IngredientEntity>)> FindReceiptCombinations(
List<string> availableIngredients,
float targetProtein, float targetFat, float targetCarbs, int targetKcal, int skipDays = 0)
	{ throw new NotImplementedException(); }


	/*
	public async Task<(List<ReceiptEntity>, List<IngredientEntity>)> FindReceiptCombinations(
	List<string> availableIngredients,
	float targetProtein, float targetFat, float targetCarbs, int targetKcal, int skipDays = 0)
	{
		// Получаем все рецепты из базы данных
		var allReceipts = await _context.Receipts.Take(50)
			.ToListAsync();

		List<(float portion, ReceiptEntity recept)> recepts 
			= new List<(float portionAmount, ReceiptEntity recept)>();

		var firstReceip = allReceipts[Random.Shared.Next()];
		float firsstPortion = NutrientsSummarizer.GetTotalKcal(GetReceiptIngredients(firstReceip)) / (targetKcal / 4);


		var secondReceip = allReceipts[Random.Shared.Next()];
		float secondPortion = NutrientsSummarizer.GetTotalKcal(GetReceiptIngredients(firstReceip)) / (targetKcal / 4);

		var thirdReceip = allReceipts[Random.Shared.Next()];
		float thirdPortion = NutrientsSummarizer.GetTotalKcal(GetReceiptIngredients(firstReceip)) / (targetKcal / 4);

		






























		// Предварительно рассчитываем нутриенты для всех рецептов
		var receiptsWithNutrition = allReceipts.Select(receipt =>
		{
			var (protein, fat, carbs, kcal, missingIngredients) = CalculateReceiptNutrition(receipt, ingredientDict);
			var missingIngredientsCount = receipt.IngredientsAmount
				.Keys
				.Count(ingredientName => !availableIngredients.Contains(ingredientName));

			return new
			{
				Receipt = receipt,
				Protein = protein ?? 0,
				Fat = fat ?? 0,
				Carbs = carbs ?? 0,
				Kcal = kcal ?? 0,
				MissingIngredientsCount = missingIngredientsCount
			};
		}).ToList();

		// Находим все возможные комбинации из 4 рецептов
		var combinations = FindCombinations(receiptsWithNutrition, 4);

		// Фильтруем комбинации по целевым значениям БЖУ и калорий
		var suitableCombinations = combinations.Where(comb =>
		{
			var totalProtein = comb.Sum(r => r.Protein);
			var totalFat = comb.Sum(r => r.Fat);
			var totalCarbs = comb.Sum(r => r.Carbs);
			var totalKcal = comb.Sum(r => r.Kcal);

			return IsNutritionMatch(totalProtein, totalFat, totalCarbs, (int)totalKcal,
				targetProtein, targetFat, targetCarbs, targetKcal);
		}).ToList();

		// Сортируем комбинации по общему количеству недостающих ингредиентов
		var sortedCombinations = suitableCombinations
			.OrderBy(comb => comb.Sum(r => r.MissingIngredientsCount))
			.ToList();

		// Выбираем комбинацию с учетом skipDays
		if (!sortedCombinations.Any())
		{
			return (new List<ReceiptEntity>(), new List<IngredientEntity>());
		}

		int daysReady = sortedCombinations.Count;
		skipDays = skipDays % daysReady;

		var selectedCombination = sortedCombinations.Skip(skipDays).FirstOrDefault();

		if (selectedCombination == null)
		{
			return (new List<ReceiptEntity>(), new List<IngredientEntity>());
		}

		var selectedReceipts = selectedCombination.Select(r => r.Receipt).ToList();

		// Находим недостающие ингредиенты для выбранной комбинации рецептов
		var missingIngredients = FindMissingIngredients(selectedReceipts, availableIngredients, ingredientDict);

		return (selectedReceipts, missingIngredients);
	}
	*/
		// Вспомогательный метод для нахождения всех комбинаций из k элементов
	private List<List<T>> FindCombinations<T>(List<T> items, int k)
	{
		var result = new List<List<T>>();
		var current = new List<T>();
		GenerateCombinations(items, k, 0, current, result);
		return result;
	}

	private void GenerateCombinations<T>(List<T> items, int k, int start, List<T> current, List<List<T>> result)
	{
		if (current.Count == k)
		{
			result.Add(new List<T>(current));
			return;
		}

		for (int i = start; i < items.Count; i++)
		{
			current.Add(items[i]);
			GenerateCombinations(items, k, i + 1, current, result);
			current.RemoveAt(current.Count - 1);
		}
	}

	private (float? protein, float? fat, float? carbs, int? kcal, List<string> missingIngredients)
		CalculateReceiptNutrition(ReceiptEntity receipt, Dictionary<string, IngredientEntity> ingredientDict)
	{
		float? totalProtein = 0;
		float? totalFat = 0;
		float? totalCarbs = 0;
		int totalKcal = 0;
		var missingIngredients = new List<string>();

		foreach (var (ingredientName, amount) in receipt.IngredientsAmount)
		{
			if (ingredientDict.TryGetValue(ingredientName, out var ingredient))
			{
				// Конвертируем количество в граммы/миллилитры для расчета
				var convertedAmount = ConvertToGrams(amount, ingredient.MeasurementUnit);

				// Рассчитываем показатели для данного количества ингредиента
				totalProtein += (ingredient.Protein * convertedAmount) / 100f;
				totalFat += (ingredient.Fat * convertedAmount) / 100f;
				totalCarbs += (ingredient.Carbs * convertedAmount) / 100f;
				totalKcal += (int)((ingredient.Kcal * convertedAmount) / 100f);
			}
			else
			{
				missingIngredients.Add(ingredientName);
			}
		}

		return (totalProtein, totalFat, totalCarbs, totalKcal, missingIngredients);
	}

	private float ConvertToGrams(int amount, IngredientEntity.IngredientMeasurementUnits? unit)
	{
		return unit switch
		{
			IngredientEntity.IngredientMeasurementUnits.g => amount,
			IngredientEntity.IngredientMeasurementUnits.ml => amount, // для воды и жидкостей 1ml ≈ 1g
			IngredientEntity.IngredientMeasurementUnits.pcs => amount * 100f, // предполагаем средний вес 100г на штуку
			_ => amount
		};
	}

	private bool IsNutritionMatch(float protein, float fat, float carbs, int kcal,
								 float targetProtein, float targetFat, float targetCarbs, int targetKcal)
	{
		// Допуск 20% от целевых значений
		const float tolerance = 0.2f;

		return Math.Abs(protein - targetProtein) <= targetProtein * tolerance &&
			   Math.Abs(fat - targetFat) <= targetFat * tolerance &&
			   Math.Abs(carbs - targetCarbs) <= targetCarbs * tolerance &&
			   Math.Abs(kcal - targetKcal) <= targetKcal * tolerance;
	}

	private List<IngredientEntity> FindMissingIngredients(
		List<ReceiptEntity> receipts,
		List<string> availableIngredients,
		Dictionary<string, IngredientEntity> ingredientDict)
	{
		var missingIngredientNames = new HashSet<string>();

		foreach (var receipt in receipts)
		{
			foreach (var ingredientName in receipt.IngredientsAmount.Keys)
			{
				if (!availableIngredients.Contains(ingredientName) &&
					ingredientDict.ContainsKey(ingredientName))
				{
					missingIngredientNames.Add(ingredientName);
				}
			}
		}

		return missingIngredientNames
			.Where(name => ingredientDict.ContainsKey(name))
			.Select(name => ingredientDict[name])
			.ToList();
	}
}
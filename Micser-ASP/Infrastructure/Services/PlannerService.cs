using Microsoft.EntityFrameworkCore;
using tbank_back_web.Core.Data_Entities.Business;
using tbank_back_web.Infrastructure.DbContext;

public class PlannerService
{
	private readonly ApplicationDbContext _context;

	public PlannerService(ApplicationDbContext context)
	{
		_context = context;
	}

	public async Task<(List<ReceiptEntity>, List<IngredientEntity>)> FindReceipts(
		List<string> availableIngredients,
		float targetProtein, float targetFat, float targetCarbs, int targetKcal, int skipDays = 0)
	{
		// Получаем все рецепты из базы данных
		var allReceipts = await _context.Receipts
			.Include(r => r.IngredientsAmount) // Если используется Lazy Loading, это может не понадобиться
			.ToListAsync();

		// Получаем все ингредиенты из базы данных
		var allIngredients = await _context.Ingredients.ToListAsync();

		// Словарь для быстрого поиска ингредиентов по названию
		var ingredientDict = allIngredients.ToDictionary(i => i.Title, i => i);

		// Список для подходящих рецептов
		var suitableReceipts = new List<(ReceiptEntity receipt, float? protein, float? fat, float? carbs, int? kcal, int missingIngredientsCount)>();

		foreach (var receipt in allReceipts)
		{
			// Рассчитываем суммарные показатели рецепта
			var (totalProtein, totalFat, totalCarbs, totalKcal, missingIngredients) = CalculateReceiptNutrition(receipt, ingredientDict);

			// Проверяем, соответствуют ли показатели целевым значениям (с допуском ±10%)
			if (IsNutritionMatch((float)totalProtein, (float)totalFat, (float)totalCarbs, (int)totalKcal,
				targetProtein, targetFat, targetCarbs, targetKcal))
			{
				// Подсчитываем количество недостающих ингредиентов
				var missingIngredientsCount = receipt.IngredientsAmount
					.Keys
					.Count(ingredientName => !availableIngredients.Contains(ingredientName));

				suitableReceipts.Add((receipt, totalProtein, totalFat, totalCarbs, totalKcal, missingIngredientsCount));
			}
		}

		// Сортируем по количеству недостающих ингредиентов (от меньшего к большему)
		int daysReady = suitableReceipts.Count()/4;
		skipDays = skipDays % daysReady;
		var topReceipts = suitableReceipts
			.OrderBy(x => x.missingIngredientsCount)
			.Skip(skipDays)
			.Take(4)
			.Select(x => x.receipt)
			.ToList();


		// Находим недостающие ингредиенты для выбранных рецептов
		var _missingIngredientes = FindMissingIngredients(topReceipts, availableIngredients, ingredientDict);

		return (topReceipts, _missingIngredientes);
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
		// Допуск ±10% от целевых значений
		const float tolerance = 0.1f;

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
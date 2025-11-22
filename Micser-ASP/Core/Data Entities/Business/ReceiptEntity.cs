using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace tbank_back_web.Core.Data_Entities.Business
{
	public class ReceiptEntity
	{

		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public Guid Id { get; set; }
		public string? Title { get; set; }
		public string? Instructions { get; set; }
		
		[NotMapped]
		public Dictionary<string, int> IngredientsAmount { get; set; } = new();

		[Column("IngredientsAmount")]
		public string IngredientsAmountJson
		{
			get => JsonSerializer.Serialize(IngredientsAmount);
			set => IngredientsAmount = string.IsNullOrEmpty(value)
				? new Dictionary<string, int>()
				: JsonSerializer.Deserialize<Dictionary<string, int>>(value);
		}


		// Функция для получения суммы питательных веществ
		[JsonIgnore]
		[NotMapped]
		public NutritionSummary TotalNutrition
		{
			get
			{
				return CalculateTotalNutrition();
			}
		}

		// Основная функция расчета
		public NutritionSummary CalculateTotalNutrition(List<IngredientEntity> availableIngredients = null)
		{
			var summary = new NutritionSummary();

			if (IngredientsAmount == null || !IngredientsAmount.Any())
				return summary;

			foreach (var ingredientEntry in IngredientsAmount)
			{
				var ingredientName = ingredientEntry.Key;
				var amount = ingredientEntry.Value;

				// Поиск ингредиента по названию
				var ingredient = availableIngredients?.FirstOrDefault(i =>
					i.Title?.Equals(ingredientName, StringComparison.OrdinalIgnoreCase) == true);

				if (ingredient != null)
				{
					// Расчет коэффициента для преобразования единиц измерения
					float multiplier = GetUnitMultiplier(ingredient.MeasurementUnit, amount);

					summary.TotalProtein += (ingredient.Protein ?? 0) * multiplier;
					summary.TotalFat += (ingredient.Fat ?? 0) * multiplier;
					summary.TotalCarbs += (ingredient.Carbs ?? 0) * multiplier;
					summary.TotalKcal += (ingredient.Kcal ?? 0) * multiplier;
				}
			}

			return summary;
		}

		// Вспомогательная функция для преобразования единиц измерения
		private float GetUnitMultiplier(IngredientEntity.IngredientMeasurementUnits? unit, int amount)
		{
			if (unit == null) return amount;

			return unit switch
			{
				IngredientEntity.IngredientMeasurementUnits.g => amount / 100f, // предположим, что питательные вещества указаны на 100г
				IngredientEntity.IngredientMeasurementUnits.ml => amount / 100f, // предположим, что питательные вещества указаны на 100мл
				IngredientEntity.IngredientMeasurementUnits.pieces => amount,
				_ => amount
			};
		}

		// Упрощенная версия без параметров (требует передачи ингредиентов через параметр)
		public NutritionSummary CalculateTotalNutritionSimplified(List<IngredientEntity> ingredients)
		{
			return CalculateTotalNutrition(ingredients);
		}
	}

	// Класс для хранения итоговых значений
	public class NutritionSummary
	{
		public float TotalProtein { get; set; }
		public float TotalFat { get; set; }
		public float TotalCarbs { get; set; }
		public float TotalKcal { get; set; }



	}
}

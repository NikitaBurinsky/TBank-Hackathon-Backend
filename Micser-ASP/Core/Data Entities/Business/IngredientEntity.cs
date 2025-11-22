using System.Text.Json.Serialization;

namespace tbank_back_web.Core.Data_Entities.Business
{
	public class IngredientEntity
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public Guid Id {  get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public enum IngredientMeasurementUnits
		{
			g = 0,
			ml = 1,
			pcs = 2
		}
		public string? Title { get; set; }
		public float? Protein { get; set; }
		public float? Fat { get; set; }
		public float? Carbs { get; set; }
		public int? Kcal { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public IngredientMeasurementUnits? MeasurementUnit { get; set; }
	}
}

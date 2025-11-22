using System.Text.Json.Serialization;

namespace tbank_back_web.Core.Data_Entities.Business
{
	public class IngredientEntity
	{
		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public Guid Id {  get; set; }
		public enum IngredientMeasurementUnits
		{
			g = 0,
			ml = 1,
			pieces = 2
		}
		public string Title { get; set; }
		public float Protein { get; set; }
		public float Fat { get; set; }
		public float Carbs { get; set; }
		public int Kcal { get; set; }
		public IngredientMeasurementUnits MeasurementUnit { get; set; }
	}
}

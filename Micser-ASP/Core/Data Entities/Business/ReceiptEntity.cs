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
	}
}

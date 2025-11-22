using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json.Serialization;

namespace tbank_back_web.Core.Data_Entities.Business
{
	public class ReceiptEntity
	{

		[JsonIgnore(Condition = JsonIgnoreCondition.Always)]
		public Guid Id { get; set; }
		public string Title { get; set; }
		public string Instructions { get; set; }
		public List<string> Ingredients { get; set; }
	}
}

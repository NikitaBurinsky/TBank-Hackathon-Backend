using static tbank_back_web.Controllers.Finder.Models.Dto.ReceiptResponseModel;

namespace tbank_back_web.Controllers.Finder.Models.Dto
{
	public class ReceiptResponseModel
	{
		public class ReceiptComponent
		{
			public string title { get; set; }
			public int amount { get; set; }
		} 
		public string? title { get; set; }
		public string? instructions { get; set; }
		public List<ReceiptComponent> ingridients { get; set; }
	}

	public static class DictionaryExtensions
	{
		public static List<ReceiptComponent> ToReceiptComponents(this Dictionary<string, int> dictionary)
		{
			return dictionary?
				.Select(kvp => new ReceiptComponent
				{
					title = kvp.Key,
					amount = kvp.Value
				})
				.ToList()
				?? new List<ReceiptComponent>();
		}
	}
}

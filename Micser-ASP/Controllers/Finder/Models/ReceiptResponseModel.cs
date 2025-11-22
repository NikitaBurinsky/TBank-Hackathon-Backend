using static tbank_back_web.Controllers.Finder.Models.ReceiptResponseModel;

namespace tbank_back_web.Controllers.Finder.Models
{
	public class ReceiptResponseModel
	{
		public class ReceiptComponent
		{
			public string ingridient { get; set; }
			public int amount { get; set; }
		} 
		public string title { get; set; }
		public string instructions { get; set; }
		public List<ReceiptComponent> components { get; set; }
	}

	public static class DictionaryExtensions
	{
		public static List<ReceiptComponent> ToReceiptComponents(this Dictionary<string, int> dictionary)
		{
			return dictionary?
				.Select(kvp => new ReceiptComponent
				{
					ingridient = kvp.Key,
					amount = kvp.Value
				})
				.ToList()
				?? new List<ReceiptComponent>();
		}
	}
}

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace tbank_back_web.Core.Data_Entities.Business
{
	public class ReceiptEntity
	{
		public string Title { get; set; }
		public string Instructions { get; set; }
		public List<string> Ingredients { get; set; }
	}
}

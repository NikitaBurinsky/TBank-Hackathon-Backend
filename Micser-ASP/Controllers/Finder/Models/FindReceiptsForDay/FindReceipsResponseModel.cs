namespace tbank_back_web.Controllers.Finder.Models.FindReceiptsForDay
{
	public class FindReceipsRequestModel
	{
		public List<string> Titles { get; set; }
	}

	public class FindReceipsResponseModel
	{
		public ReceiptResponseModel brekfast { get; set; }
		public ReceiptResponseModel lunch { get; set; }
		public ReceiptResponseModel dinner { get; set; }
		public ReceiptResponseModel snack { get; set; }
		public List<IngredientResponseModel> ingridientsToBuy { get; set; }
	}

}

using tbank_back_web.Controllers.Finder.Models.FindReceiptsForDay;

namespace tbank_back_web.Controllers.Finder.Models.FindReceiptsForMonth
{
	public class FindReceipsForMonthResponseModel
	{
		public List<FindReceipsResponseModel> Titles { get; set; }
		public int days { get; set; }
	}
}

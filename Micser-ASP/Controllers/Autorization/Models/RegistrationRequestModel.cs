
using static tbank_back_web.Core.Identity.User.BaseApplicationUser;

namespace ZooStores.Web.Area.Identity.Autorization.Models
{
    public class RegistrationRequestModel
    {
		public string Login { get; set; }
		public string Password { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public float Heigth { get; set; }
		public float Weight { get; set; }
		public ActivityLevelE ActivityLevel { get; set; }
	}
}

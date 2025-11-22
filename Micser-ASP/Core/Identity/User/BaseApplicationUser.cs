using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace tbank_back_web.Core.Identity.User
{
    public class BaseApplicationUser : IdentityUser<int>
    {
		public enum ActivityLevelE
		{
			VeryLow = 1,
			Low = 2,
			Medium = 3,
			MediumHigh = 4,
			VeryHigh = 5,
		}

		public bool IsGuest { get; set; } = false;
		public int Age { get; set; }
		public string Gender { get; set; }
		public float Heigth { get; set; }
		public float Weight { get; set; }
		public ActivityLevelE ActivityLevel { get; set; }
	}

    public class BaseApplicationUserEntityConfigurator : IEntityTypeConfiguration<BaseApplicationUser>
    {
        public virtual void Configure(EntityTypeBuilder<BaseApplicationUser> builder)
        {
        }
    }

}

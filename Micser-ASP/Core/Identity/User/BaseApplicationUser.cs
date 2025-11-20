using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace tbank_back_web.Core.Identity.User
{
    public class BaseApplicationUser : IdentityUser<int>
    {
		public bool IsGuest { get; set; } = false;
    }

    public class BaseApplicationUserEntityConfigurator : IEntityTypeConfiguration<BaseApplicationUser>
    {
        public virtual void Configure(EntityTypeBuilder<BaseApplicationUser> builder)
        {
        }
    }

}

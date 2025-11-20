using tbank_back_web.Core.Identity.Role;
using tbank_back_web.Core.Identity.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zoobee.Domain.DataEntities.Media;

namespace tbank_back_web.Infrastructure.DbContext
{
	public class ApplicationDbContext : IdentityDbContext<BaseApplicationUser, ApplicationRole, int>
	{
		public DbSet<MediaFileEntity> MediaFiles { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{
			Database.EnsureCreated();
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			UseEntitiesConfigurators(modelBuilder);
			modelBuilder.Entity<BaseApplicationUser>(entity =>
			{
				entity.Property(u => u.Email).IsRequired(false);
				entity.Property(u => u.NormalizedEmail).IsRequired(false);

				// Делаем UserName обязательным и уникальным
				entity.Property(e => e.UserName).IsRequired(true);
				entity.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
			});
		}

		protected void UseEntitiesConfigurators(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new MediaFileEntityConfigurator());
			builder.ApplyConfigurationsFromAssembly(typeof(BaseApplicationUser).Assembly);
		}
	}
}

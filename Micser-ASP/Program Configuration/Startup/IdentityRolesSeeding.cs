using tbank_back_web.Core.Identity.Role;
using tbank_back_web.Core.Identity.User;
using Microsoft.AspNetCore.Identity;
using Serilog;


namespace Zoobee.Web.ProgramConfigurators
{
    public static class IdentityRolesSeeder 
	{
		public static async void RolesSeeding(this WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var logger = services.GetRequiredService<ILogger<WebApplication>>();
				
				var rolesManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
				var userManager = services.GetRequiredService<UserManager<BaseApplicationUser>>();
				await InitializeAsync(userManager, rolesManager);
				var roles = rolesManager.Roles.ToList();
				logger.LogInformation("Seeding Roles succeded: {@Roles}", roles);

			}
		}

		private static async Task InitializeAsync(UserManager<BaseApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
		{
			if (await roleManager.FindByNameAsync("auth_customer") == null)
				await roleManager.CreateAsync(new ApplicationRole("auth_customer"));

			if (await roleManager.FindByNameAsync("guest") == null)
				await roleManager.CreateAsync(new ApplicationRole("guest"));
		}


	}
}

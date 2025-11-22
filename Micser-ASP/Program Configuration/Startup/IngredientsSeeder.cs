using Microsoft.AspNetCore.Identity;
using tbank_back_web.Core.Identity.Role;
using tbank_back_web.Core.Identity.User;
using tbank_back_web.Infrastructure.DbContext;

namespace tbank_back_web.Program_Configuration.Startup
{
	public static class IngredientsSeeder
	{
		public static async void IngredientsSeeding(this WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var logger = services.GetRequiredService<ILogger<WebApplication>>();
				var seeder = services.GetRequiredService<JsonSeedingService>();
				var db = services.GetRequiredService<ApplicationDbContext>();

				var ingredients = await seeder.ReadIngredientsFromJsonAsync();
				var recepes = await seeder.ReadReceiptsFromJsonAsync();

				logger.LogInformation("Readed Ingredients : {@Ingredients}", ingredients);
				logger.LogInformation("Readed Receips : {@Receips}", ingredients);

				try
				{
					db.Ingredients.AddRange(ingredients);
					await db.SaveChangesAsync();


					db.Receipts.AddRange(recepes);
					await db.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Seeding caused exception : ");
				}

			}
		}
	}

}

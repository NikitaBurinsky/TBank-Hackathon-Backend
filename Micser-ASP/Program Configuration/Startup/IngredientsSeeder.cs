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

				try
				{
				var seeder = services.GetRequiredService<JsonSeedingService>();
				var db = services.GetRequiredService<ApplicationDbContext>();
					var summator = services.GetRequiredService<NutrientsSummarizerService>();
				var ingredients = await seeder.ReadIngredientsFromJsonAsync();
				var recepes = await seeder.ReadReceiptsFromJsonAsync();

				logger.LogInformation("Readed Ingredients : {@Ingredients}", ingredients);
				logger.LogInformation("Readed Receips : {@Receips}", ingredients);

					foreach(var ing in ingredients)
					{
						if (!db.Ingredients.Any(e => e.Title.ToLower().Trim() == ing.Title.ToLower().Trim()))
							await db.Ingredients.AddAsync(ing);

					}
					await db.SaveChangesAsync();

					foreach (var rec in recepes)
					{
						rec.TotalProtein = summator.GetProteinSumm(rec);
						rec.TotalFat = summator.GetFatSumm(rec);
						rec.TotalKcal = summator.GetKcalSumm(rec);  
						rec.TotalCarbs = summator.GetCarbsSumm(rec);
						if (!db.Receipts.Any(e => e.Title.ToLower().Trim() == rec.Title.ToLower().Trim()))
							await db.Receipts.AddAsync(rec);
					}
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

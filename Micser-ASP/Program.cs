using tbank_back_web.Core.Identity.Role;
using tbank_back_web.Core.Identity.User;
using tbank_back_web.Infrastructure.DbContext;
using tbank_back_web.Pipeline.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Zoobee.Infrastructure.Repositoties.MediaStorage;
using Zoobee.Web.ProgramConfigurators;
using static AlalysisService.AnalysisService;
using tbank_back_web.Program_Configuration.Startup;
using tbank_back_web.Infrastructure.Services;

internal class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();
		AddLoggingSerilog(builder);
		AddGrpsSystems(builder.Services, builder.Configuration);
		AddRepositories(builder.Services);
		AddServices(builder.Services);
		AddAuthorisationIdentity(builder.Services);
		AddDbContext(builder.Services, builder.Configuration);
		ConfigureCors(builder.Services);
		ConfigureCookies(builder.Services);
		
		var app = builder.Build();

		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}
		ApplyMigrations(app);
		app.UseCors("StrictPolicy");
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
		app.RolesSeeding();
		app.IngredientsSeeding();
		//app.ReceiptsSeeding();
		using (var scope = app.Services.CreateScope())
		{
			var logger = scope.ServiceProvider.GetService<ILogger<WebApplication>>();
			logger.LogInformation("APPLICATION STARTED");
		}
		app.Run();

		static void AddServices(IServiceCollection services)
		{
			services.AddScoped<MediaStorageService>();
			services.AddScoped<JsonSeedingService>();
			services.AddScoped<PlannerService>();
			services.AddScoped<NutrientsSummarizerService>();
		}

		static void AddRepositories(IServiceCollection services)
		{
			services.AddScoped<FileStorageRepository>();
			services.AddScoped<MediaFileRepository>();
		}

		static void AddLoggingSerilog(WebApplicationBuilder builder)
		{
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Console(LogEventLevel.Information)
				.WriteTo.File($"{builder.Configuration["Logging:LogsStorage:LogsFolderPath"]}/Logs/log-{DateTime.Now}.txt")
				.ReadFrom.Configuration(builder.Configuration)
				.CreateLogger();

			builder.Host.UseSerilog();
		}

		static void AddDbContext(IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(o =>
			{
				o.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
					b =>
					{
						b.MigrationsAssembly(assemblyName: Assembly.GetExecutingAssembly().FullName);
					});
			});
		}

		static void ConfigureCors(IServiceCollection services)
		{
			//ïðîäàêøí
			services.AddCors(options =>
			{
				options.AddPolicy("StrictPolicy", policy =>
				{
					policy.WithOrigins("https://tbank-front.vercel.app", "http://localhost") // Òîëüêî îäèí äîìåí
						  .AllowAnyMethod()
						  .AllowAnyHeader()
						  .AllowCredentials();
				});
			});
			//äåâ
			/*
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAllPolicy", policy =>
				{
					policy.SetIsOriginAllowed(e => true)   // Ïðèíèìàåò îò ëþáîãî äîìåíà
						  .AllowAnyMethod()   // Ðàçðåøàåò ëþáûå HTTP-ìåòîäû
						  .AllowAnyHeader()   // Ðàçðåøàåò ëþáûå çàãîëîâêè
						  .AllowCredentials(); // Ðàçðåøàåò ó÷åòíûå äàííûå
				});
			});
			*/
		}

		static void AddGrpsSystems(IServiceCollection services, IConfiguration configuration)
		{
			/*services.AddGrpcClient<AnalysisServiceClient>(options =>
			{
				options.Address = new Uri("http://micser-2:5000");
			})
			.ConfigurePrimaryHttpMessageHandler(() =>
			{
				var handler = new HttpClientHandler();
				handler.ServerCertificateCustomValidationCallback =
					HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
				return handler;
			});*/
		}

		static void ConfigureCookies(IServiceCollection services)
		{
			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.SameSite = SameSiteMode.None; // Ðàçðåøàåò îòïðàâêó êóêè ñ cross-site çàïðîñàìè.
				options.Cookie.Name = "AuthWOW"; // Èìÿ êóêè
				options.ExpireTimeSpan = TimeSpan.FromDays(7); // Âðåìÿ æèçíè
				options.SlidingExpiration = true; // Îáíîâëÿòü âðåìÿ æèçíè ïðè àêòèâíîñòè
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				options.Cookie.Domain = ".development0.xyz";
				options.Cookie.HttpOnly = false;
			});

			services.AddAntiforgery(o =>
			{
				o.Cookie.Domain = ".development0.xyz";
				o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				o.Cookie.SameSite = SameSiteMode.None;
				o.HeaderName = "X-XSRF-TOKEN";
			});
		}

		static void AddAuthorisationIdentity(IServiceCollection services)
		{
			services.AddIdentity<BaseApplicationUser, ApplicationRole>(options =>
			{
				// Îòêëþ÷àåì òðåáîâàíèå ïîäòâåðæäåíèÿ email
				options.SignIn.RequireConfirmedAccount = false;
				options.SignIn.RequireConfirmedEmail = false;

				// Íàñòðàèâàåì òðåáîâàíèÿ ê èìåíè ïîëüçîâàòåëÿ
				options.User.RequireUniqueEmail = false; // Âàæíî!
														 // Íàñòðîéêè ïàðîëÿ (ïî æåëàíèþ)
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 3;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = false;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders()
			.AddRoleManager<RoleManager<ApplicationRole>>()
			.AddUserManager<UserManager<BaseApplicationUser>>();
		}

		static void ApplyMigrations(WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var logger = services.GetRequiredService<ILogger<Program>>();
				try
				{
					var context = services.GetRequiredService<ApplicationDbContext>();
					context.Database.MigrateAsync();
					logger.LogInformation("Ìèãðàöèè óñïåøíî ïðèìåíåíû");
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Ïðîèçîøëà îøèáêà ïðè ïðèìåíåíèè ìèãðàöèé");
				}
			}
		}

	}
}

using Microsoft.AspNetCore.Identity;
using tbank_back_web.Core.Identity.User;

namespace tbank_back_web.Pipeline.Middlewares
{
	/// <summary>
	/// Промежуточник для работы с гостевыми пользователями
	/// </summary>
	public class GuestUserMiddleware
	{
		private readonly RequestDelegate _next;
		public GuestUserMiddleware(RequestDelegate next)
		{
			_next = next;
		}
		public async Task InvokeAsync(HttpContext context, UserManager<BaseApplicationUser> userManager, SignInManager<BaseApplicationUser> signInManager)
		{
			// Если пользователь не аутентифицирован
			if (!context.User.Identity.IsAuthenticated)
			{
				// Создаем гостевого пользователя
				var guestUser = new BaseApplicationUser
				{
					UserName = $"guest_{Guid.NewGuid():N}",
					IsGuest = true,
				};

				var result = await userManager.CreateAsync(guestUser);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(guestUser, "guest");
					// Автоматически логиним гостя
					await signInManager.SignInAsync(guestUser, isPersistent: true);
				}
			}
			await _next(context);
		}
	}
}

using Microsoft.AspNetCore.Identity;
using tbank_back_web.Core.Identity.Role;
using tbank_back_web.Core.Identity.User;

namespace tbank_back_web.Program_Configuration.Startup
{
	public static class ReceiptSeeder
	{
		public static async void ReceiptsSeeding(this WebApplication app)
		{
			using (var scope = app.Services.CreateScope())
			{

			}
		}


	}
}

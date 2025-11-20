using Microsoft.AspNetCore.Identity;

namespace tbank_back_web.Core.Identity.Role
{
    /// <summary>
    /// Создан для соответствия BaseApplicationUser для использования Guid в качестве ключа
    /// Основной класс роли в приложении
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }
}

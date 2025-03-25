using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace myAISapi.Attributes
{
	public class AuthorizeAttribute : TypeFilterAttribute
	{
		public string RoleName { get; set; }
		public string ActionValue { get; set; }

		public AuthorizeAttribute(string roleName, string actionValue) : base(typeof(AuthorizeFilter))
		{
			RoleName = roleName;
			ActionValue = actionValue;
			Arguments = new object[] { RoleName, ActionValue };
		}

		public class AuthorizeFilter : IAuthorizationFilter
		{
			public string RoleName { get; set; }
			public string ActionValue { get; set; }

			public AuthorizeFilter(string roleName, string actionValue)
			{
				RoleName = roleName;
				ActionValue = actionValue;
			}

			public void OnAuthorization(AuthorizationFilterContext context)
			{
				// Kiểm tra xem người dùng đã đăng nhập hay chưa
				if (!context.HttpContext.User.Identity.IsAuthenticated)
				{
					context.Result = new UnauthorizedResult(); // Hoặc ForbidResult
					return;
				}

				// Kiểm tra role
				if (!CanAccessToAction(context.HttpContext))
				{
					context.Result = new ForbidResult();
				}
			}

			private bool CanAccessToAction(HttpContext httpContext)
			{
				// Lấy danh sách các role từ claim "role"
				var roles = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role)
									  .Select(c => c.Value);

				// Kiểm tra xem role của người dùng có nằm trong danh sách các role được phép hay không
				return roles.Contains(RoleName);
			}
		}
	}
}

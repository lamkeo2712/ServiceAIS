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
				if (!context.HttpContext.User.Identity.IsAuthenticated)
				{
					context.Result = new UnauthorizedResult();
					return;
				}

				if (!CanAccessToAction(context.HttpContext))
				{
					context.Result = new ForbidResult();
				}
			}

			private bool CanAccessToAction(HttpContext httpContext)
			{
				var roles = httpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role)
									  .Select(c => c.Value);

				return roles.Contains(RoleName);
			}
		}
	}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CIResearch.Middleware
{
    /// <summary>
    /// Attribute để yêu cầu authentication cho các action
    /// </summary>
    public class RequireAuthenticationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var username = context.HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                // Lưu URL hiện tại để quay lại sau khi đăng nhập
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;

                // Log để debug
                Console.WriteLine($"🔍 RequireAuthentication: Redirecting to login with returnUrl: {returnUrl}");

                context.Result = new RedirectToActionResult("Login", "LoginRegister", new { returnUrl = returnUrl });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
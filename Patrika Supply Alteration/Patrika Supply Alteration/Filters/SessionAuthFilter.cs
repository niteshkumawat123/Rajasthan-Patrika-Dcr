using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DCRSupplyApp.Services;

namespace DCRSupplyApp.Filters;

public class SessionAuthFilter : IActionFilter
{
    private readonly SessionService _sessionService;

    public SessionAuthFilter(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        if (string.Equals(controller, "Account", StringComparison.OrdinalIgnoreCase))
            return;

        var user = _sessionService.GetUser(context.HttpContext.Session);
        if (user == null)
        {
            context.Result = new RedirectToActionResult("RoleSelect", "Account", null);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

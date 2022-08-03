using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace RadixDlt.NetworkGateway.Frontend.Endpoints;

public class InvalidModelStateFilter : IActionFilter, IOrderedFilter
{
    public int Order => -3000;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Result == null && !context.ModelState.IsValid)
        {
            var validationErrorHandler = context.HttpContext.RequestServices.GetRequiredService<IValidationErrorHandler>();

            context.Result = validationErrorHandler.GetClientError(context);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }
}

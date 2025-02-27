using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppService.Products
{
    public class FluentValidationFiltre : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)// delegate metodların değerlerini ve referanslarıı saklayan bir veri türüdür.
        {
            if (!context.ModelState.IsValid)
            {

                var errors = context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();

                var resultModel = ServiceResult.Faildd(errors);

                context.Result = new BadRequestObjectResult(resultModel);
                return;
            }
            await next();

        }
    }
}

using AppRepository.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppService.Filters
{
    public class NotFoundFilter<T,TId>(IGenericRepository<T,TId> genericRepository) :
        IAsyncActionFilter where T : class where TId : struct
    {
       
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idValue = context.ActionArguments.TryGetValue("id", out var idAsObject) ? idAsObject 
                : null;

            if (idAsObject is not TId id)
            {
                await next();
                return;
            }
          
            if (await genericRepository.AnyAsync(id))
            {
                await next();
                return;
            }
            var entityName = typeof(T).Name;
            //action method name
            var actionName = context.ActionDescriptor.DisplayName;
            var result = ServiceResult.Faild($"{entityName} with id: {id} not found in {actionName}", System.Net.HttpStatusCode.NotFound);
            context.Result = new NotFoundObjectResult(result);
           
        }
    }
}

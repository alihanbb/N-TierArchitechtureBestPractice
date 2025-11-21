using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AppService.ExceptionHandlers
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Beklenmeyen hata oluştu! İstek: {RequestMethod} {RequestPath}, Kullanıcı: {User}, IP: {IP}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.User?.Identity?.Name ?? "Anonim",
                httpContext.Connection.RemoteIpAddress?.ToString());

            var errorAsDto = ServiceResult.Faild(exception.Message,HttpStatusCode.InternalServerError);

            httpContext.Response.StatusCode = (int)errorAsDto.StatusCode;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(errorAsDto, cancellationToken);
            return true;
        }
    }
}

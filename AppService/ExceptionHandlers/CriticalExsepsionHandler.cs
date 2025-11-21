using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AppService.ExceptionHandlers
{
    public class CriticalExsepsionHandler(ILogger<CriticalExsepsionHandler> logger) : IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if(exception is CriticalException)
            {
                logger.LogCritical(exception, "KRİTİK HATA! Kullanıcı: {User}, IP: {IP}, Hata Mesajı: {Message}", 
                    httpContext.User?.Identity?.Name ?? "Anonim",
                    httpContext.Connection.RemoteIpAddress?.ToString(),
                    exception.Message);
                Console.WriteLine("Kritik hata ile ilgili SMS gönderildi");
            }
            return ValueTask.FromResult(false);
        }
    }
}

using AppService;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AppApis.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class CustomBaseController : ControllerBase
    {
        [NonAction]
        public IActionResult CreateActionResult<T>(ServiceResult<T> serviceResult)
        {
            if (serviceResult.StatusCode == HttpStatusCode.NoContent)
            {
                return new ObjectResult(null) { StatusCode = (int)serviceResult.StatusCode };
            }
            if(serviceResult.StatusCode == HttpStatusCode.Created)
            {
                return Created(serviceResult.Url, serviceResult);
            }
            return new ObjectResult(serviceResult) { StatusCode = (int)serviceResult.StatusCode };

        }

        [NonAction]
        public IActionResult CreateActionResult(ServiceResult serviceResult)
        {
            if (serviceResult.StatusCode == HttpStatusCode.NoContent)
            {
                return new ObjectResult(null) { StatusCode = (int)serviceResult.StatusCode };
            }
            return new ObjectResult(serviceResult) { StatusCode = (int)serviceResult.StatusCode };

        }
    }
}

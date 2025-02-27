using System.Net;
using System.Text.Json.Serialization;

namespace AppService
{
    public class ServiceResult
    {
        [JsonIgnore]
        public bool IsSuccess
        {
            get{ return ErrorMessage == null || ErrorMessage.Count == 0; }
        }
        public List<string>? ErrorMessage { get; set; }

        [JsonIgnore]
        public bool Fail
        {
            get
            {
                return !IsSuccess;
            }
        }
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }
        

        public static ServiceResult Succes(HttpStatusCode statusCode=HttpStatusCode.OK)
        {
            return new ServiceResult
            {
                
                StatusCode = statusCode
            };

        }

        public static ServiceResult Faild(string message, HttpStatusCode statusCode=HttpStatusCode.BadRequest)
        {
            return new ServiceResult
            {
                ErrorMessage = [message] ,
                StatusCode = statusCode
            };
        }

        public static object? Faildd(List<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
             return new ServiceResult { ErrorMessage = errors, StatusCode = HttpStatusCode.BadRequest };        
        }
    }
    public class ServiceResult<T>
    {
        public T? Data { get; set; }
        [JsonIgnore]
        public bool IsSuccess
        {
            get { return ErrorMessage == null || ErrorMessage.Count == 0; }
        }
        [JsonIgnore]public string Url { get; set; }

        public List<string>? ErrorMessage { get; set; }
        [JsonIgnore]
        public bool Fail
        {
            get
            {
                return !IsSuccess;
            }
        }
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }


        public static ServiceResult<T> Succes(T  data,HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ServiceResult<T>
            {
                Data=data,
                StatusCode = statusCode
            };

        }

        public static ServiceResult<T> SuccessAsCreated(T data, string url)
        {
            return new ServiceResult<T>
            {
                Data = data,
                StatusCode = HttpStatusCode.Created,
                Url = url
            };
        }

        public static ServiceResult<T> Faild(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ServiceResult<T>
            {
                ErrorMessage = [message],
                StatusCode = statusCode
            };
        }

    }
}
    
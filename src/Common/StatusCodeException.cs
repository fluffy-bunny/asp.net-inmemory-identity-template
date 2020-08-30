using System;
using System.Net;

namespace Common
{
    public class StatusCodeException : Exception
    {
        public StatusCodeException(HttpStatusCode statusCode, string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}

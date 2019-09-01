using System.Net;

namespace LightestNight.System.Api
{
    public class RestExceptionDetails
    {
        /// <summary>
        /// The Status Code of the Response that failed
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
        
        /// <summary>
        /// The Error details
        /// </summary>
        public string Error { get; set; }
        
        /// <summary>
        /// A more detailed error message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// A unique error code to distinguish from other exceptions
        /// </summary>
        public string ErrorCode { get; set; }
        
        /// <summary>
        /// The full Uniform Resource Identifier that was used when making the REST request
        /// </summary>
        public string FullUri { get; set; }
    }
}
using System;
using System.Net;

namespace LightestNight.System.Api
{
    public class RestException : Exception
    {
        /// <summary>
        /// The Status Code of the Response that failed
        /// </summary>
        public HttpStatusCode StatusCode { get; }
        
        /// <summary>
        /// Any metadata associated with this Exception
        /// </summary>
        public RestExceptionMeta Meta { get; private set; }
        
        /// <summary>
        /// Any Response Content sent back with the failing Rest Response
        /// </summary>
        public string Content { get; }
        
        /// <summary>
        /// The full Uniform Resource Identifier that was used when making the REST request
        /// </summary>
        public string FullUri { get; }
        
        /// <summary>
        /// If a body was sent with the request, the contents of such body
        /// </summary>
        public object RequestBody { get; }

        /// <summary>
        /// Creates a new instance of <see cref="RestException" /> with the given message and meta
        /// </summary>
        /// <param name="fullUri">The full request URI this exception pertains to</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode" /> that was returned with the failing response</param>
        /// <param name="meta">The Exception Meta</param>
        /// <param name="requestBody">If a body was sent with the request, the contents of such body</param>
        public RestException(string fullUri, HttpStatusCode statusCode, RestExceptionMeta meta, object requestBody = null) 
            : base($"An error occurred when sending a request to '{fullUri}'. See meta for more information")
        {
            Meta = meta;
            FullUri = fullUri;
            StatusCode = statusCode;
            RequestBody = requestBody;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RestException" /> with the given message and meta
        /// </summary>
        /// <param name="fullUri">The full request URI this exception pertains to</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode" /> that was returned with the failing response</param>
        /// <param name="content">Any Response Content sent back with the failing Rest Response</param>
        /// <param name="requestBody">If a body was sent with the request, the contents of such body</param>
        public RestException(string fullUri, HttpStatusCode statusCode, string content, object requestBody = null)
            : base($"An error occurred when sending a request to '{fullUri}'. See meta for more information")
        {
            FullUri = fullUri;
            StatusCode = statusCode;
            Content = content;
            RequestBody = requestBody;
        }
    }
}
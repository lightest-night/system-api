using System;
using Newtonsoft.Json;

namespace LightestNight.System.Api
{
    public class RestException : Exception
    {
        public RestExceptionDetails Details { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="RestException" /> with the given message and details
        /// </summary>
        /// <param name="resource">The REST resource this exception pertains to</param>
        /// <param name="details">The Exception Details</param>
        public RestException(string resource, RestExceptionDetails details) : base($"An error occurred when sending a request to the '{resource}' resource. See details for more information")
        {
            Details = details;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RestException" /> with the given message and details
        /// </summary>
        /// <param name="resource">The REST resource this exception pertains to</param>
        /// <param name="details">The Exception Details</param>
        public RestException(string resource, string details) : base($"An error occurred when sending a request to the '{resource}' resource. See details for more information")
        {
            Details = JsonConvert.DeserializeObject<RestExceptionDetails>(details);
        }
    }
}
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
        /// <param name="fullUri">The full request URI this exception pertains to</param>
        /// <param name="details">The Exception Details</param>
        public RestException(string fullUri, RestExceptionDetails details) 
            : base($"An error occurred when sending a request to '{fullUri}'. See details for more information")
        {
            Details = details;
            Details.FullUri = fullUri;
        }

        /// <summary>
        /// Creates a new instance of <see cref="RestException" /> with the given message and details
        /// </summary>
        /// <param name="fullUri">The full request URI this exception pertains to</param>
        /// <param name="details">The Exception Details</param>
        public RestException(string fullUri, string details) 
            : base($"An error occurred when sending a request to '{fullUri}'. See details for more information")
        {
            Details = JsonConvert.DeserializeObject<RestExceptionDetails>(details);
            Details.FullUri = fullUri;
        }
    }
}
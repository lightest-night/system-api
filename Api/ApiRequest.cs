using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace LightestNight.System.Api
{
    public class ApiRequest
    {
        /// <summary>
        /// The Http Method to use when making the request
        /// </summary>
        public string HttpMethod { get; set; } = HttpMethods.Get;

        private bool _useJson = true;

        /// <summary>
        /// Denotes whether to use Json as the Data Format
        /// </summary>
        public bool UseJson
        {
            get => _useJson;
            set
            {
                if (value)
                    UseXml = false;

                _useJson = value;
            }
        }

        private bool _useXml;
        /// <summary>
        /// Denotes whether to use Xml as the Data Format
        /// </summary>
        public bool UseXml
        {
            get => _useXml;
            set
            {
                if (value)
                    UseJson = false;

                _useXml = value;
            }
        }
        
        /// <summary>
        /// The Resource type to request
        /// </summary>
        public string Resource { get; }
        
        /// <summary>
        /// The identifier of the resource
        /// </summary>
        public string ResourceId { get; set; }
        
        /// <summary>
        /// The edge to add to the request
        /// </summary>
        public string Edge { get; set; }

        /// <summary>
        /// A collection of Key/Value pairs to add as query parameters to the request
        /// </summary>
        public IDictionary<string, string> QueryParams { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Any Authorization information to use with the request
        /// </summary>
        public Authorization Authorization { get; set; }

        /// <summary>
        /// Denotes whether to use a Machine Token if no Authorization is set
        /// </summary>
        public bool UseMachineToken { get; set; } = true;

        /// <summary>
        /// The Http Headers to send with the Request
        /// </summary>
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// The media type of the resource
        /// </summary>
        public string ResourceMediaType { get; set; }
        
        /// <summary>
        /// The object to include in the body of the request
        /// </summary>
        /// <remarks>Will be ignored if Body is not appropriate for the <see cref="Method" /></remarks>
        public object Body { get; set; }
        
        /// <summary>
        /// The Formatted Request Uri 
        /// </summary>
        public string FormattedRequestUri
        {
            get
            {
                var uri = Resource.TrimStart('/');
                
                if (!string.IsNullOrEmpty(ResourceId))
                    uri += $"/{ResourceId.TrimStart('/')}";

                if (!string.IsNullOrEmpty(Edge))
                    uri += $"/{Edge.TrimStart('/')}";

                return uri.EndsWith("/")
                    ? uri
                    : uri += "/";
            }
        }
        
        public ApiRequest(string resource)
        {
            Resource = resource;
        }
    }
}
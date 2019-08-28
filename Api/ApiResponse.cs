using System.Collections.Generic;
using System.Linq;
using System.Net;
using RestSharp;

namespace LightestNight.System.Api
{
    public class ApiResponse
    {
        /// <summary>
        /// The <see cref="HttpStatusCode" /> of this response
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        private bool? _isSuccessful;

        /// <summary>
        /// Denotes whether or not the request was successful
        /// </summary>
        public bool IsSuccessful
        {
            get
            {
                if (!_isSuccessful.HasValue)
                    return (int) StatusCode >= 200 && (int) StatusCode <= 300;

                return _isSuccessful.Value;
            }

            set => _isSuccessful = value;
        }
        
        /// <summary>
        /// The body of the response
        /// </summary>
        public string Body { get; set; }
        
        /// <summary>
        /// The Http Headers sent back with the response
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Maps a <see cref="IRestResponse" /> object into an <see cref="ApiResponse" /> object
        /// </summary>
        /// <param name="response">The <see cref="IRestResponse" /> to map</param>
        /// <returns>A new populated instance of <see cref="ApiResponse" /></returns>
        public static ApiResponse FromRestResponse(IRestResponse response)
            => new ApiResponse
            {
                StatusCode = response.StatusCode,
                IsSuccessful = response.IsSuccessful,
                Body = response.Content,
                Headers = response.Headers.ToDictionary(param => param.Name, param => param.Value.ToString())
            };
    }

    public class ApiResponse<T> : ApiResponse
        where T : class
    {
        /// <summary>
        /// The Response Body serialized as Type <typeparamref name="T" />
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Maps a <see cref="IRestResponse{T}" /> object into an <see cref="ApiResponse{T}" /> object
        /// </summary>
        /// <param name="response">The <see cref="IRestResponse{T}" /> to map</param>
        /// <returns>A new populated instance of <see cref="ApiResponse{T}" /></returns>
        public static ApiResponse<T> FromRestResponse(IRestResponse<T> response)
            => new ApiResponse<T>
            {
                StatusCode = response.StatusCode,
                IsSuccessful = response.IsSuccessful,
                Body = response.Content,
                Headers = response.Headers.ToDictionary(param => param.Name, param => param.Value.ToString()),
                Data = response.Data
            };
    }
}
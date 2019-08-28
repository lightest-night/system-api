using System;

namespace LightestNight.System.Api
{
    public class Authorization
    {
        /// <summary>
        /// Denotes whether this Authorization should be placed in the request header
        /// </summary>
        public bool IsHeader => AuthorizationType == AuthorizationType.Basic || AuthorizationType == AuthorizationType.Bearer;
        
        /// <summary>
        /// The <see cref="AuthorizationType" /> to use with the request
        /// </summary>
        public AuthorizationType AuthorizationType { get; }
        
        /// <summary>
        /// The Access Token to include with the request
        /// </summary>
        public string AccessToken { get; }

        private string _urlProperty;

        /// <summary>
        /// The Url Property to use as the access token container
        /// </summary>
        /// <example>access_token; will result in a querystring parameter of access_token={AccessToken}</example>
        /// <exception cref="NotSupportedException">If <see cref="AuthorizationType" /> is not set to Url, or there is no UrlProperty set</exception>
        public string UrlProperty
        {
            get
            {
                if (AuthorizationType == AuthorizationType.Url && string.IsNullOrEmpty(_urlProperty))
                    throw new NotSupportedException($"Must supply a Url Property when {nameof(AuthorizationType)} is set to {AuthorizationType.Url}");

                return _urlProperty;
            }
            set
            {
                if (AuthorizationType != AuthorizationType.Url)
                    throw new NotSupportedException($"Cannot use a Url Property when {nameof(AuthorizationType)} is not set to {AuthorizationType.Url}");

                _urlProperty = value;
            }
        }

        public Authorization(AuthorizationType authorizationType, string accessToken)
        {
            AuthorizationType = authorizationType;
            AccessToken = accessToken;
        }
    }
}
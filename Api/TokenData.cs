namespace LightestNight.System.Api
{
    public class TokenData
    {
        /// <summary>
        /// The access token to be used in any requests
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The amount of seconds the <see cref="AccessToken" /> will expire in
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// The scopes attributed to this <see cref="AccessToken" />
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// The type of this <see cref="AccessToken" />
        /// </summary>
        public TokenTypes TokenType { get; set; }
    }
}
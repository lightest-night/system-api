namespace LightestNight.System.Api
{
    public class RestExceptionMeta
    {
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
    }
}
using System.ComponentModel.DataAnnotations;

namespace OVHCredentials
{
    /// <summary>
    /// Options to configure the message handler
    /// </summary>
    public class OvhCredentialsOption
    {
        /// <summary>
        /// Application key
        /// </summary>
        [Required(ErrorMessage = "Application key is required.")]
        public string ApplicationKey { get; set; }

        /// <summary>
        /// Application secret
        /// </summary>
        [Required(ErrorMessage = "Application secret is required.")]
        public string ApplicationSecret { get; set; }

        /// <summary>
        /// Consumer key
        /// </summary>
        [Required(ErrorMessage = "Consumer key is required.")]
        public string ConsumerKey { get; set; }

        /// <summary>
        /// If provided, HttpClient instance used to retrieved the remote time
        /// will be created using the configuration specified by RemoteTimeHttpClientName
        /// </summary>
        public string RemoteTimeHttpClientName { get; set; }
    }
}

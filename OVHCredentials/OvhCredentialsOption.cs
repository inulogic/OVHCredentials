namespace OVHCredentials;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Options to configure the message handler.
/// </summary>
public class OvhCredentialsOption
{
    /// <summary>
    /// Gets or sets application key.
    /// </summary>
    [Required(ErrorMessage = "Application key is required.")]
    public string ApplicationKey { get; set; }

    /// <summary>
    /// Gets or sets application secret.
    /// </summary>
    [Required(ErrorMessage = "Application secret is required.")]
    public string ApplicationSecret { get; set; }

    /// <summary>
    /// Gets or sets consumer key.
    /// </summary>
    [Required(ErrorMessage = "Consumer key is required.")]
    public string ConsumerKey { get; set; }

    /// <summary>
    /// Gets or sets if provided, HttpClient instance used to retrieved the remote time
    /// will be created using the configuration specified by RemoteTimeHttpClientName.
    /// </summary>
    public string RemoteTimeHttpClientName { get; set; }
}

namespace ApiTester;

using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

public interface ICredentialRequestOVHApi
{
    [Post("/auth/credential")]
    public Task<CredentialRequestResult> RequestConsumerKeyAsync([Body] CredentialRequest credentialRequest, [Header("X-Ovh-Application")] string applicationKey);
}

public class CredentialRequestResult
{

    public string ValidationUrl { get; set; }

    public string ConsumerKey { get; set; }
}


public class CredentialRequest
{

    public List<AccessRight> AccessRules { get; }

    public string Redirection { get; set; }

    public CredentialRequest() => this.AccessRules = new List<AccessRight>();
}

public class AccessRight
{
    public string Method { get; set; }

    public string Path { get; set; }
}

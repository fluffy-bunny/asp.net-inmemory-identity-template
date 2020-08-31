using IdentityModel.Client;

namespace oauth2.helpers
{
    public class OAuth2Credentials
    {
        public string Authority { get; set; }
        public DiscoveryDocumentResponse DiscoveryDocumentResponse { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

    }
}

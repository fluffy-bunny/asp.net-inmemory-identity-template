using System.Collections.Generic;

namespace SharedModels
{
    public class OpenIdConnectSessionDetails
    {
        public string LoginProider { get; set; }
        public Dictionary<string, string> OIDC { get; set; }
    }
}

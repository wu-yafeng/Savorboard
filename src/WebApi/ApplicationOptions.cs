using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WebApi
{
    public class ApplicationOptions
    {
        public class AuthorizeHubOptions
        {
            public required string Issuer { get; set; }

            public required IEnumerable<string> JsonWebKey { get; set; }
        }

        public required AuthorizeHubOptions Authorize { get; set; }

        private OpenIdConnectConfiguration? _configuration;
        public OpenIdConnectConfiguration BuildOidcConfiguration()
        {
            if(_configuration == null)
            {
                _configuration = new OpenIdConnectConfiguration()
                {
                    Issuer = "AuthorizeHub",
                };

                PushSigningKeys(_configuration);
            }

            return _configuration;
        }

        private void PushSigningKeys(OpenIdConnectConfiguration configuration)
        {
            foreach(var securityKey in GetSupportedSecurityKeys())
            {
                configuration.SigningKeys.Add(securityKey);
            }
        }

        public IEnumerable<JsonWebKey> GetSupportedSecurityKeys()
        {
            foreach (var jwk in Authorize.JsonWebKey)
            {
                var json = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(jwk));
                var securityKey = JsonSerializer.Deserialize<JsonWebKey>(json)!;
                yield return securityKey;
            }
        }

        public async Task<string> CreateJwk()
        {
            var securityKey = new RsaSecurityKey(RSA.Create(keySizeInBits: 2048))
            {
                KeyId = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray())
            };

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);

            jwk.Alg = "RS256";
            jwk.Use = "sig";

            using var buffer = new MemoryStream();

            await JsonSerializer.SerializeAsync(buffer, jwk);

            return WebEncoders.Base64UrlEncode(buffer.GetBuffer());
        }
    }
}

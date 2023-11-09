using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace WebApi
{
    public class JwtBearerOptionsConfigure : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IOptions<ApplicationOptions> _options;

        public JwtBearerOptionsConfigure(IOptions<ApplicationOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void PostConfigure(string? name, JwtBearerOptions options)
        {
            if (string.Equals(name, JwtBearerDefaults.AuthenticationScheme))
            {
                //options.Events ??= new();

                //options.Events.OnMessageReceived = context =>
                //{
                //    var access_token = context.Request.Query["access_token"];

                //    if (!string.IsNullOrEmpty(access_token))
                //    {
                //        context.Token = access_token;
                //    }

                //    return Task.CompletedTask;
                //};

                options.TokenValidationParameters ??= new();

                options.TokenValidationParameters.ValidAudiences = new[] { "GameHub" };
                options.TokenValidationParameters.IssuerSigningKey = JsonSerializer.Deserialize<JsonWebKey>(WebEncoders.Base64UrlDecode(_options.Value.JsonWebKey));
                options.TokenValidationParameters.ValidIssuers = _options.Value.Issuers;
            }
        }
    }
}

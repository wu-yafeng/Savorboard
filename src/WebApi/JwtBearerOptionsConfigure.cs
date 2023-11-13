using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace WebApi
{
    public class JwtBearerOptionsConfigure : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IOptions<ApplicationOptions> _options;

        public JwtBearerOptionsConfigure(IOptions<ApplicationOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            if (JwtBearerDefaults.AuthenticationScheme == name)
            {
                options.Configuration = _options.Value.BuildOidcConfiguration();
                options.Audience = "GameHub";

                options.Events ??= new JwtBearerEvents();

                options.Events.OnMessageReceived += (context) =>
                {
                    if (string.IsNullOrEmpty(context.Token) && !string.IsNullOrEmpty(context.Request.Query["access_token"]))
                    {
                        context.Token = context.Request.Query["access_token"];

                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            Configure(Options.DefaultName, options);
        }
    }
}

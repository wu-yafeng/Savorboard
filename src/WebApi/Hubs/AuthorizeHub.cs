using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebApi.Protocols;

namespace WebApi.Hubs
{
    public class AuthorizeHub : Hub<IAuthorizeHubClient>, IAuthorizeHub
    {
        private readonly ApplicationOptions _options;
        public record GamePlayer(long Id, int UserId, int ServerId)
        {
            public IEnumerable<Claim> ToClaims()
            {
                yield return new Claim(JwtRegisteredClaimNames.Sub, Id.ToString());

                yield return new Claim("server_id", ServerId.ToString());

                yield return new Claim("user_id", UserId.ToString());
            }
        }
        private readonly List<GamePlayer> _players = new()
        {
            { new GamePlayer(1001,1,1) },
            { new GamePlayer(2001,1,2) },
            { new GamePlayer(1002,2,1) },
            { new GamePlayer(2002,2,2) },
        };

        public AuthorizeHub(IOptions<ApplicationOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        private SigningCredentials SelectBestCandicate(string? keyId = null)
        {
            var supportedKeys = _options.GetSupportedSecurityKeys().ToArray();

            JsonWebKey? securityKey = null;

            if (supportedKeys.Length == 0)
            {
                throw new NotSupportedException("No any signing keys found.");
            }

            if (string.IsNullOrEmpty(keyId))
            {
                securityKey = supportedKeys.FirstOrDefault();
            }
            else
            {
                securityKey = supportedKeys.FirstOrDefault(x => x.KeyId == keyId);
            }

            if (securityKey == null)
            {
                throw new InvalidOperationException($"Could not found key '{keyId}'.");
            }

            return new SigningCredentials(securityKey, securityKey.Alg);
        }

        private string CreateAcessToken(GamePlayer player, string audience = "GameHub", string? keyId = null)
        {
            var configuration = _options.BuildOidcConfiguration();

            var handler = new JwtSecurityTokenHandler();

            // create security token
            var credential = SelectBestCandicate(keyId);
            var header = new JwtHeader(credential);

            var currentTime = DateTime.UtcNow;
            var issuer = configuration.Issuer;
            var payload = new JwtPayload(issuer, audience, player.ToClaims(), currentTime, currentTime.AddHours(1), currentTime);

            var token = new JwtSecurityToken(header, payload);

            return handler.WriteToken(token);
        }

        public async Task SignInAsync(SignInReq context)
        {
            if (string.IsNullOrEmpty(context.Password))
            {
                await Clients.Caller.OnFailed(1, "incorrect password.", null);

                return;
            }

            var ply = _players.SingleOrDefault(x => x.UserId == context.UserId && x.ServerId == context.ServerId);

            if (ply == null)
            {
                await Clients.Caller.OnFailed(2, "incorrect player.", null);

                return;
            }

            var token = CreateAcessToken(ply);

            await Clients.Caller.OnSucceed(token);
        }

    }
}

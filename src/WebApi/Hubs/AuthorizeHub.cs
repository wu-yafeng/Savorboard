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
        private readonly List<PlayerTable> _players = new()
        {
            { new PlayerTable(1001,1,1) },
            { new PlayerTable(2001,1,2) },

            { new PlayerTable(1002,2,1) },
            { new PlayerTable(2002,2,2) },
        };

        public AuthorizeHub(IOptions<ApplicationOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        private static RsaSecurityKey CreateRsaSecurityKey()
        {
            return new RsaSecurityKey(RSA.Create(keySizeInBits: 2048))
            {
                KeyId = WebEncoders.Base64UrlEncode(Guid.NewGuid().ToByteArray())
            };
        }

        private static JsonWebKey CreateJsonWebKey()
        {
            var securityKey = CreateRsaSecurityKey();

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(securityKey);

            jwk.Alg = "RS256";
            jwk.Use = "sig";

            return jwk;
        }

        private async Task<string> CreateAcessToken(PlayerTable player)
        {
            using var mem = new MemoryStream();
            if (string.IsNullOrEmpty(_options.JsonWebKey))
            {
                JsonSerializer.Serialize(mem, CreateJsonWebKey());

                // for test.
                _options.JsonWebKey = WebEncoders.Base64UrlEncode(mem.GetBuffer());
            }
            else
            {
                await mem.WriteAsync(WebEncoders.Base64UrlDecode(_options.JsonWebKey));
            }

            mem.Seek(0, SeekOrigin.Begin);

            var jwk = JsonSerializer.Deserialize<JsonWebKey>(mem)!;

            var handler = new JwtSecurityTokenHandler();

            // create security token
            var credential = new SigningCredentials(jwk, jwk.Alg);
            var header = new JwtHeader(credential);

            var currentTime = DateTime.UtcNow;
            var issuer = "AuthorizeHub";
            var audience = "GameHub";
            var payload = new JwtPayload(issuer, audience, GetClaims(player.Id, player.UserId, player.ServerId), currentTime, currentTime.AddHours(1), currentTime);

            var token = new JwtSecurityToken(header, payload);

            return handler.WriteToken(token);
        }

        private static IEnumerable<Claim> GetClaims(long playerid, int userid, int serverId)
        {
            yield return new Claim(JwtRegisteredClaimNames.Sub, playerid.ToString());
            yield return new Claim("user_id", userid.ToString());
            yield return new Claim("server_id", serverId.ToString());
        }

        public async Task SignInAsync(SignInReq context)
        {
            if (string.IsNullOrEmpty(context.Password))
            {
                await Clients.Caller.OnFailed(1, "incorrect password.", null);

                return;
            }

            if (!_options.Servers.Contains(context.ServerId))
            {
                await Clients.Caller.OnFailed(2, "incorrect server.", null);

                return;
            }

            var ply = _players.FirstOrDefault(x => x.UserId == context.UserId && x.ServerId == context.ServerId);

            var token = await CreateAcessToken(ply);

            await Clients.Caller.OnSucceed(token);
        }

        public class PlayerTable
        {
            public PlayerTable(long id, int userId, int serverId)
            {
                Id = id;
                UserId = userId;
                ServerId = serverId;
            }

            public long Id { get; set; }

            public int UserId { get; set; }

            public int ServerId { get; set; }
        }
    }
}

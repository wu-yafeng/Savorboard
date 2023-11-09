using GameSdk;
using GameSdk.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Silos.Grains;
using Silos.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using WebApi;
using WebApi.Hubs;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(orleans =>
{
    orleans.UseLocalhostClustering().AddMemoryGrainStorageAsDefault();
});

builder.Services.AddGrainService<TickService>();
builder.Services.AddGrainService<MetaService>();
builder.Services.AddSingleton(typeof(IServiceClient<>), typeof(DefaultGrainServiceClient<>));

builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigure>());

builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));

var app = builder.Build();

app.UseHttpsRedirection();

//app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/GameHub").RequireAuthorization();
app.MapHub<AuthorizeHub>("/AuthorizeHub");


app.Run();

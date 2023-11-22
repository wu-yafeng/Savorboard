using GameSdk;
using GameSdk.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Silos;
using Silos.Grains;
using Silos.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using WebApi;
using WebApi.Hubs;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseSavorboard();

builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigure>());
builder.Services.Configure<ApplicationOptions>(builder.Configuration.GetSection("Application"));

var app = builder.Build();

app.MapDefaultEndpoints();

//app.UseHttpsRedirection();

//app.UseAuthentication();
app.UseAuthorization();

app.MapHub<GameHub>("/GameHub").RequireAuthorization();

app.MapHub<AuthorizeHub>("/AuthorizeHub");

app.Run();

using AzureADAuth.Filters;
using AzureADAuth.Options;
using AzureADAuth.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMvc(opts =>
{
    opts.EnableEndpointRouting = false;
    opts.Filters.Add(new AdalTokenAcquisitionExceptionFilter());
});

builder.Services.AddDataProtection();

//Add a strongly-typed options class to DI
//services.Configure<AuthOptions>(Configuration.GetSection("Authentication"));

builder.Services.AddScoped<ITokenCacheFactory, TokenCacheFactory>();

builder.Services.AddAuthentication(auth =>
{
    auth.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(opts =>
{
//    Configuration.GetSection("Authentication").Bind(opts);

    opts.Events = new OpenIdConnectEvents
    {
        OnAuthorizationCodeReceived = async ctx =>
        {
            HttpRequest request = ctx.HttpContext.Request;
                        //We need to also specify the redirect URL used
                        string currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);
                        //Credentials for app itself
                        var credential = new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

                        //Construct token cache
                        ITokenCacheFactory cacheFactory = ctx.HttpContext.RequestServices.GetRequiredService<ITokenCacheFactory>();
            TokenCache cache = cacheFactory.CreateForUser(ctx.Principal);

            var authContext = new AuthenticationContext(ctx.Options.Authority, cache);

                        //Get token for Microsoft Graph API using the authorization code
                        string resource = "https://graph.microsoft.com";
            AuthenticationResult result = await authContext.AcquireTokenByAuthorizationCodeAsync(
                ctx.ProtocolMessage.Code, new Uri(currentUri), credential, resource);

                        //Tell the OIDC middleware we got the tokens, it doesn't need to do anything
                        ctx.HandleCodeRedemption(result.AccessToken, result.IdToken);
        }
    };
});

builder.Services.Configure<HstsOptions>(o =>
{
    o.IncludeSubDomains = false;
    o.Preload = false;
    o.MaxAge = TimeSpan.FromDays(365);
});

builder.Services.AddControllersWithViews();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();

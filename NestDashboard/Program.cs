using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(o =>
{
    // This forces challenge results to be handled by Google OpenID Handler, so there's no
    // need to add an AccountController that emits challenges for Login.
    o.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    // This forces forbid results to be handled by Google OpenID Handler, which checks if
    // extra scopes are required and does automatic incremental auth.
    o.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    // Default scheme that will handle everything else.
    // Once a user is authenticated, the OAuth2 token info is stored in cookies.
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
        .AddCookie()
        .AddGoogleOpenIdConnect(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("CLIENTID");
            options.ClientSecret = Environment.GetEnvironmentVariable("SECRET");
        });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    RequireHeaderSymmetry = false
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseRouting();
app.MapControllers();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

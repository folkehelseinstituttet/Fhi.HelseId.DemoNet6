using Fhi.HelseId.Common;
using Fhi.HelseId.Web;
using Fhi.HelseId.Web.ExtensionMethods;
using Fhi.HelseId.Web.Hpr;
using Fhi.HelseId.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;

var whitelist = new Whitelist();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = builder.Configuration;

var helseIdKonfigurasjonSeksjon = configuration.GetSection("MyHelseIdKonfigurasjon");
var helseIdWebKonfigurasjon = helseIdKonfigurasjonSeksjon.Get<HelseIdWebKonfigurasjon>();

builder.Services.AddSingleton<IWhitelist>(whitelist); 
builder.Services.AddScoped<IGodkjenteHprKategoriListe, NoHprApprovals>();
builder.Services.AddAccessTokenManagement();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHelseIdWebAuthentication(
    helseIdWebKonfigurasjon,
    new RedirectPagesKonfigurasjon(),
    new HprKonfigurasjon { UseHpr = false, UseHprPolicy = false },
    whitelist,
    null,
    null
    );

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHelseIdProtectedPaths(
        helseIdWebKonfigurasjon, 
    new HprKonfigurasjon { UseHpr = false, UseHprPolicy = false },
        new RedirectPagesKonfigurasjon(),
    new List<PathString>
    {
        "/assets/favicon.ico"
    });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();

public class NoHprApprovals : GodkjenteHprKategoriListe
{
}

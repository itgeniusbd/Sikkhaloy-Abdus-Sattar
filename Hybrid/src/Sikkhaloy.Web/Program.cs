using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.App.Services;
using Sikkhaloy.LocalData;
using Sikkhaloy.LocalData.Auth;
using Sikkhaloy.LocalData.Sync;
using Sikkhaloy.Web;
using Sikkhaloy.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var baseUrl = SyncApiClient.NormalizeBaseUrl(builder.Configuration["SyncApi:BaseUrl"]);

builder.Services.AddSingleton(AppHostInfo.Web);
builder.Services.AddScoped<AppSession>();
builder.Services.AddScoped<AppLocale>();
builder.Services.AddScoped<OfflineAuthService>();
builder.Services.AddScoped<ISyncApiClient, SyncApiClient>();
builder.Services.AddScoped<SyncEngine>();
builder.Services.AddScoped<SyncLoopService>();
builder.Services.AddScoped<LocalDbPath>();
builder.Services.AddScoped<IDbContextFactory<LocalDbContext>, ScopedSqliteFactory>();
builder.Services.AddScoped<CircuitHandler, WebSessionCircuitHandler>();
builder.Services.AddHttpClient(SyncApiClient.HttpClientName, client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromMinutes(2);
}).ConfigurePrimaryHttpMessageHandler(SyncApiClient.CreateHandler);

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Sikkhaloy.App.Routes).Assembly);

app.Run();

using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sikkhaloy.App.Services;
using Sikkhaloy.Client.Updates;
using Sikkhaloy.LocalData;
using Sikkhaloy.LocalData.Auth;
using Sikkhaloy.LocalData.Sync;

namespace Sikkhaloy.Client;

public partial class App : System.Windows.Application
{
    private SyncLoopService? _syncLoop;
    private HybridUpdateService? _updater;

    public App()
    {
        var services = BuildServices();
        Resources.Add("services", services);
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = (IServiceProvider)Resources["services"];
        var dbFactory = services.GetRequiredService<IDbContextFactory<LocalDbContext>>();
        await using (var db = await dbFactory.CreateDbContextAsync())
            await LocalDbContext.EnsureSchemaAsync(db);

        _syncLoop = services.GetRequiredService<SyncLoopService>();
        _syncLoop.Start();

        var window = new MainWindow();
        window.Show();

        // Desktop auto-update: silent when offline or already up to date; shows a banner when a newer version exists.
        _updater = services.GetRequiredService<HybridUpdateService>();
        window.AttachUpdater(_updater);
        _updater.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _updater?.Dispose();
        _syncLoop?.Dispose();
        base.OnExit(e);
    }

    private static IServiceProvider BuildServices()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
#if DEBUG
        configBuilder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);
#else
        configBuilder.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: false);
#endif
        var configuration = configBuilder.Build();

        var baseUrl = SyncApiClient.NormalizeBaseUrl(configuration["SyncApi:BaseUrl"]);
#if DEBUG
        baseUrl = "http://127.0.0.1:5135/";
#endif
        var collection = new ServiceCollection();
        collection.AddWpfBlazorWebView();
#if DEBUG
        collection.AddBlazorWebViewDeveloperTools();
#endif
        collection.AddSingleton(configuration);
        collection.AddSingleton(AppHostInfo.Desktop);
        collection.AddSingleton<AppSession>();
        collection.AddSingleton<AppLocale>();
        collection.AddSingleton<StudentNoticeInbox>();
        collection.AddSingleton<AdminFeedbackInbox>();
        collection.AddSingleton<TeacherFeedbackInbox>();
        collection.AddDbContextFactory<LocalDbContext>(options =>
            options.UseSqlite($"Data Source={LocalDbContext.DefaultDatabasePath()}"));
        collection.AddSingleton<OfflineAuthService>();
        collection.AddSingleton<ISyncApiClient, SyncApiClient>();
        collection.AddSingleton<SyncEngine>();
        collection.AddSingleton<SyncLoopService>();
        collection.AddSingleton(sp => new HybridUpdateService(configuration["Update:InfoUrl"], sp.GetRequiredService<SyncEngine>()));
        collection.AddHttpClient(SyncApiClient.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(2);
        }).ConfigurePrimaryHttpMessageHandler(SyncApiClient.CreateHandler);

        return collection.BuildServiceProvider();
    }
}

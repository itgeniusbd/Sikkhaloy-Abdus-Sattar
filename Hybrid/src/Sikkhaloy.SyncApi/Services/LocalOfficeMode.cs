using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Sikkhaloy.SyncApi.Services;

/// <summary>
/// Local/dev office: skip live SMS and ShurjoPay. Production (non-Development) uses the real gateways.
/// </summary>
public sealed class LocalOfficeMode
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _http;

    public LocalOfficeMode(IWebHostEnvironment env, IConfiguration config, IHttpContextAccessor http)
    {
        _env = env;
        _config = config;
        _http = http;
    }

    public bool IsLocal
    {
        get
        {
            if (_config.GetValue("Sms:ForceLive", false))
                return false;
            if (_config.GetValue("Sms:DevMode", false))
                return true;
            if (_env.IsDevelopment())
                return true;
            var host = _http.HttpContext?.Request.Host.Host ?? "";
            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                   || host is "127.0.0.1" or "::1";
        }
    }
}

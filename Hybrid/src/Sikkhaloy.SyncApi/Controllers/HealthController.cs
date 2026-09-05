using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly EduConnectionFactory _connections;

    public HealthController(EduConnectionFactory connections)
    {
        _connections = connections;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _ = WarmSqlAsync();
        return Ok(new { status = "ok" });
    }

    private async Task WarmSqlAsync()
    {
        try
        {
            await using var con = _connections.Create();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            await con.OpenAsync(cts.Token);
            await using var cmd = new SqlCommand("SELECT 1", con) { CommandTimeout = 8 };
            await cmd.ExecuteScalarAsync(cts.Token);
        }
        catch
        {
        }
    }
}

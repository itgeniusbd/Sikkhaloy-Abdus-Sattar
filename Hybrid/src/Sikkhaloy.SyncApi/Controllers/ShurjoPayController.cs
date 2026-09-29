using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sikkhaloy.SyncApi.Services;

namespace Sikkhaloy.SyncApi.Controllers;

[ApiController]
[Route("api/pay/shurjopay")]
public sealed class ShurjoPayController : ControllerBase
{
    private readonly ShurjoPayCallbackService _callback;

    public ShurjoPayController(ShurjoPayCallbackService callback)
    {
        _callback = callback;
    }

    /// <summary>Browser return_url / cancel recovery from ShurjoPay — no auth (gateway callback).</summary>
    [HttpGet("callback")]
    [HttpPost("callback")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Callback(CancellationToken cancellationToken)
    {
        var orderId = First("order_id") ?? First("orderId") ?? First("sp_order_id");
        var result = await _callback.CompleteAsync(orderId, cancellationToken);
        return Redirect(_callback.BuildRedirectUrl(result));
    }

    /// <summary>JSON verify for Hybrid "Check payment" button.</summary>
    [HttpGet("complete")]
    [HttpPost("complete")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<ActionResult<ShurjoPayCallbackResult>> Complete(CancellationToken cancellationToken)
    {
        var orderId = First("order_id") ?? First("orderId") ?? First("sp_order_id");
        var result = await _callback.CompleteAsync(orderId, cancellationToken);
        return Ok(result);
    }

    private string? First(string name)
    {
        if (Request.Query.TryGetValue(name, out var q) && !string.IsNullOrWhiteSpace(q))
            return q.ToString();
        if (Request.HasFormContentType && Request.Form.TryGetValue(name, out var f) && !string.IsNullOrWhiteSpace(f))
            return f.ToString();
        return null;
    }
}

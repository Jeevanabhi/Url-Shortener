using Microsoft.AspNetCore.Mvc;

[ApiController]
public class GetController : ControllerBase
{
    private readonly UrlService _urlService;
    public GetController(UrlService urlService)
    {
        _urlService = urlService;
    }
    [HttpGet("/{shortCode}")]
    public async Task<IActionResult> RedirectToOriginal(string shortCode)
{
    var url = await _urlService.GetByShortCodeAsync(shortCode);

        if (url == null)
        {
            return NotFound();
        }
    var referrer = Request.Headers["Referer"].ToString();
    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
       await _urlService.LogClicks(url.UrlId, referrer, ipAddress);
    return Redirect(url.OriginalUrl);
}
}

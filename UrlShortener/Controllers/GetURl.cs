using Microsoft.AspNetCore.Mvc;

[ApiController]
public class GetController : ControllerBase
{
    private readonly UrlService _urlService;
    private readonly RabbitMqPublisher _publisher;
    public GetController(UrlService urlService,RabbitMqPublisher publisher)
    {
        _urlService = urlService;
        _publisher = publisher;
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
        var click = new ClickEvent
        {
            UrlId = url.UrlId,
            Referrer = referrer,
            IpAddress = ipAddress,
            ClickedAt = DateTime.UtcNow
        };
        await _publisher.PublishAsync(click);
    return Redirect(url.OriginalUrl);
}
}

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/urls")]
public class CreateController : ControllerBase
{
    private readonly UrlService _urlService;
    public CreateController(UrlService urlService)
    {
        _urlService = urlService;
        
    }
    [HttpPost]
    public async Task<ActionResult<UrlResponseDto>> Create(CreateUrlDTO dto)
    {
        if (!Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out var uriResult)
        || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
    {
        throw new ArgumentException("OriginalUrl must be a valid absolute HTTP or HTTPS URL.");
    }
        var code = await _urlService.CreateShortUrlAsync(dto);
        var shortUrl = $"{Request.Scheme}://{Request.Host}/{code}";   // ← builds the full link here
        var response = new UrlResponseDto
        {
            ShortCode = code,
            ShortUrl = shortUrl,   // ← goes into the response the client receives
            OriginalUrl = dto.OriginalUrl
        };

         return Created(shortUrl, response);
    }
}
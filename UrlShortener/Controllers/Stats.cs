using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class Stats: ControllerBase
{
    private readonly ILogger<Stats> _logger;
    private readonly UrlService _urlService;
     public Stats(UrlService urlService,ILogger<Stats> logger)
    {
        _urlService = urlService;
        _logger = logger;
        
    }
    [HttpGet("/api/urls/{shortCode}/stats")]
    public async Task<ActionResult<UrlStatsDto>> UrlStats(string shortCode)
    {
        var statsDto = await _urlService.GetStatsAsync(shortCode);
        if (statsDto is null)
        {
            _logger.LogWarning("Stats not found for {shortCode}", shortCode);
            return NotFound();
        }
        _logger.LogInformation("Stats returned for {shortCode}", shortCode);
        return Ok(statsDto);
    }
    
}
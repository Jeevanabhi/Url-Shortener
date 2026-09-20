using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

public class UrlServiceTests
{
    private UrlShorten GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<UrlShorten>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UrlShorten(options);
    }

    private UrlService CreateService(UrlShorten context)
    {
        var logger = new LoggerFactory().CreateLogger<UrlService>();
        var mockRedis = new Mock<IConnectionMultiplexer>();
        return new UrlService(context, logger, mockRedis.Object);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ValidUrl_SavesToDatabase()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = CreateService(context);
        var dto = new CreateUrlDTO { OriginalUrl = "https://www.youtube.com/" };

        // Act
        var code = await service.CreateShortUrlAsync(dto);

        // Assert
        var savedUrl = await context.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
        Assert.NotNull(savedUrl);
        Assert.Equal("https://www.youtube.com/", savedUrl.OriginalUrl);
    }
}
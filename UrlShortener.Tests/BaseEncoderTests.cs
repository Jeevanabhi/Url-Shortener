namespace UrlShortener.Tests;

public class UnitTest1
{
    [Fact]

    public void Encode_Zero_ReturnsFirstCharacter()
    {
        var result = Base62Encoder.Encode(0);
        Assert.Equal("0", result);
    }
     [Theory]
    [InlineData(1, "1")]
    [InlineData(61, "Z")]
    [InlineData(62, "10")]
    [InlineData(63, "11")]
    [InlineData(125, "21")]
    public void Encode_VariousInputs_ReturnsExpectedCode(long input, string expected)
    {
        var result = Base62Encoder.Encode(input);
        Assert.Equal(expected, result);
    }
}

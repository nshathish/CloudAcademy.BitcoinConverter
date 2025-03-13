using System.Net;
using System.Web;
using BitcoinConverter.Code;
using Moq;
using Moq.Protected;

namespace BitcoinConverter.Tests;

public class ConverterServiceTest
{
    private readonly ConverterService _converter;

    public ConverterServiceTest()
    {
        _converter = GetMockBitcoinConverterService();
    }


    [Theory]
    [InlineData("USD", 100d)]
    [InlineData("GBP", 200d)]
    [InlineData("EUR", 300d)]
    public async Task GetExchangeRate_ReturnsCorrectExchangeRate(string currency, double expected)
    {
        // Act
        var result = await _converter.GetExchangeRate(currency);
        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("USD", 1, 100)]
    [InlineData("USD", 2, 200)]
    [InlineData("GBP", 1, 200)]
    [InlineData("GBP", 2, 400)]
    [InlineData("EUR", 1, 300)]
    [InlineData("EUR", 2, 600)]
    public async Task ConvertBitcoins_BitcoinsToCurrency_ReturnsCurrency(string currency, int coins,
        int converterDollar)
    {
        // Act
        var result = await _converter.ConvertBitCoin(currency, coins);

        // Assert
        Assert.Equal(converterDollar, result);
    }


    [Fact]
    public async Task ConvertBitcoins_BitcoinsLessThanZero_ThrowsArgumentException()
    {
        // Act
        Task Result() => _converter.ConvertBitCoin("USD", -1);
        
        // Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(Result);
        Assert.Equal("Coins must be greater than 0", exception.Message);
    }

    private ConverterService GetMockBitcoinConverterService()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
            {
                var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
                var currency = query["tsyms"];

                var responseContent = currency switch
                {
                    "USD" => "{\"USD\":100}",
                    "GBP" => "{\"GBP\":200}",
                    "EUR" => "{\"EUR\":300}",
                    _ => "{}"
                };

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        return new ConverterService(httpClient);
    }
}
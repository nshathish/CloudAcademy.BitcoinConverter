using System.Text.Json;

namespace BitcoinConverter.Code;

public class ConverterService(HttpClient client)
{
    private const string BITCOIN_CURRENT_PRICE_URL = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=%";

    public ConverterService() : this(new HttpClient())
    {
    }

    public async Task<double> GetExchangeRate(string currency)
    {
        string[] supportedCurrencies = ["USD", "GBP", "EUR"];
        if (!supportedCurrencies.Contains(currency))
            throw new ArgumentException(
                $"Invalid currency. Supported currencies: {string.Join(",", supportedCurrencies)}");

        var url = BITCOIN_CURRENT_PRICE_URL.Replace("%", currency);
        var response = await client.GetStringAsync(url);

        using var document = JsonDocument.Parse(response);
        if (document.RootElement.TryGetProperty(currency, out var rate))
            return rate.GetDouble();

        throw new Exception("Currency not found");
    }

    public async Task<double> ConvertBitCoin(string currency, int coins)
    {
        if (coins < 0) throw new ArgumentException("Coins must be greater than 0");

        var rate = await GetExchangeRate(currency);
        return rate > 0 ? rate * coins : 0d;
    }
}
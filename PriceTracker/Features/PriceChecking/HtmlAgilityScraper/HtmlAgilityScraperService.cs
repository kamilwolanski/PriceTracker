using HtmlAgilityPack;
using PriceTracker.Features.PriceHistory.ValueObjects;

using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PriceTracker.Features.PriceChecking.HtmlAgilityScraper
{
    public class HtmlAgilityScraperService : IPriceScrapingStrategy
    {
        public int Priority => 1000;

        public bool CanHandle(Uri uri) => true;

        public async Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            AddBrowserLikeHeaders(request);

            using var response = await client.SendAsync(request, cancellationToken);
            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            await WriteDebugFilesAsync(uri, response, html, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return TryExtractPriceFromHtml(html);
        }

        public static Money? TryExtractPriceFromHtml(string html, string defaultCurrencyCode = "PLN")
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var jsonLdPrice = TryGetPriceFromJsonLd(doc, defaultCurrencyCode);
            if (jsonLdPrice != null)
                return jsonLdPrice;

            var priceText =
                GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:amount']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@name='product:price:amount']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@property='og:price:amount']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@name='og:price:amount']"),
                    "content")
                ??  GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@property='product:sale_price:amount']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//*[@itemprop='price']"),
                    "content")
                ?? doc.DocumentNode.SelectSingleNode("//*[@itemprop='price']")

                    ?.InnerText;


            var currencyCode = GetCurrencyCodeFromHtml(doc) ?? defaultCurrencyCode;
            return CurrencyParser.TryParse(priceText, currencyCode);
        }

        private static void AddBrowserLikeHeaders(HttpRequestMessage request)
        {
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
            request.Headers.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            request.Headers.AcceptLanguage.ParseAdd("pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7");
        }

        private static async Task WriteDebugFilesAsync(
            Uri uri,
            HttpResponseMessage response,
            string html,
            CancellationToken cancellationToken)
        {
            var debugDirectory = Path.Combine(@"C:\tmp", "PriceTracker");
            Console.WriteLine($"Saving debug to: {debugDirectory}");
            Directory.CreateDirectory(debugDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(debugDirectory, "scraper-debug.html"),
                html,
                cancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(debugDirectory, "scraper-debug-status.txt"),
                $"Strategy: HtmlAgility{Environment.NewLine}StatusCode: {(int)response.StatusCode} {response.StatusCode}{Environment.NewLine}Url: {uri}{Environment.NewLine}FinalUrl: {response.RequestMessage?.RequestUri}",
                cancellationToken);
        }

        private static string? GetAttributeValueOrNull(HtmlNode? node, string attributeName)
        {
            var value = node?.GetAttributeValue(attributeName, string.Empty);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private static Money? TryGetPriceFromJsonLd(HtmlDocument doc, string defaultCurrencyCode)
        {
            var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes == null)
                return null;

            foreach (var node in jsonLdNodes)
            {
                var json = HtmlEntity.DeEntitize(node.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(json))
                    continue;

                var price = TryExtractPriceFromJsonLd(json, defaultCurrencyCode);
                if (price != null)
                    return price;
            }

            return null;
        }

        private static Money? TryExtractPriceFromJsonLd(string json, string defaultCurrencyCode)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                return FindProductPrice(document.RootElement, defaultCurrencyCode);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static Money? FindProductPrice(JsonElement element, string defaultCurrencyCode)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (IsProduct(element))
                {
                    var productPrice = TryGetPriceFromProduct(element, defaultCurrencyCode);
                    if (productPrice != null)
                        return productPrice;
                }

                if (element.TryGetProperty("@graph", out var graph))
                {
                    var graphPrice = FindProductPrice(graph, defaultCurrencyCode);
                    if (graphPrice != null)
                        return graphPrice;
                }

                foreach (var property in element.EnumerateObject())
                {
                    var nestedPrice = FindProductPrice(property.Value, defaultCurrencyCode);
                    if (nestedPrice != null)
                        return nestedPrice;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var price = FindProductPrice(item, defaultCurrencyCode);
                    if (price != null)
                        return price;
                }
            }

            return null;
        }

        private static bool IsProduct(JsonElement element)
        {
            if (!element.TryGetProperty("@type", out var type))
                return false;

            if (type.ValueKind == JsonValueKind.String)
                return string.Equals(type.GetString(), "Product", StringComparison.OrdinalIgnoreCase);

            if (type.ValueKind == JsonValueKind.Array)
            {
                return type.EnumerateArray().Any(item =>
                    item.ValueKind == JsonValueKind.String &&
                    string.Equals(item.GetString(), "Product", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        private static Money? TryGetPriceFromProduct(JsonElement product, string defaultCurrencyCode)
        {
            if (!product.TryGetProperty("offers", out var offers))
                return null;

            return TryGetPriceFromOffers(offers, defaultCurrencyCode);
        }

        private static Money? TryGetPriceFromOffers(JsonElement offers, string defaultCurrencyCode)
        {
            if (offers.ValueKind == JsonValueKind.Object)
            {
                if (offers.TryGetProperty("price", out var price))
                    return ParseJsonPrice(price, offers, defaultCurrencyCode);

                if (offers.TryGetProperty("lowPrice", out var lowPrice))
                    return ParseJsonPrice(lowPrice, offers, defaultCurrencyCode);
            }

            if (offers.ValueKind == JsonValueKind.Array)
            {
                foreach (var offer in offers.EnumerateArray())
                {
                    var price = TryGetPriceFromOffers(offer, defaultCurrencyCode);
                    if (price != null)
                        return price;
                }
            }

            return null;
        }

        private static string? GetCurrencyCodeFromHtml(HtmlDocument doc)
        {
            return GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@property='product:price:currency']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@name='product:price:currency']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@property='og:price:currency']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//meta[@name='og:price:currency']"),
                    "content")
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//*[@itemprop='priceCurrency']"),
                    "content")
                ?? doc.DocumentNode.SelectSingleNode("//*[@itemprop='priceCurrency']")
                    ?.InnerText;
        }

        private static Money? ParseJsonPrice(JsonElement priceElement, JsonElement offerElement, string defaultCurrencyCode)
        {
            var priceText = priceElement.ValueKind switch
            {
                JsonValueKind.Number => priceElement.GetDecimal().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.String => priceElement.GetString(),
                _ => null
            };

            var currencyCode = TryGetJsonStringProperty(offerElement, "priceCurrency") ?? defaultCurrencyCode;
            return CurrencyParser.TryParse(priceText, currencyCode);
        }

        private static string? TryGetJsonStringProperty(JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object &&
                element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String
                    ? property.GetString()
                    : null;
        }
    }
}




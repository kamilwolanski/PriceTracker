using HtmlAgilityPack;
using PriceTracker.Features.PriceChecking.Strategies;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PriceTracker.Features.PriceChecking.HtmlAgilityScraper
{
    public class HtmlAgilityScraperService : IPriceScrapingStrategy
    {
        public int Priority => 1000;

        public bool CanHandle(Uri uri) => true;

        public async Task<decimal?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
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

        public static decimal? TryExtractPriceFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var jsonLdPrice = TryGetPriceFromJsonLd(doc);
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
                ?? GetAttributeValueOrNull(
                    doc.DocumentNode.SelectSingleNode("//*[@itemprop='price']"),
                    "content")
                ?? doc.DocumentNode.SelectSingleNode("//*[@itemprop='price']")
                    ?.InnerText;

            return ParsePrice(priceText);
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

        private static decimal? TryGetPriceFromJsonLd(HtmlDocument doc)
        {
            var jsonLdNodes = doc.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (jsonLdNodes == null)
                return null;

            foreach (var node in jsonLdNodes)
            {
                var json = HtmlEntity.DeEntitize(node.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(json))
                    continue;

                var price = TryExtractPriceFromJsonLd(json);
                if (price != null)
                    return price;
            }

            return null;
        }

        private static decimal? TryExtractPriceFromJsonLd(string json)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                return FindProductPrice(document.RootElement);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static decimal? FindProductPrice(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (IsProduct(element))
                {
                    var productPrice = TryGetPriceFromProduct(element);
                    if (productPrice != null)
                        return productPrice;
                }

                if (element.TryGetProperty("@graph", out var graph))
                {
                    var graphPrice = FindProductPrice(graph);
                    if (graphPrice != null)
                        return graphPrice;
                }

                foreach (var property in element.EnumerateObject())
                {
                    var nestedPrice = FindProductPrice(property.Value);
                    if (nestedPrice != null)
                        return nestedPrice;
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var price = FindProductPrice(item);
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

        private static decimal? TryGetPriceFromProduct(JsonElement product)
        {
            if (!product.TryGetProperty("offers", out var offers))
                return null;

            return TryGetPriceFromOffers(offers);
        }

        private static decimal? TryGetPriceFromOffers(JsonElement offers)
        {
            if (offers.ValueKind == JsonValueKind.Object)
            {
                if (offers.TryGetProperty("price", out var price))
                    return ParsePrice(price);

                if (offers.TryGetProperty("lowPrice", out var lowPrice))
                    return ParsePrice(lowPrice);
            }

            if (offers.ValueKind == JsonValueKind.Array)
            {
                foreach (var offer in offers.EnumerateArray())
                {
                    var price = TryGetPriceFromOffers(offer);
                    if (price != null)
                        return price;
                }
            }

            return null;
        }

        private static decimal? ParsePrice(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
                return element.GetDecimal();

            if (element.ValueKind == JsonValueKind.String)
                return ParsePrice(element.GetString());

            return null;
        }

        private static decimal? ParsePrice(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var cleaned = Regex.Replace(text, @"[^\d,.]", "");
            if (string.IsNullOrWhiteSpace(cleaned))
                return null;

            var commaIndex = cleaned.LastIndexOf(',');
            var dotIndex = cleaned.LastIndexOf('.');

            if (commaIndex >= 0 && dotIndex >= 0)
            {
                cleaned = commaIndex > dotIndex
                    ? cleaned.Replace(".", "").Replace(',', '.')
                    : cleaned.Replace(",", "");
            }
            else if (commaIndex >= 0)
            {
                cleaned = NormalizeSingleSeparator(cleaned, commaIndex, ',');
            }
            else if (dotIndex >= 0)
            {
                cleaned = NormalizeSingleSeparator(cleaned, dotIndex, '.');
            }

            if (decimal.TryParse(
                cleaned,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price))
            {
                return price;
            }

            return null;
        }

        private static string NormalizeSingleSeparator(string value, int separatorIndex, char separator)
        {
            var digitsAfterSeparator = value.Length - separatorIndex - 1;
            if (digitsAfterSeparator == 3)
                return value.Replace(separator.ToString(), "");

            return separator == ','
                ? value.Replace(',', '.')
                : value;
        }
    }
}

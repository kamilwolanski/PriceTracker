using Microsoft.Playwright;
using PriceTracker.Features.PriceChecking.HtmlAgilityScraper;
using PriceTracker.Features.PriceHistory.ValueObjects;


namespace PriceTracker.Features.PriceChecking.PlaywrightScraper
{
    public class PlaywrightScraperService : IPriceScrapingStrategy
    {
        public int Priority => 2000;

        public bool CanHandle(Uri uri) => true;

        public async Task<Money?> ScrapePriceAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var page = await browser.NewPageAsync(new BrowserNewPageOptions
            {
                Locale = "pl-PL",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36"
            });

            await page.GotoAsync(uri.ToString(), new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            var html = await page.ContentAsync();
            await WriteDebugFilesAsync(uri, html, cancellationToken);

            return HtmlAgilityScraperService.TryExtractPriceFromHtml(html);
        }

        private static async Task WriteDebugFilesAsync(Uri uri, string html, CancellationToken cancellationToken)
        {
            var debugDirectory = Path.Combine(@"C:\tmp", "PriceTracker");
            Console.WriteLine($"Saving debug to: {debugDirectory}");
            Directory.CreateDirectory(debugDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(debugDirectory, "playwright-debug.html"),
                html,
                cancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(debugDirectory, "playwright-debug-status.txt"),
                $"Strategy: Playwright{Environment.NewLine}Url: {uri}",
                cancellationToken);
        }
    }
}


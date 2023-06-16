using Scraper;

string url = (args.Length == 1) ? args[0] : "https://www.mangaworld.bz/manga/1392/cowboy-bebop";

var infoScaper = new InfoScraper(url);
infoScaper.Scrape();
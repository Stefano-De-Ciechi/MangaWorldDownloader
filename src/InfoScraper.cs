namespace Scraper;

using HtmlAgilityPack;
using System.Collections.Concurrent;

// TODO generate a JSON file (based on the name scraped) containing all of the volumes info

public class InfoScraper
{
    private string _url;    // url of the manga's page with the chapters and volumes list
    private int _totalPagesRequested;   // counter of the number of html pages requested (just for info)

    public int TotalPagesRequested
    {
        get { return _totalPagesRequested; }
    }

    public InfoScraper(string url)
    {
        _url = url;
        _totalPagesRequested = 0;
    }

    private string GetHtml(string url)        // makes an Http request and returns the html of the page as a string
    {
        var client = new HttpClient();
        var html = client.GetStringAsync(url);      // I decided to make this async request work syncronously just because without the html page this program has no way of continuing
        _totalPagesRequested += 1;
        
        return html.Result;
    }

    private async Task<string> GetHtmlAsync()       // async variant of the GetHtml method --> I didn't use this one to avoid the async / await chain on all the methods' declarations
    {
        var client = new HttpClient();
        var html = await client.GetStringAsync(_url);
        
        return html;
    }

    private Manga ParseMangaPage(string html)     // parse the html of the page containing the list of volumes, chapters and informations of the manga
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;

        var mangaName = GetMangaName(document);     // extract this information direclty from the html

        var chaptersWrapper = document.GetElementByClassName("chapters-wrapper");

        var volumes = GetVolumes(chaptersWrapper);

        // some mangas may have no volumes but only chapters --> in this case I create a single volume and insert all the chapters into it
        if (volumes.Count() == 0)
        {
            var chapters = GetChapters(chaptersWrapper);
            var vols = new List<Volume>();

            vols.Add(new Volume("noVolumes", chapters));
            
            return new Manga(mangaName, vols);
        }

        volumes = volumes.OrderBy(volume => volume.Name);       // order the volumes by name (usually an ascending number) since the parallel execution will produce unordered results
        
        return new Manga(mangaName, volumes);
    }

    private string GetMangaName(HtmlNode document)      // retrieve the manga name information directly from the html
    {
        var info = document.GetElementByClassName("info");
        var h1 = info.GetElementByTagName("h1");
        
        return h1.InnerText;
    }

    private IEnumerable<Volume> GetVolumes(HtmlNode chaptersWrapper)       // retrieve all of the volumes' names, and for each volume retrieve informations on his chapters (chapters-wrapper is the class used in the div)
    {
        var volumeElements = chaptersWrapper.GetElementsByClassName("volume-element");
        
        var volumes = new ConcurrentBag<Volume>();      // thread-safe unordered collection of objects (to be used with Parallel below)

        // execution with multiple threads (if available)
        Parallel.ForEach(volumeElements, volumeElement =>
        {
            var volumeName = volumeElement.GetElementsByTagName("p").First().InnerText;
            var volumeChapters = GetChapters(volumeElement);        // retrieve a list of all the chapters (and their informations) of a volume
            volumeChapters = volumeChapters.OrderBy(chapter => chapter.Name);       // order chapters by name (usually an ascending number) since the parallel execution will produce unordered results

            volumes.Add(new Volume(volumeName, volumeChapters));
        });

        return volumes;
    }

    private IEnumerable<Chapter> GetChapters(HtmlNode volumeElement)    // retrieve all of the volume's chapters and their informations (volume-element is the name of the class used in the div)
    {
        var chapters = volumeElement.GetElementsByClassName("chapter");
        List<Chapter> chaptersList = new();

        foreach (var chapt in chapters)
        {
            var chaptName = chapt.GetElementByTagName("span").InnerText;
            var releaseDate = chapt.GetElementByTagName("i").InnerText;
            var link = chapt.FirstChild.Attributes["href"].Value;       // this link points to the page containing the volume informations (like the number of pages) and the link to the first image

            var (numPages, firstPageLink, format) = GetChapterPagesInfo(link);

            chaptersList.Add(new Chapter(chaptName, releaseDate, numPages, firstPageLink, format));
        }

        return chaptersList;
    }

    /// <exception cref="FormatException"> this exception is thrown if the page has a format different from jpg or png </exception>
    private (int numPages, string firstPageLink, string format) GetChapterPagesInfo(string link)    // retrieve informations on the chapter (number of pages, link to the first page and format of the page (jpg or png))
    {
        // unfortunately, these informations are stored in a separate html page, so an entire new page has to be requested for each chapter ! (I don't think that there is a way around this)
        var html = GetHtml(link);       // TODO test if performance can improve by using the Async variant here (but I don't think so)
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;

        var pagesSelector = document.GetElementByClassName("page");
        var numPages = pagesSelector.ChildNodes.Count();        // count the number of pages indirectly (by counting the child nodes)

        var page = document.GetElementById("page");
        var firstPageLink = page.GetElementByTagName("img").Attributes["src"].Value;        // this link contains the url of the first page/image of the chapter
        var format = firstPageLink[^3..^0];     // retrieve the last 3 characters of the link (they should always be jpg or png)

        if (format is not "jpg" or "png")
            throw new FormatException($"unknown format for the page found at { firstPageLink }");

        return (numPages, firstPageLink, format);
    }

    public Manga Scrape()        // main method of the class
    {
        if (!_url.Contains("mangaworld"))
        {
            Console.WriteLine("This scraper only works on the site: https://www.mangaworld.bz !");
            Environment.Exit(-1);
        }

        var html = GetHtml(_url);
        var manga = ParseMangaPage(html);

        PrintMangaInfo(manga);

        return manga;
    }

    private void PrintMangaInfo(Manga manga)        // more of a debug method to print some of the scraped informations
    {
        Console.WriteLine($"\nmanga name: { manga.Name }");

        var volumes = manga.Volumes;
        int totalPages = 0;

        foreach (var vol in volumes = volumes ?? Enumerable.Empty<Volume>())
        {
            Console.WriteLine($"\n===== { vol.Name } - { vol.Chapters.Count() } chapters =====");
            
            foreach (var chapt in vol.Chapters)
            {
                Console.WriteLine($"{ chapt.Name } - { chapt.NumPages } pages");
                totalPages += chapt.NumPages;
            }
        }

        Console.WriteLine($"\ntotal manga pages: { totalPages }");
        Console.WriteLine($"\ntotal html pages requested: { TotalPagesRequested }");
    }
}
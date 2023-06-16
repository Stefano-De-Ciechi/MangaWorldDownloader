namespace Scraper;

using System.Collections.Concurrent;
using HtmlAgilityPack;

// TODO generate a JSON file (based on the name scraped) containing all of the volumes info

public class InfoScraper
{
    private string _url;    // url of the manga's page with the chapters and volumes list

    public InfoScraper(string url)
    {
        _url = url;
    }

    private string GetHtml(string url)        // return the html page containing the list of volumes and chapters of the manga
    {
        var client = new HttpClient();
        var html = client.GetStringAsync(url);
        
        return html.Result;
    }

    private async Task<string> GetHtmlAsync()       // async variant
    {
        var client = new HttpClient();
        var html = await client.GetStringAsync(_url);
        
        return html;
    }

    private Manga ParseHtml(string html)     // parse the html of the page containing the list of volumes and chapters
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;

        var mangaName = GetMangaName(document);

        var volumes = GetVolumes(document);
        volumes = volumes.OrderBy(volume => volume.Name);
        
        return new Manga(mangaName, volumes);
    }

    private string GetMangaName(HtmlNode document)
    {
        var info = document.GetElementByClassName("info");
        var h1 = info.GetElementByTagName("h1");
        
        return h1.InnerText;
    }

    private IEnumerable<Volume> GetVolumes(HtmlNode document)
    {
        var volumeElements = document.GetElementsByClassName("volume-element");
        
        var volumes = new ConcurrentBag<Volume>();

        Parallel.ForEach(volumeElements, volumeElement =>
        {
            var volumeName = volumeElement.GetElementsByTagName("p").First().InnerText;
            var volumeChapters = GetChapters(volumeElement);
            volumeChapters = volumeChapters.OrderBy(chapter => chapter.Name);

            volumes.Add(new Volume(volumeName, volumeChapters));
        });

        return volumes;
    }

    private IEnumerable<Chapter> GetChapters(HtmlNode volumeElement)
    {
        var chapters = volumeElement.GetElementsByClassName("chapter");
        List<Chapter> chaptersList = new();

        foreach (var chapt in chapters)
        {
            var chaptName = chapt.GetElementByTagName("span").InnerText;
            var releaseDate = chapt.GetElementByTagName("i").InnerText;
            var link = chapt.FirstChild.Attributes["href"].Value;

            var (numPages, firstPageLink, format) = GetChapterPagesInfo(link);

            chaptersList.Add(new Chapter(chaptName, releaseDate, numPages, firstPageLink, format));
        }

        return chaptersList;
    }

    private (int numPages, string firstPageLink, string format) GetChapterPagesInfo(string link)
    {
        var html = GetHtml(link);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;

        var pagesSelector = document.GetElementByClassName("page");
        var numPages = pagesSelector.ChildNodes.Count();

        var page = document.GetElementById("page");
        var firstPageLink = page.GetElementByTagName("img").Attributes["src"].Value;
        var format = firstPageLink[^3..^0];

        return (numPages, firstPageLink, format);
    }

    public void Scrape()
    {
        var html = GetHtml(_url);
        var manga = ParseHtml(html);

        PrintMangaInfo(manga);
    }

    private void PrintMangaInfo(Manga manga)
    {
        Console.WriteLine($"\nname: { manga.Name }");

        var volumes = manga.Volumes;

        foreach (var vol in volumes = volumes ?? Enumerable.Empty<Volume>())
        {
            Console.WriteLine($"\n===== { vol.Name } - { vol.Chapters.Count() } chapters =====");
            
            foreach (var chapt in vol.Chapters)
            {
                Console.WriteLine($"{ chapt.Name } - { chapt.NumPages } pages");
            }
        }
    }
}

record Manga(string Name, IEnumerable<Volume> Volumes);

record Volume
{
    public string Name { get; init; }
    public IEnumerable<Chapter> Chapters { get; init; }

    public Volume(string name, IEnumerable<Chapter> chapters)
    {
        Name = name;
        Chapters = chapters;
    }
}

record Chapter
{
    public string Name { get; init; }
    public string ReleaseDate { get; init; }
    public int NumPages { get; set; }
    public string FirstPageLink { get; init; }
    public string FirstPageFormat { get; init; } 

    public Chapter(string name, string releaseDate, int numPages, string firstPageLink, string firstPageFormat)
    {
        Name = name;
        ReleaseDate = releaseDate;
        NumPages = numPages;
        FirstPageLink = firstPageLink;
        FirstPageFormat = firstPageFormat;
    }
}
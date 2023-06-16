namespace Scraper;

using HtmlAgilityPack;

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

    private List<string> ParseHtml(string html)     // parse the html of the page containing the list of volumes and chapters
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;
        var res = new List<string>();

        var volumes = GetVolumes(document);
        
        foreach (var vol in volumes = volumes ?? Enumerable.Empty<Volume>())
        {
            Console.WriteLine($"{ vol.Name } : numChapters is { vol.Chapters.Count() }");
            foreach (var chapt in vol.Chapters)
            {
                Console.WriteLine($"{ chapt.Name } has { chapt.NumPages } pages");
            }
        }

        return res;
    }

    private IEnumerable<Volume> GetVolumes(HtmlNode document)
    {
        var volumeElements = document.GetElementsByClassName("volume-element");
        
        List<Volume> volumes = new();

        foreach(var volumeElement in volumeElements)
        {
            var volumeName = volumeElement.GetElementsByTagName("p").First().InnerText;
            var volumeChapters = GetChapters(volumeElement);

            volumes.Add(new Volume(volumeName, volumeChapters));
        }

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
            var numPages = GetChapterNumPages(link);

            chaptersList.Add(new Chapter(chaptName, releaseDate, link, numPages));
        }

        return chaptersList;
    }

    private int GetChapterNumPages(string link)
    {
        var html = GetHtml(link);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var document = htmlDoc.DocumentNode;

        var pagesSelector = document.GetElementByClassName("page");
        return pagesSelector.ChildNodes.Count();
    }

    private void PrintInfo(List<string> list)
    {
        foreach (var i in list)
        {
            Console.WriteLine(i);
        }
    }

    public void Scrape()
    {
        var html = GetHtml(_url);
        var list = ParseHtml(html);
        PrintInfo(list);
    }
}

record Volume
{
    //public int Number { get; init; }
    public string Name { get; init; }
    public IEnumerable<Chapter> Chapters { get; init; }

    /*public Volume(int number, IEnumerable<Chapter> chapters)
    {
        Number = number;
        Chapters = chapters;
    }*/

    public Volume(string name, IEnumerable<Chapter> chapters)
    {
        Name = name;
        Chapters = chapters;
    }
}

record Chapter
{
    //public int Number { get; init; }
    public string Name { get; init; }
    public string ReleaseDate { get; init; }
    public string Link { get; init; }
    public int NumPages { get; set; }

    /*public Chapter(int number, string releaseDate, string link, int numPages)
    {
        Number = number;
        ReleaseDate = releaseDate;
        Link = link;
        NumPages = numPages;
    }*/

    public Chapter(string name, string releaseDate, string link, int numPages = 0)
    {
        Name = name;
        ReleaseDate = releaseDate;
        Link = link;
        NumPages = numPages;
    }
}

record Page
{
    public string Format { get; init; }
    public string Link { get; init; }

    public Page(string format, string link)
    {
        Format = format;
        Link = link;
    }
}
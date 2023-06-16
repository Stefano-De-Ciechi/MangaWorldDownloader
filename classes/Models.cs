namespace Scraper;

public record Manga(string Name, IEnumerable<Volume> Volumes);

public record Volume
{
    public string Name { get; init; }
    public IEnumerable<Chapter> Chapters { get; init; }

    public Volume(string name, IEnumerable<Chapter> chapters)
    {
        Name = name;
        Chapters = chapters;
    }
}

public record Chapter
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
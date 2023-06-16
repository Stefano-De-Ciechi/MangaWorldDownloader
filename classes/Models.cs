using System.Text.Json.Serialization;

namespace Scraper;

public record Manga
{
    [JsonPropertyName("manga")]
    public string Name { get; init; }
    public IEnumerable<Volume> Volumes { get; init; }

    public Manga(string name, IEnumerable<Volume> volumes)
    {
        Name = name;
        Volumes = volumes;
    }
}

public record Volume
{
    [JsonPropertyName("volume")]     // added to make JsonSerializer use this name when converting to json format
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
    [JsonPropertyName("chapterNum")]     // added to make JsonSerializer use this name when converting to json format
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
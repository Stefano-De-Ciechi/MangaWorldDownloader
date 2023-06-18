using System.Text.Json;
using Scraper;

public class MangaSerializer
{
    private Manga _manga;

    public MangaSerializer(Manga manga)
    {
        _manga = manga;
    }

    public async Task Serialize()
    {
        var fileName = $"../Data/info/{ _manga.Name }-info.json";       // TODO make the path platform indepentent (and maybe cofigurable from a .env, .cfg or other configuration files)
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync<Manga>(createStream, _manga, options);
        await createStream.DisposeAsync();

        Console.WriteLine("finished scraping and writing to JSON file");
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;
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
        var fileName = $"./scrapedMangas/{ _manga.Name }-info.json";
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        //string json = JsonSerializer.Serialize<Manga>(_manga, options);
        //Console.WriteLine(json);

        using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync<Manga>(createStream, _manga, options);
        await createStream.DisposeAsync();

        Console.WriteLine("finished scraping and writing to JSON file");
    }
}
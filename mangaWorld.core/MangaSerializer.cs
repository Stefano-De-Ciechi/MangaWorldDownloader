using System.Text.Json;

namespace MangaWorld.Core;

public static class MangaSerializer
{

    public static string DocumentsRootPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    private static string _dataFolder = Path.Combine(DocumentsRootPath, "MWD/Data");
    
    public static async Task Serialize(Manga manga)
    {
        //var fileName = $"../Data/info/{ manga.Name }-info.json";       // TODO make the path platform indepentent (and maybe cofigurable from a .env, .cfg or other configuration files)
        var fileName = $"{Path.Combine(_dataFolder, $"Info/{manga.Name}-info.json")}";
        var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        Directory.CreateDirectory(Path.Combine(_dataFolder, "Info"));
        await using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync<Manga>(createStream, manga, options);
        //await createStream.DisposeAsync();    // tecnically not needed since there is the "using" statement

        Console.WriteLine("finished scraping and writing to JSON file");
    }

    public static async Task<Manga?> Deserialize(string fileName, string filePath = "../Data/info/")
    {
        //var file = $"{ filePath }/{ fileName }";    // TODO add checks on the file format, name, existence etc.
        var file = $"{Path.Combine(DocumentsRootPath, $"Downloads/{fileName}")}";
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        using FileStream readStream = File.OpenRead(file);
        Manga? manga = await JsonSerializer.DeserializeAsync<Manga>(readStream, options);

        return manga;
    }
}
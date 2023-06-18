namespace Downloader;

// TODO make all paths platform independent (and maybe configurable from a .env, a .cfg or other configuration files)

public class MangaDownloader
{
    private char _separator = Path.DirectorySeparatorChar;
    private string _downloadPath;

    public MangaDownloader(string? downloadPath = null)
    {
        _downloadPath = downloadPath ?? $"..{ _separator }Data{ _separator }downloads{ _separator }";
    }

    /*public bool LoadManga(string fileName)
    {
        return false;
    }*/

    internal async Task DownloadImage(string mangaName, string volume, string chapter, int pageNum/*, string format*/, string pageLink)
    {
        var format = pageLink[^3..^0];
        var fileName = $"page{ pageNum }.{ format }";       // pageN.jpg or pageN.png
        var folderName = $"{ mangaName }{ _separator }{ volume }{ _separator }{ chapter }{ _separator }";      // Volume N/Capitolo M/
        var fullPath = $"{ _downloadPath }{ folderName }";

        string debugMsg = $"page { pageNum } ({ format }) - { chapter } - { volume }";

        HttpClient http = new HttpClient();

        bool status =await ImageRequest(http, fullPath, fileName, format, pageLink);

        if (status == true)
        {
            Console.WriteLine($"succesfully downloaded { debugMsg }");
            return;
        }

        var oldFormat = format;

        // if response was insuccessful try to change the format (sometimes manga switch format type of the scans even in the middle of a chapter!)
        format = format switch
        {
            "jpg" => "png",
            "png" => "jpg",
            _ => throw new FormatException($"unsupported format '{ format }' for { debugMsg }")
        };

        pageLink = pageLink.Replace($"{ pageNum }.{ oldFormat }", $"{ pageNum }.{ format }");
        fileName = $"page{ pageNum }.{ format }";
        debugMsg = $"page { pageNum } ({ format }) - { chapter } - { volume }";
        status = await ImageRequest(http, fullPath, fileName, format, pageLink);

        if (status == true)
        {
            Console.WriteLine($"succesfully downloaded { debugMsg }");
            return;
        }

        throw new HttpRequestException($"could not download { debugMsg }");

    }

    private async Task<bool> ImageRequest(HttpClient http, string fullPath, string fileName, string format, string pageLink)
    {
        var res = await http.GetAsync(pageLink);

        if (res.IsSuccessStatusCode)
        {
            Directory.CreateDirectory(fullPath);
            byte[] bytes = await res.Content.ReadAsByteArrayAsync();

            await File.WriteAllBytesAsync($"{ fullPath }{ fileName }", bytes);
            return true;
        }

        return false;
    }
}
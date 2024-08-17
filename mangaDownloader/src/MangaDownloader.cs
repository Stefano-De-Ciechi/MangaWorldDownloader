using MangaWorld.Core;

namespace MangaWorld.Downloader;

// TODO make all paths platform independent (and maybe configurable from a .env, a .cfg or other configuration files)
// TODO dowload optimizaion following this link (https://www.michalbialecki.com/en/2018/04/19/how-to-send-many-requests-in-parallel-in-asp-net-core/) or this link (https://makolyte.com/csharp-how-to-make-concurrent-requests-with-httpclient/)

public class MangaDownloader
{
    private char _separator = Path.DirectorySeparatorChar;
    private string _downloadPath;
    private Manga? _manga;
    private HttpClient _httpClient;

    // TODO make the path of the info.json files settable too (like the download path)
    public MangaDownloader(string fileName, string? downloadPath = null)
    {
        _downloadPath = downloadPath ?? $"..{ _separator }Data{ _separator }downloads{ _separator }";
        _httpClient = new HttpClient();
        LoadManga(fileName);
    }

    private void LoadManga(string fileName)
    {
        var manga = MangaSerializer.Deserialize(fileName).Result;

        if (manga == null)
            throw new FileNotFoundException($"unable to find a .json info file about this manga");

        _manga = manga;
    }

    public async Task DownloadVolume(string volumeName)
    {
        if (_manga == null)
            return;

        var volume = _manga[volumeName];
        
        if (volume == null)
            return;

        var chapters = (List<Chapter>) volume.Chapters;     // TODO use a Hashmap / Dictionary instead of a simple List ?? It should be way faster to search into it (without having to loop over all of it all of the times) --> _manga.Volumes.ToDictionary()

        foreach (var chapter in chapters)
        {
            await DownloadChapter(volume.Name, chapter.Name);
        }

        // even by using Parallel, it doesn't see to download much faster (sometimes even slower)
        /*await Parallel.ForEachAsync(chapters, async (chapter, token) => {
            await DownloadChapter(volume.Name, chapter.Name);
        });*/
    }

    public async Task DownloadChapter(string volumeName, string chapterName)
    {
        if (_manga == null)
            return;

        var volume = _manga[volumeName];

        if (volume == null)
            return;

        var chapter = volume[chapterName];

        if (chapter == null)
            return;

        for (int i = 1; i <= chapter.NumPages; i++)
        {
            var link = chapter.FirstPageLink.Replace(chapter.FirstPageLink[^5..^0], $"{i}.{chapter.FirstPageFormat}");
            try
            {
                await DownloadImage(_manga.Name, volume.Name, chapter.Name, i, link);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"EXCEPTION: {e.Message}");
                Console.WriteLine($"skipping this page...");
            }
            
        }

        // even by using Parallel, it doesn't see to download much faster (sometimes even slower)
        /*int[] pages = new int[chapter.NumPages];
        for (int i = 1; i <= chapter.NumPages; i++)
        {
            pages[i - 1] = i;
        }
        
        await Parallel.ForEachAsync(pages, async (i, token) => {
            var link = chapter.FirstPageLink.Replace(chapter.FirstPageLink[^5..^0], $"{i}.{chapter.FirstPageFormat}");
            await DownloadImage(_manga.Name, volume.Name, chapter.Name, i, link);
        });*/
    }

    private async Task DownloadImage(string mangaName, string volume, string chapter, int pageNum/*, string format*/, string pageLink)
    {
        var format = pageLink[^3..^0];
        var fileName = $"page{ pageNum }.{ format }";       // pageN.jpg or pageN.png
        var folderName = $"{ mangaName }{ _separator }{ volume }{ _separator }{ chapter }{ _separator }";      // Volume N/Capitolo M/
        var fullPath = $"{ _downloadPath }{ folderName }";

        string debugMsg = $"page { pageNum } ({ format }) - { chapter } - { volume }";

        HttpClient http = new HttpClient();

        bool status = await ImageRequest(fullPath, fileName, format, pageLink);

        if (status == true)
        {
            Console.WriteLine($"successfully downloaded { debugMsg }");
            return;
        }

        var oldFormat = format;

        // if response was insuccessful try to change the format (sometimes manga switch format type of the scans even in the middle of a chapter!)
        format = format switch
        {
            "jpg" => "png",
            "png" => "jpg",
            "gif" => "jpg",
            _ => throw new FormatException($"unsupported format '{ format }' for { debugMsg }")
        };

        pageLink = pageLink.Replace($"{ pageNum }.{ oldFormat }", $"{ pageNum }.{ format }");
        fileName = $"page{ pageNum }.{ format }";
        debugMsg = $"page { pageNum } ({ format }) - { chapter } - { volume }";
        status = await ImageRequest(fullPath, fileName, format, pageLink);

        if (status == true)
        {
            Console.WriteLine($"successfully downloaded { debugMsg }");
            return;
        }

        throw new HttpRequestException($"could not download { debugMsg }");
    }

    private async Task<bool> ImageRequest(string fullPath, string fileName, string format, string pageLink)
    {
        var res = await _httpClient.GetAsync(pageLink);

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
using CommandLine;

namespace MangaWorld.Downloader;

public static class CliApplication
{
    public static async Task Start(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(StartProgram);
    }

    private static async Task StartProgram(Options options)
    {
        if (options.File == null)   // this shouldn't be necessary as File is a required argument and the program won't enter this function without it (but the compiler says "posible null dereference")
            return;

        var downloader = new MangaDownloader(options.File);

        if (options.Volumes != null)
        {
            foreach (var volume in options.Volumes)
            {
                //Console.WriteLine(volume);
                await downloader.DownloadVolume(volume);
            }
        }

        if (options.Chapters != null)
        {
            foreach (var volumeChapterPair in options.Chapters)     // each element of options.Chapter should be a strings like "Volume num":"Chapter num"
            {
                var parts = volumeChapterPair.Split(':');

                if (parts.Length != 2)
                {
                    Console.WriteLine($"wrong format for the argument: { volumeChapterPair }");
                    return;
                }

                var volume = parts[0];
                var chapter = parts[1];

                //Console.WriteLine($"{ chapter } in { volume }");
                await downloader.DownloadChapter(volume, chapter);
            }
        }
    }
}

// TODO use Verbs like scrape and download to choose with tool to call (Scraper or Downloader) --> https://github.com/commandlineparser/commandline/wiki/Verbs
class Options
{
    [Option('f', Required = true, HelpText = "Name of the <Manga Name>-info.json file of the manga to download")]
    public string? File { get; set; }

    [Option('v', HelpText = "List of volume names to download, ex. \"Volume 01\" <space> \"Volume 10\"")]
    public IEnumerable<string>? Volumes { get; set; }

    [Option('c', HelpText = "List of VolumeName:ChapterName of chapters to download, ex. \"Volume 01:Capitolo 00.01\" <space> \"Volume 02:Capitolo 04\"")]
    public IEnumerable<string>? Chapters { get; set; }
}
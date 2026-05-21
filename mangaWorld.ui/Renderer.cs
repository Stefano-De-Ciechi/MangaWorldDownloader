using System.Numerics;
using ImGuiNET;
using MangaWorld.Core;
using MangaWorld.Downloader;
using MangaWorld.Scraper;

namespace mangaWorld.ui;

public class Renderer
{
    private enum AppState
    {
        ToolSelector,
        Scraper,
        Downloader,
    }
    
    private AppState _currentState =  AppState.ToolSelector;
    
    private string _mangaUrl = "";
    private bool _isScaperWorking = false;
    private string _scraperStatusMessage = "Ready to scrape";

    private int _selectedMangaIndex = -1;
    private string[] _mangaFiles = [];
    private Manga? _currentManga = null;
    private string _currentMangaName = "";
    private HashSet<string> _selectedVolumes = [];
    private HashSet<string> _selectedSingleChapters = [];
    

    public Renderer()
    {
        if (!Directory.Exists(MangaSerializer.InfoFolder)) return;
        
        _mangaFiles = Directory.GetFiles(MangaSerializer.InfoFolder)
            .Select(Path.GetFileName)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(fn => fn!.Remove(fn!.IndexOf("-info.json", StringComparison.InvariantCulture), "-info.json".Length))
            .ToArray();
    }
    
    // main Render method (entry point)
    public void Render()
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(displaySize);
        
        const ImGuiWindowFlags flags = 
            ImGuiWindowFlags.NoCollapse | 
            ImGuiWindowFlags.NoMove | 
            ImGuiWindowFlags.NoResize | 
            ImGuiWindowFlags.NoTitleBar;

        ImGui.Begin("MangaWorld Scraper / Downloader", flags);

        switch (_currentState)
        {
            case AppState.ToolSelector:
                RenderToolSelector();
                break;
            case AppState.Scraper:
                RenderScraper();
                break;
            case AppState.Downloader:
                RenderDownloader();
                break;
            default:
                return;
        }
        
        ImGui.End();
        
        //ImGui.ShowMetricsWindow();
    }
    
    private void RenderToolSelector()
    {
        var wSize = ImGui.GetWindowSize();
        
        ImGui.SetCursorScreenPos(new Vector2(wSize.X / 2 - 25, wSize.Y / 2 - 100));
        ImGui.Text("Select Tool:");

        ImGui.SetCursorScreenPos(new Vector2(wSize.X / 2 - 50, wSize.Y / 2 - 50));
        if (ImGui.Button("Manga Scraper", new Vector2(200, 40)))
            _currentState = AppState.Scraper;

        ImGui.SetCursorScreenPos(new Vector2(wSize.X / 2 - 50, wSize.Y / 2));
        if (ImGui.Button("Manga Downloader", new Vector2(200, 40)))
            _currentState = AppState.Downloader;
    }

    private void RenderScraper()
    {
        if (ImGui.ArrowButton("##LeftArrow", ImGuiDir.Left)) _currentState = AppState.ToolSelector;
        ImGui.Separator();
        
        ImGui.InputText("Manga URL", ref _mangaUrl, 2048);
        
        ImGui.SameLine();
        if (ImGui.Button("Paste url from System Clipboard"))
        {
            _mangaUrl = ImGui.GetClipboardText();
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Clear url"))
        {
            _mangaUrl = "";
        }
        
        if (ImGui.Button("Start Scraping"))
        {
            StartBackgroundScraping();
        }
        
        ImGui.SameLine();
        ImGui.Text($"Scraper status: {_scraperStatusMessage}");
    }

    private void StartBackgroundScraping()
    {
        if (string.IsNullOrWhiteSpace(_mangaUrl)) return;
        
        _isScaperWorking = true;
        _scraperStatusMessage = "Scraper started ...";
        
        Task.Run(async () => 
        {
            try
            {
                var infoScraper = new InfoScraper(_mangaUrl);
                var manga = infoScraper.Scrape();

                await MangaSerializer.Serialize(manga);
                _scraperStatusMessage = $"Scraper finished (tot. html pages requested: {infoScraper.TotalPagesRequested})";
            }
            catch (Exception e)
            {
                _scraperStatusMessage = $"Error: {e.Message}";
            }
            finally
            {
                _isScaperWorking = false;
            }
        });
    }

    private void RenderDownloader()
    {
        if (ImGui.ArrowButton("##LeftArrow", ImGuiDir.Left)) _currentState = AppState.ToolSelector;
        ImGui.Separator();

        if (_mangaFiles.Length == 0)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "You haven't scraped any mangas yet...");
            return;
        }

        if (ImGui.Combo("manga-title", ref _selectedMangaIndex, _mangaFiles, _mangaFiles.Length))
        {
            LoadMangaData(_mangaFiles[_selectedMangaIndex]);
        }
        
        ImGui.Spacing();
        ImGui.Separator();

        if (_currentManga == null) return;
        
        ImGui.Columns(2);
        ImGui.SetColumnWidth(0, 400);

        ImGui.BeginChild("Volumes", new Vector2(0, 600));
        foreach (var volume in _currentManga.Volumes)
        {
            if (ImGui.CollapsingHeader($"{volume.Name}"))
            {
                bool isVolumeSelected = _selectedVolumes.Contains(volume.Name);
                string btnLabel = isVolumeSelected ? "De-select volume" : "Select volume";

                if (ImGui.Button($"{btnLabel}##{volume.Name}"))
                {
                    ToggleVolumeSelection(volume, !isVolumeSelected);
                }

                foreach (var chapter in volume.Chapters)
                {
                    string chapId = $"{volume.Name}:{chapter.Name}";
                    bool isChapSelected = _selectedSingleChapters.Contains(chapId);

                    if (ImGui.Checkbox($"{chapter.Name} - {chapter.NumPages} pages##{chapId}", ref isChapSelected))
                    {
                        if (isChapSelected) _selectedSingleChapters.Add(chapId);
                        else _selectedSingleChapters.Remove(chapId);
                    }
                }
            }
        }
        ImGui.EndChild();
        
        ImGui.NextColumn();
        ImGui.BeginChild("functions", new Vector2(0, 600));
        
        if (ImGui.Button("Download Selected Items", new Vector2(-1, 30)))
        {
            ExecuteDownloadCommand();
        }
        
        if (ImGui.Button("Debug Info", new Vector2(-1, 30)))
        {
            PrintSelectedItems();
        }
        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private void LoadMangaData(string mangaName)
    {
        _currentMangaName = mangaName;
        _selectedVolumes.Clear();
        _selectedSingleChapters.Clear();

        try
        {
            Task.Run(async () =>
            {
                _currentManga = await MangaSerializer.Deserialize(mangaName);
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }

    private void ToggleVolumeSelection(Volume volume, bool select)
    {
        if (select)
        {
            _selectedVolumes.Add(volume.Name);
            foreach (var chapt in volume.Chapters)
                _selectedSingleChapters.Add($"{volume.Name}:{chapt.Name}");
        }
        else
        {
            _selectedVolumes.Remove(volume.Name);
            foreach (var chapt in volume.Chapters)
                _selectedSingleChapters.Remove($"{volume.Name}:{chapt.Name}");
        }
    }
    
    private void PrintSelectedItems()
    {
        Console.WriteLine("--- DEBUG INFO ---");
        Console.WriteLine($"Entire Volumes: {string.Join(", ", _selectedVolumes)}");
        Console.WriteLine($"Single Chapters: {string.Join(", ", _selectedSingleChapters)}");
    }

    private void ExecuteDownloadCommand()
    {
        // TODO continue to develop this part...
        #if false
        if (_currentManga == null) return;

        var volumesToDownload = _selectedVolumes.ToList();
        var chaptersToDownload = _selectedSingleChapters.ToList();
        
#endif
        
    }
}
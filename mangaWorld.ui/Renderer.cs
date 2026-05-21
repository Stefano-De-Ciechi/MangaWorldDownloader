using System.Numerics;
using ImGuiNET;
using MangaWorld.Core;
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
    
    // main Render method (entry point)
    public void Render()
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(displaySize);
        
        var flags = 
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
        
        //if (_isScaperWorking) ImGui.BeginDisabled();

        if (ImGui.Button("Start Scraping"))
        {
            StartBackgroundScraping();
        }
        
        ImGui.SameLine();
        ImGui.Text($"Scraper status: {_scraperStatusMessage}");
        
        //if (!_isScaperWorking) ImGui.EndDisabled();
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
    }
}
using MangaWorld.Downloader;

// TODO build a simple GUI app to select volumes and chapter using AvaloniaUI (cross platform - open source)

// example of execution
var downloader = new MangaDownloader("GTO-info.json");
await downloader.DownloadVolume("Volume 01");
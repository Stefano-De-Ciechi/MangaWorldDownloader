using MangaWorld.Downloader;

var downloader = new MangaDownloader();
await downloader.DownloadImage("Berserk", "Volume 01", "Capitolo 00.01", 2, "https://cdn.mangaworld.bz/chapters/berserk-5fb842b84c29e1099b62b0dd/volume-01-5fb844014106670a1da64094/capitolo-0001-5fb844014106670a1da64095/2.png");

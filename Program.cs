﻿using MangaWorld.Scraper;

if (args.Length != 1)
{
    Console.WriteLine("need a link to presentation page of a manga from the site https://www.mangaworld.bz");
    Environment.Exit(-1);
}

string url = args[0];

var infoScaper = new InfoScraper(url);
var manga = infoScaper.Scrape();
var serializer = new MangaSerializer(manga);

await serializer.Serialize();
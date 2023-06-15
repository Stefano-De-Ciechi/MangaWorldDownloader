/*using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;*/

using Scraper;

string url = (args.Length == 1) ? args[0] : "https://www.mangaworld.bz/manga/2246/gto";

var infoScaper = new InfoScraper(url);
infoScaper.Scrape();
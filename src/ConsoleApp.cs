namespace MangaWorld.Downloader;

/*
    class that shows a sort of CLI menu to choose one of these options:
        - download an entire manga
        - download an entire volume
        - download a range of volumes
        - download an entire chapter
        - download a range of chapters

    all of the info are retrieved from the .json file generated for that manga by the InfoScraper
*/
public class ConsoleApp
{
    public static void ShowMenu()       // TODO continue to develop this mess
    {
        bool quitFlag = false;
        do
        {
            Console.WriteLine("""
                ===== Manga Downloader =====

                choose an option:

                1) download an entire manga
                2) download an entire volume
                3) download a range of volumes
                4) download a chapter
                5) download a range of chapters
                6) QUIT
            """);

            int option = Convert.ToInt32(Console.ReadLine());
            
            switch (option)
            {
                case 1:
                    return;
                case 2:
                    return;
                case 3:
                    return;
                case 4:
                    return;
                case 5:
                    return;
                case 6:
                    quitFlag = true;
                    break;
                default:
                    break;
            }
        }
        while(!quitFlag);
    }
}
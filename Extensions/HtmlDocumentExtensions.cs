using HtmlAgilityPack;

public static class HtmlDocumentExtensions
{
    public static HtmlNode GetElementByClassName(this HtmlNode document, string className)
    {
        //return document.SelectSingleNode($"//div[contains(@class, '{ className }')]");
        var res = (
            from node in document.Descendants()
            where node.HasClass(className)
            select node
        ).First();
        
        return res;
    }

    public static IEnumerable<HtmlNode> GetElementsByClassName(this HtmlNode document, string className)
    {
        //return document.SelectNodes($"//div[contains(@class, '{ className }')]");
        var res = 
            from node in document.Descendants()
            where node.HasClass(className)
            select node;

        return res;
    }

    public static HtmlNode GetElementByTagName(this HtmlNode document, string tagName)
    {
        //return document.SelectNodes($"//div[contains(@class, '{ className }')]");
        var res = (
            from node in document.Descendants()
            where node.Name == tagName
            select node
        ).First();

        return res;
    }

    public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode document, string tagName)
    {
        //return document.SelectNodes($"//div[contains(@class, '{ className }')]");
        var res = 
            from node in document.Descendants()
            where node.Name == tagName
            select node;

        return res;
    }
}
using HtmlAgilityPack;

public static class HtmlDocumentExtensions
{
    public static HtmlNode GetElementByClassName(this HtmlNode document, string className)
    {
        return GetElementsByClassName(document, className).First();
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
        return GetElementsByTagName(document, tagName).First();
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
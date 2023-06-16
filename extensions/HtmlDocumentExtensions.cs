using HtmlAgilityPack;

public static class HtmlnodeExtensions
{
    public static HtmlNode GetElementByClassName(this HtmlNode node, string className)
    {
        return GetElementsByClassName(node, className).First();
    }

    public static IEnumerable<HtmlNode> GetElementsByClassName(this HtmlNode node, string className)
    {
        //return node.SelectNodes($"//div[contains(@class, '{ className }')]");
        var res = 
            from n in node.Descendants()
            where n.HasClass(className)
            select n;

        return res;
    }

    public static HtmlNode GetElementByTagName(this HtmlNode node, string tagName)
    {
        return GetElementsByTagName(node, tagName).First();
    }

    public static IEnumerable<HtmlNode> GetElementsByTagName(this HtmlNode node, string tagName)
    {
        //return node.SelectNodes($"//div[contains(@class, '{ className }')]");
        var res = 
            from n in node.Descendants()
            where n.Name == tagName
            select n;

        return res;
    }

    public static HtmlNode GetElementById(this HtmlNode node, string id)
    {
        return node.SelectSingleNode($"//*[@id='{ id }']");
    }
}
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Linq;

namespace Url2Ringtone
{
    public static class ExtensionMethods
    {
        public static string GetTitle(this string html)
        {
            if (html.Length == 0)
                return string.Empty;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (doc == null)
                return string.Empty;
            
            var titleNode = doc.DocumentNode.Descendants("title").FirstOrDefault();
            if (titleNode == null) return "";

            return titleNode.InnerText;            
        }
    }
}

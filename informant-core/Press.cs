using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UntisExp
{
	public class Press
	{
		public Press ()
		{
		}
		public async Task<List<News>> getNews(){
            XDocument doc;
            var r = new Regex("<.*?>");
            var gathered = new List<News>();
            XNamespace namespaces;
            XNamespace medians;
            await Task.Run(() => {
                doc = XDocument.Load(VConfig.feed);
                namespaces = XNamespace.Get("http://purl.org/rss/1.0/modules/content/");
                medians = XNamespace.Get("http://search.yahoo.com/mrss/");
                var articles = from article in doc.Descendants("item")
                               select article;
                foreach (XElement articlet in articles)
                {
                    var writing = new News();
                    var titleQuery = from titles in articlet.Descendants("title")
                                     select titles.Value;
                    writing.Title = titleQuery.First();
                    string content;
                    try
                    {
                        var contQuery = from item in articlet.Descendants(namespaces + "encoded")
                                        select item.Value;
                        content = WebUtility.HtmlDecode(contQuery.First());
                    }
                    catch {
                        var contQuery = from item in articlet.Descendants("description")
                                        select item.Value;
                        content = WebUtility.HtmlDecode(contQuery.First());
                    }
                    writing.Content = r.Replace(content, "");
                    var linkQuery = from link in articlet.Descendants("link")
                                    select link.Value;
                    writing.Source = new Uri(linkQuery.First());
                    var mediaQuery = from medias in articlet.Descendants(medians + "content")
                                     select medias.Attribute("url").Value;
                    writing.Image = mediaQuery.ElementAtOrDefault(1);
                    gathered.Add(writing);
                }
            });
            return gathered;
		}

		public void wrong(string aa, string bb, string cc) {}

		/// <summary>
		/// Remove HTML tags from string using char array.
		/// </summary>
		public string StripTagsCharArray(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}
	}
}


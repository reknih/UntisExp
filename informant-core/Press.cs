using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UntisExp
{
	public class Press
	{
        XNamespace namespaces = XNamespace.Get("http://purl.org/rss/1.0/modules/content/");
        XNamespace medians = XNamespace.Get("http://search.yahoo.com/mrss/");
        Regex r = new Regex("<.*?>");
        Action<List<News>> newscallback;
		public Press ()
		{
		}
        /// <summary>
        /// Gets news articles from the RSS feed specified in VConfig <seealso cref="VConfig"/>
        /// </summary>
        /// <returns>List of news articles (asynchronous)</returns>
        public async Task<List<News>> getNews()
        {
            XDocument doc;
            var gathered = new List<News>();
            await Task.Run(() =>
            {
                doc = XDocument.Load(VConfig.feed);
                var articles = from article in doc.Descendants("item")
                               select article;
                foreach (XElement articlet in articles)
                {
                    var writing = processXML(articlet, doc);
                    gathered.Add(writing);
                }
            });
            return gathered;
        }
        public void getCalledBackForNews(Action<List<News>> _newscallback)
        {
            newscallback = _newscallback;
            Networking.DownloadLegacyStream(VConfig.feed, newsStreamCallback, alarmDummy);
        }
        public void newsStreamCallback(Stream newsstream) {
            var doc = XDocument.Load(newsstream);
            var articles = from article in doc.Descendants("item")
                           select article;
            var gathered = new List<News>();
            foreach (XElement articlet in articles)
            {
                var writing = processXML(articlet, doc);
                gathered.Add(writing);
            }
            newscallback(gathered);
        }

        public void alarmDummy(string s1, string s2, string s3) { }

        protected News processXML(XElement articlet, XDocument doc)
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
            catch
            {
                var contQuery = from item in articlet.Descendants("description")
                                select item.Value;
                content = WebUtility.HtmlDecode(contQuery.First());
            }
            writing.Content = r.Replace(content, "");
            var linkQuery = from link in articlet.Descendants("link")
                            select link.Value;
            writing.Source = new Uri(linkQuery.First());
            writing.Summary = Helpers.TruncateWithPreservation(writing.Content, 30);
            var mediaQuery = from medias in articlet.Descendants(medians + "content")
                             select medias.Attribute("url").Value;
            writing.Image = mediaQuery.ElementAtOrDefault(1);
            writing.Refresh();
            return writing;
        }

		protected void wrong(string aa, string bb, string cc) {}

		/// <summary>
		/// Remove HTML tags from string using char array.
		/// </summary>
		protected string StripTagsCharArray(string source)
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


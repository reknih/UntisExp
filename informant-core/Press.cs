﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UntisExp
{
    /// <summary>
    /// Is equipped with methods to fetch news objects from the RSS feed specified in 
    /// </summary>
	public class Press
	{
	    readonly XNamespace _namespaces = XNamespace.Get("http://purl.org/rss/1.0/modules/content/");
	    readonly XNamespace _mediaNs = XNamespace.Get("http://search.yahoo.com/mrss/");
	    readonly Regex _r = new Regex("<.*?>");
	    private readonly INetworkAccessor _networkAccessor;
#if (WINDOWS || WINDOWS_PHONE || DEBUG)
        Action<List<News>> _newscallback;
#endif

        /// <summary>
        /// Creates a new Press object
        /// </summary>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
	    public Press(INetworkAccessor networkAccessor = null)
	    {
	        if (networkAccessor == null) networkAccessor = new Networking();
	        _networkAccessor = networkAccessor;
	    }    
        /// <summary>
        /// Gets news articles from the RSS feed specified in VConfig <seealso cref="VConfig"/>
        /// </summary>
        /// <returns>List of news articles (asynchronous)</returns>
        public async Task<List<News>> GetNews()
        {
            XDocument doc;
            var gathered = new List<News>();
            await Task.Run(() =>
            {
                doc = XDocument.Load(VConfig.Feed);
                var articles = from article in doc.Descendants("item")
                               select article;
                gathered.AddRange(articles.Select(ProcessXml));
            });
            return gathered;
        }
#if (WINDOWS || WINDOWS_PHONE || DEBUG)
        /// <summary>
        /// Will callback with the news articles from the RSS feed specified in <seealso cref="VConfig"/> as an parameter
        /// </summary>
        /// <param name="newscallbackAction">The callback which is called after execution</param>
        public void GetCalledBackForNews(Action<List<News>> newscallbackAction)
        {
            _newscallback = newscallbackAction;
            _networkAccessor.DownloadLegacyStream(VConfig.Feed, NewsStreamCallback);
        }

	    private void NewsStreamCallback(Stream newsstream) {
            var doc = XDocument.Load(newsstream);
            var articles = from article in doc.Descendants("item")
                           select article;
            var gathered = articles.Select(ProcessXml).ToList();
	        _newscallback(gathered);
        }
#endif

	    private News ProcessXml(XElement articlet)
        {
            var writing = new News();
            var titleQuery = from titles in articlet.Descendants("title")
                             select titles.Value;
            writing.Title = titleQuery.First();
            string content;
            try
            {
                var contQuery = from item in articlet.Descendants(_namespaces + "encoded")
                                select item.Value;
                content = WebUtility.HtmlDecode(contQuery.First());
            }
            catch
            {
                var contQuery = from item in articlet.Descendants("description")
                                select item.Value;
                content = WebUtility.HtmlDecode(contQuery.First());
            }
            writing.Content = _r.Replace(content, "");
            var linkQuery = from link in articlet.Descendants("link")
                            select link.Value;
            writing.Source = new Uri(linkQuery.First());
            writing.Summary = Helpers.TruncateWithPreservation(writing.Content, 30);
            var mediaQuery = from medias in articlet.Descendants(_mediaNs + "content")
                             select medias.Attribute("url").Value;
            writing.Image = mediaQuery.ElementAtOrDefault(0);
            writing.Refresh();
            return writing;
        }
	}
}


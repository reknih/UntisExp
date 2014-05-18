using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace UntisExp
{
	public class Press
	{
		public Press ()
		{
		}
		public void getNews(){
			Networking.DownloadData ("http://benjaminlmoore.wordpress.com/feed/", understand, wrong);
		}

		public void wrong(string aa, string bb, string cc) {}

		protected void understand (string informantion) {
			XDocument doc = XDocument.Parse (informantion);
			var articles = from article in doc.Descendants ("item")
				select article.Elements();
			foreach (XElement articlet in articles) {
				var titleQuery = from titles in articlet.Descendants ("title")
					select titles;
				int i = 0;
			}
		}
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


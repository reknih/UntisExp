using System;
using System.Net;

namespace UntisExp
{
    /// <summary>
    /// Represents a news article
    /// </summary>
	public class News
	{
        /// <summary>
        /// Generates new empty News object
        /// </summary>
		public News ()
		{
		}
        /// <summary>
        /// Generates a prefilled news object
        /// </summary>
        /// <param name="_ID">ID of the article. Can be 0</param>
        /// <param name="_Title">Title of the article, for display</param>
        /// <param name="_source">URI of article</param>
        /// <param name="_summary">Short summary. Will be autofilled if "".</param>
        /// <param name="_Image">URI of article's image (mandantory)</param>
        /// <param name="_Content">Articles content</param>
		public News(int _ID, string _Title, Uri _source, string _summary, string _Image, string _Content = "")
		{
			ID = _ID;
			Title = _Title;
			Source = _source;
			Summary = _summary;
			Image = _Image;
			Content = _Content;
            Refresh();
		}
		public int ID { get; set; }
        /// <summary>
        /// Human-readable title of the article
        /// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Website of it
        /// </summary>
		public Uri Source { get; set; }
        public string SourcePrint { get; set; }
        /// <summary>
        /// Short excerpt or similar. Will be created automatically if not given
        /// </summary>
		public string Summary { get; set; }
        /// <summary>
        /// URI for article's image. Mandantory for visual rendering in most cases
        /// </summary>
		public string Image { get; set; }
        /// <summary>
        /// No comment.
        /// </summary>
		public string Content { get; set; }

        /// <summary>
        /// Generates Summary, if empty
        /// </summary>
        public void Refresh() {
            if (Summary == "") {
                Summary = Helpers.TruncateWithPreservation(Content, 50);
            }
            if (Source.AbsoluteUri.IndexOf(VConfig.url) != -1)
            {
                SourcePrint = "CHRISTIAN-WIRTH-SCHULE";
            }
            else
            {
                SourcePrint = "SR-BLOG";
            }
        }
	}
}


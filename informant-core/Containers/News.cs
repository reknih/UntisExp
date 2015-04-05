using System;

namespace UntisExp.Containers
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
        /// <param name="id">ID of the article. Can be 0</param>
        /// <param name="title">Title of the article, for display</param>
        /// <param name="source">URI of article</param>
        /// <param name="summary">Short summary. Will be autofilled if "".</param>
        /// <param name="image">URI of article's image (mandantory)</param>
        /// <param name="content">Articles content</param>
		public News(int id, string title, Uri source, string summary, string image, string content = "")
		{
			Id = id;
			Title = title;
			Source = source;
			Summary = summary;
			Image = image;
			Content = content;
            Refresh();
		}
        /// <summary>
        /// A number
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
		public int Id { get; set; }
        /// <summary>
        /// Human-readable title of the article
        /// </summary>
		public string Title { get; set; }
        /// <summary>
        /// Website of it
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
		public Uri Source { get; set; }
        /// <summary>
        /// A on-screen representation of the <see cref="News.Source"/> URI
        /// </summary>
        public string SourcePrint { get; private set; }
        /// <summary>
        /// Short excerpt or similar. Will be created automatically if not given
        /// </summary>
		public string Summary { get; set; }
        /// <summary>
        /// URI for article's image. Mandantory for visual rendering in most cases
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
		public string Image { get; set; }
        /// <summary>
        /// No comment.
        /// </summary>
		public string Content { get; set; }

        /// <summary>
        /// Generates Summary, if empty
        /// </summary>
        public void Refresh() {
            if (string.IsNullOrEmpty(Summary) && !string.IsNullOrEmpty(Content)) {
                Summary = Helpers.TruncateWithPreservation(Content, 50);
            }
            if (Source == null)
            {
                return;
            }
            SourcePrint = Source.AbsoluteUri.IndexOf(VConfig.Url, StringComparison.Ordinal) != -1 ? "CHRISTIAN-WIRTH-SCHULE" : "SR-BLOG";
        }
	}
}


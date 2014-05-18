using System;

namespace UntisExp
{
	public class News
	{
		public News ()
		{
		}
		public News(int _ID, string _Title, string _source, string _summary, string _Image = "default", string _Content = "")
		{
			ID = _ID;
			Title = _Title;
			Source = _source;
			Summary = _summary;
			Image = _Image;
			Content = _Content;
		}
		public int ID { get; set; }
		public string Title { get; set; }
		public string Source { get; set; }
		public string Summary { get; set; }
		public string Image { get; set; }
		public string Content { get; set; }
	}
}


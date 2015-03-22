using System;
using System.Collections.Generic;

namespace UntisExp
{
	public class InterstitialFetching
	{
		public int outerLoopCursor = 0;
		public List<Data> parsedRows;
		public News parsedNews;
		public bool dontImmediatelyRefresh = false;

		public InterstitialFetching (int _outerLoopCursor, List<Data> _parsedRows)
		{
			outerLoopCursor = _outerLoopCursor;
			parsedRows = _parsedRows;
		}
		public InterstitialFetching () {
		}
	}
}


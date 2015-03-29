using System.Collections.Generic;

namespace UntisExp
{
	public class InterstitialFetching
	{
		public int OuterLoopCursor;
		public List<Data> ParsedRows;
		public News ParsedNews;
		public bool DontImmediatelyRefresh;

		public InterstitialFetching (int outerLoopCursor, List<Data> parsedRows)
		{
		    DontImmediatelyRefresh = false;
		    OuterLoopCursor = outerLoopCursor;
			ParsedRows = parsedRows;
		}
		public InterstitialFetching ()
		{
		    DontImmediatelyRefresh = false;
		    OuterLoopCursor = 0;
		}
	}
}


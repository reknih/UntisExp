using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UntisExp
{
    /// <summary>
    /// Contains a static method to process a table of a day and, when instanciated, will carry the result of these methods
    /// </summary>
	public class InterstitialFetching
	{
        /// <summary>
        /// Which day the function is iterating through
        /// </summary>
		public int OuterLoopCursor;
        /// <summary>
        /// The parsed <see cref="UntisExp.Data"/> entries out of the table 
        /// </summary>
		public List<Data> ParsedRows;
        /// <summary>
        /// The parsed <see cref="UntisExp.News"/> entry out of the table
        /// </summary>
		public News ParsedNews;
        /// <summary>
        /// Whether the caller has to get another table from the network
        /// </summary>
		public bool HasToGetSecondSchedule;

        /// <summary>
        /// Given a string representing a table of a day in WebUntis' HTML, <see cref="ProcessRow"/> will return an object including the <see cref="UntisExp.Data"/> representations of each schedule value
        /// </summary>
        /// <returns>Object containing the current progress of parsing and the last <see cref="UntisExp.Data"/>-objects that were parsed</returns>
        /// <param name="item">The HTML string representing the table of a day</param>
        /// <param name="iOuter">The progress through the day tables</param>
        /// <param name="mode">The background operations mode. See also <seealso cref="UntisExp.Fetcher"/></param>
        /// <param name="silent">Whether the task will add headings</param>
        /// <param name="daysAndNewsBoxes">The number of news tables in the week</param>
        /// <param name="passDontImmediatelyRefresh">If appropriate, this value will be passed to <see cref="HasToGetSecondSchedule"/>.</param>
        /// <param name="activity">The action which should be performed.</param>
        public static InterstitialFetching ProcessRow(string item, int iOuter, int daysAndNewsBoxes, int mode, bool silent, bool passDontImmediatelyRefresh, Activity activity = Activity.ParseFirstSchedule)
        {
            List<Data> v1 = new List<Data>();
            InterstitialFetching result = new InterstitialFetching();
            result.HasToGetSecondSchedule = passDontImmediatelyRefresh;
            int daysRec = 0;
            if (item.IndexOf(VConfig.SearchNoAccess, StringComparison.Ordinal) == -1)
            {
                string it = item.Replace("&nbsp;", String.Empty);
                MatchCollection mc;
                string searchInFront;
                News news = null;
                if (activity == Activity.GetNews)
                {
                    searchInFront = "<tr>";
                    news = new News { Image = "http://centrallink.de/sr/Blackboard.png", Source = new Uri(VConfig.Url) };
                }
                else
                {
                    searchInFront = "<trclass='list";
                }
                if ((item.IndexOf(VConfig.NoEventsText.Replace(" ", string.Empty), StringComparison.Ordinal) == -1) || activity == Activity.GetNews)
                {
                    int iterations = 0;
                    it = it.Substring(it.IndexOf("</tr>", StringComparison.Ordinal) + 5, it.Length - it.IndexOf("</tr>", StringComparison.Ordinal) - 5);
                    while (it.IndexOf(searchInFront, StringComparison.Ordinal) != -1)
                    {
                        if (iterations == 0)
                        {
                            // news box should not be a day so we count days here
                            daysRec++;
                        }
                        if (activity == Activity.GetNews)
                        {
                            if (news != null && news.Summary != null)
                            {
                                news.Summary += "\n\n";
                            }
                            DateTime date = GetDateFromDay(iOuter, Activity.ParseFirstSchedule);
                            string dateName = new CultureInfo("de-DE").DateTimeFormat.GetDayName(date.DayOfWeek);
                            if (news != null) news.Summary += dateName + ", " + date.Day + "." + date.Month + ":\n";
                        }
                        Data data = new Data();
                        string w = it.Substring(it.IndexOf(searchInFront, StringComparison.Ordinal));
                        w = w.Substring(0, w.IndexOf("</tr>", StringComparison.Ordinal));
                        it = it.Substring(it.IndexOf("</tr>", StringComparison.Ordinal) + 5, it.Length - it.IndexOf("</tr>", StringComparison.Ordinal) - 5);
                        mc = VConfig.CellSearch.Matches(w);
                        int webColumn = 0;
                        foreach (var thing in mc)
                        {
                            string compute = PrepareScheduleItem(thing);
                            if (activity != Activity.GetNews)
                            {
                                data = ProceedScheduleItem(compute, data, webColumn, iterations, silent, v1);
                                webColumn++;
                            }
                            else
                            {
                                news = ProcessNewsItem(compute, news);
                                result.ParsedNews = news;
                            }
                        }
                        if (activity != Activity.GetNews)
                        {
                            data.Refresh();
                            if ((mode == 1 && daysRec == 2) || (mode != 1 && mode != 0) || (mode == 0 && daysRec == 1))
                                v1.Add(data);
                        }
                        iterations++;
                    }
                    if ((iterations == 0 && activity != Activity.GetNews) || (iterations > 0 && activity == Activity.GetNews))
                    {
                        iOuter--;
                    }
                }
                else
                {
                    daysRec++;
                    if (!silent)
                    {
                        //Adds Date
                        v1.Add(new Data(GetDateFromDay(iOuter, activity)));
                        //Adds no events message
                        v1.Add(new Data());
                    }
                }
            }
            iOuter++;
            if (iOuter == daysAndNewsBoxes && (daysRec == 1) && mode != 0 && activity == Activity.ParseFirstSchedule)
            {
                result.HasToGetSecondSchedule = true;
            }
            result.OuterLoopCursor = iOuter;
            result.ParsedRows = v1;
            return result;
        }

        private static string PrepareScheduleItem(object input)
        {
            string thingy = input.ToString();
            return thingy.Substring(thingy.IndexOf(">", StringComparison.Ordinal) + 1, thingy.LastIndexOf("<", StringComparison.Ordinal) - thingy.IndexOf(">", StringComparison.Ordinal) - 1);
        }

        private static News ProcessNewsItem(string thingy, News scheduleNews)
        {
            scheduleNews.Title = "Vom Vertretungsplan:";
            scheduleNews.Summary += Helpers.AddSpaces(thingy);
            scheduleNews.Content = scheduleNews.Summary;
            return scheduleNews;
        }

        private static Data ProceedScheduleItem(string thingy, Data individualEntry, int webColumn, int iteration, bool silent, ICollection<Data> rowsData)
        {

            switch (webColumn)
            {
                case 0:
                    if (thingy == VConfig.SpecialEvtAb)
                    { individualEntry.Event = true; }
                    break;
                case 1:
                    int day = Convert.ToInt16(thingy.Substring(0, thingy.IndexOf(".", StringComparison.Ordinal)));
                    string dayStr = thingy.Substring(thingy.IndexOf(".", StringComparison.Ordinal) + 1);
                    dayStr = dayStr.Replace(".", string.Empty);
                    int month = Convert.ToInt16(dayStr);
                    int year = DateTime.Now.Year;
                    DateTime dt = new DateTime(year, month, day);
                    individualEntry.Date = dt;
                    if (iteration == 0 && !silent)
                    {
                        rowsData.Add(new Data(dt));
                    }
                    break;
                case 2:
                    individualEntry.Lesson = thingy;
                    break;
                case 3:
                    individualEntry.Cover = thingy;
                    break;
                case 4:
                    individualEntry.Subject = thingy;
                    break;
                case 5:
                    individualEntry.OldSubject = thingy;
                    break;
                case 6:
                    individualEntry.Room = thingy;
                    break;
                case 7:
                    individualEntry.Group = thingy;
                    break;
                case 8:
                    individualEntry.Teacher = thingy;
                    break;
                case 13:
                    individualEntry.Notice = thingy;
                    break;
                case 14:
                    individualEntry.OutageStr = thingy;
                    break;
                case 15:
                    individualEntry.CareStr = thingy;
                    break;
            }
            return individualEntry;
        }


        /// <summary>
        /// Will get a day object out of an int
        /// </summary>
        /// <returns>The date object for the day of week</returns>
        /// <param name="day">Value representing the wished day (0=mo,1=tu...)</param>
        /// <param name="activity">Which activity to perform</param>
        private static DateTime GetDateFromDay(int day, Activity activity)
        {
            DateTime date = DateTime.Now;
            while (date.DayOfWeek != DayOfWeek.Monday)
            {
                if (activity == Activity.ParseFirstSchedule)
                {
                    date = date.AddDays(-1);
                }
                else
                {
                    date = date.AddDays(1);
                }
            }
            return date.AddDays(day);
        }

        /// <summary>
        /// Will construct a new empty object with default values for <see cref="UntisExp.InterstitialFetching.OuterLoopCursor"/> and <see cref="UntisExp.InterstitialFetching.HasToGetSecondSchedule"/>
        /// </summary>
		public InterstitialFetching ()
		{
		    HasToGetSecondSchedule = false;
		    OuterLoopCursor = 0;
		}
	}
}


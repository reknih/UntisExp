using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UntisExp
{
    /// <summary>
    /// This class provides methods for getting the info from WebUntis
    /// </summary>
    public class Fetcher
    {
		// Actions to be called back on lifetime events
		private Action clearView;
        private Action<String, String, String> alert;
        private Action<List<Data>> refreshAll;
		private Action<News> addTheNews;
        private Action<List<Group>> refreshSet;

		/// <summary>
		/// Collections of retreived <see cref="UntisExp.Data"/>
		/// </summary>
        private List<Data> globData;

		/// <summary>
		/// Group to query WebUntis for
		/// </summary>
		protected int Group;

		/// <summary>
		/// Whether the method alerts if it fails or not
		/// </summary>
        private bool silent;

		/// <summary>
		/// MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles
		/// </summary>
        private int mode = 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class which is pre-equipped for background operation. Will surpress all rows which do not contain a timetable element, e.g. the date row. Useful for background services.
		/// </summary>
		/// <param name="Stop">Callback for the case that the fetching failed. Useful to stop the background service</param>
		/// <param name="_refeshAll">Callback function that will be called once the <see cref="UntisExp.Fetcher.getTimes"/> function and all of its callbacks are finished</param>
		/// <param name="_mode">The mode of operation: MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles</param>
        public Fetcher(Action Stop, Action<List<Data>> _refeshAll, int _mode)
        {
            alert = delegate(string a, string b, string c) { Stop(); };
            refreshAll = _refeshAll;
            mode = _mode;
            silent = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="_del">Callback function name for clearing the current list</param>
        /// <param name="_alert">Callbck function name for displaying an error with title, text and dismiss-button</param>
        /// <param name="_refeshAll">Callback function for updating the view with a List of event entries.</param>
        /// <param name="_refreshOne">Callback function for adding one event entry to the view.</param>
        /// <param name="_refreshSet">Callback function for updating the view with a List of group entries.</param>
        public Fetcher(Action _del, Action<String, String, String> _alert, Action<List<Data>> _refeshAll, Action<Data> _refreshOne, Action<List<Group>> _refreshSet=null)
        {
            clearView = _del;
            alert = _alert;
            refreshAll = _refeshAll;
            refreshSet = _refreshSet;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="_alert">Callbck function name for displaying an error with title, text and dismiss-button</param>
        /// <param name="_refeshAll">Callback function for updating the view with a List of entries.</param>
        public Fetcher(Action<String, String, String> _alert, Action<List<Group>> _refeshAll)
        {
            alert = _alert;
            refreshSet = _refeshAll;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class which will instantly checks for news in the schedule
		/// </summary>
		/// <param name="_refreshOne">Callback for returning one element</param>
		/// <param name="group">Which group to query for. It is kind of the same for all numbers, most of the time. Default is <c>5</c></param>
		/// <param name="week">Week to query news for. For debugging purposes. Will default to <c>-1</c> which means that the current schedule will be read.</param>
		public Fetcher(Action<News> _refreshOne, int group = 5, int week = -1)
		{
			addTheNews = _refreshOne;
			alert = delegate(string arg1, string arg2, string arg3) {
			};
			getTimes (group, Activity.getNews, week);
		}

			
        /// <summary>
        /// Downloads a list of groups asynchronously. Uses the callbacks from the constructor.
        /// </summary>
        public void getClasses()
        {
			Networking.DownloadData (VConfig.url + VConfig.pathToNavbar, groups_DownloadStringCompleted, alert, null, VConfig.groupIErrorTtl, VConfig.groupIErrorTxt, VConfig.groupIErrorBtn);
        }

		/// <summary>
		/// Will parse the site for groups and call <see cref="UntisExp.Fetcher.refreshSet"/> once finished
		/// </summary>
		/// <param name="res">The HTML string</param>
        private void groups_DownloadStringCompleted(String res)
        {
            try
            {
                List<Group> groupObj = new List<Group>();
                string raw = res.Replace(" ", string.Empty);

                raw = raw.Substring(raw.IndexOf("varclasses=[") + "varclasses=[".Length);
                raw = raw.Substring(0, raw.IndexOf("];"));
                raw = raw.Replace("\"", string.Empty);
                raw = raw.Replace("\n", string.Empty);
                string[] arr = raw.Split(',');
                int i = 0;
                foreach (var item in arr)
                {
                    groupObj.Add(new Group(i, item));
                    i++;
                }
                refreshSet(groupObj);
            }
            catch
            {}
        }
        
        /// <summary>
        /// Downloads a list of events for the given group
        /// </summary>
        /// <param name="group">The group number</param>
        /// <param name="follow">Optonal. If set to <c>true</c> the next week is processed. Some changes for processsing apply.</param>
        /// <param name="week">Optional, if it's not set, the current week will be fetched. Sets the calender week to get.</param>
		public void getTimes(int group, Activity activity = Activity.ParseFirstSchedule, int week = -1)
        {

            Group = group;
            string groupStr = "";
            string weekStr = "";
			int length = group.ToString ().Length;
			groupStr = "w";
			if (group == 0)
				return;
			for (int i = 0; i < (5 - length); i++) {
				groupStr = groupStr + "0";
			}
			groupStr = groupStr + Convert.ToString (group);
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            if (week == -1)
                week = cal.GetWeekOfYear(DateTime.Now, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
			if (activity == Activity.ParseSecondSchedule) {
				week++;
			}
            if (week < 10)
            {
                weekStr = "0" + Convert.ToString(week);
            }
            else if (week < 100)
            {
                weekStr = Convert.ToString(week);
            }

			var nav = VConfig.url + weekStr + "/w/" + groupStr + ".htm";
			if (activity == Activity.ParseSecondSchedule) {
				Networking.DownloadData (nav, timesNext_DownloadStringCompleted, alert, abort);
			} else if (activity == Activity.ParseFirstSchedule) {
				Networking.DownloadData (nav, times_DownloadStringCompleted, alert, null, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			} else if (activity == Activity.getNews) {
				Networking.DownloadData (nav, news_DownloadStringCompleted, alert, null, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			}
		}

		/// <summary>
		/// Will be called if subsequent downloads of <see cref="UntisExp.Fetcher.getTimes"/> fail. Uses <see cref="UntisExp.Fetcher.refreshAll"/> callback to write previous elements to the front-end
		/// </summary>
		protected void abort () {
			refreshAll (globData);
		}

		/// <summary>
		/// Counts how many news boxes are present in the WebUntis schedule
		/// </summary>
		/// <returns>The number of the newsboxes.</returns>
		/// <param name="input">The WebUntis HTML string</param>
		protected int getNewsBoxesLength(string input) {
			string haystack = input;
			string needle = VConfig.titleOfMsgBox.Replace(" ", string.Empty);
			return (haystack.Length - input.Replace(needle, "").Length) / needle.Length;
		}

		/// <summary>
		/// Splits the WebUntis site into a array of day tables
		/// </summary>
		/// <returns>An array of the <c>string<c/>s representing a table of a day</returns>
		/// <param name="input">The WebUntis HTML string</param>
		/// <param name="tables">How many tables to search for</param>
		protected string [] getDayArray(string input, int tables)
		{
			string[] result = new string[tables];
			for (int i = 0; i < tables; i++)
			{
				result[i] = input.Substring(input.IndexOf("<table"), input.IndexOf("</table>") - input.IndexOf("<table") + 8);
				input = input.Substring(input.IndexOf("</table>") + 8);
			}
			return result;
		}

		/// <summary>
		/// Given a string representing a table of a day in WebUntis' HTML, <see cref="UntisExp.Fetcher.processRow"/> will return an object including the <see cref="UntisExp.Data"/> representations of each schedule value
		/// </summary>
		/// <returns>Object containing the current progress of parsing and the last <see cref="UntisExp.Data"/>-objects that were parsed</returns>
		/// <param name="item">The HTML string representing the table of a day</param>
		/// <param name="iOuter">The progress through the day tables</param>
		/// <param name="daysAndNewsBoxes">The number of news tables in the week</param>
		/// <param name="passDontImmediatelyRefresh">If appropriate, this value will be passed to <see cref="InterstitialFetching.DontImmediatelyRefresh"/>.</param>
		/// <param name="activity">The action which should be performed.</param>
		protected InterstitialFetching processRow(string item, int iOuter, int daysAndNewsBoxes, bool passDontImmediatelyRefresh, Activity activity = Activity.ParseFirstSchedule)
		{
			string it = item;
			List<Data> v1 = new List<Data> ();
			InterstitialFetching result = new InterstitialFetching();
			result.DontImmediatelyRefresh = passDontImmediatelyRefresh;
			int daysRec = 0;
			if (item.IndexOf(VConfig.searchNoAccess) == -1)
			{
				it = item.Replace("&nbsp;", String.Empty);
				MatchCollection mc;
				string searchInFront;
				News news = null;
				if (activity == Activity.getNews)
				{
					searchInFront = "<tr>";
					if (news == null)
					{
						news = new News ();
						news.Image = "http://centrallink.de/sr/Blackboard.png";
						news.Source = new Uri (VConfig.url);
					}
				} else {
					searchInFront = "<trclass='list";
				}
				if ((item.IndexOf(VConfig.noEventsText.Replace(" ", string.Empty)) == -1)||activity==Activity.getNews)
				{
					int iterations = 0;
					it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
					while (it.IndexOf(searchInFront) != -1)
					{
                        if (iterations == 0)
                        {
                            // news box should not be a day so we count days here
                            daysRec++;
                        }
						if (activity == Activity.getNews ) {
                            if (news.Summary != null)
                            {
                                news.Summary += "\n\n";
                            }
                            DateTime date = getDateFromDay(iOuter, Activity.ParseFirstSchedule);
                            string dateName = new System.Globalization.CultureInfo("de-DE").DateTimeFormat.GetDayName(date.DayOfWeek);
                            news.Summary += Helpers.AddSpaces(dateName + ", " + date.Day + "."+date.Month+":\n");
						}
					    Data data = new Data();
						string w = it.Substring(it.IndexOf(searchInFront));
						w = w.Substring(0, w.IndexOf("</tr>"));
						it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
						mc = VConfig.cellSearch.Matches(w);
						int webColumn = 0;
						foreach (var thing in mc)
						{
							string compute = prepareScheduleItem (thing);
							if (activity != Activity.getNews) {
								data = proceedScheduleItem (compute, data, webColumn, iterations, v1);
								webColumn++;
							} else {
								news = processNewsItem (compute, news);
								result.ParsedNews = news;
							}
						}
						if (activity != Activity.getNews) {
							data.refresh();
							if ((mode == 1 && daysRec == 2)|| (mode!= 1 && mode != 0) || (mode == 0 && daysRec == 1))
							v1.Add(data);
						}
						iterations++;
					}
					if ((iterations == 0 && activity != Activity.getNews) || (iterations > 0 && activity == Activity.getNews))
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
                        v1.Add(new Data(getDateFromDay(iOuter,activity)));
                        //Adds no events message
                        v1.Add(new Data());

                    }
                }
			}
			iOuter++;
			if (iOuter == daysAndNewsBoxes && (daysRec == 1) && mode != 0 && activity == Activity.ParseFirstSchedule)
			{
				getTimes(Group, Activity.ParseSecondSchedule);
				globData = v1;
				result.DontImmediatelyRefresh = true;
			}
			result.OuterLoopCursor = iOuter;
			result.ParsedRows = v1;
			return result;
		}

        /// <summary>
		/// Will get a day object out of an int
        /// </summary>
		/// <returns>The date object for the day of week</returns>
		/// <param name="day">Value representing the wished day (0=mo,1=tu...)</param>
        /// <param name="activity">Which activity to perform</param>
        protected DateTime getDateFromDay(int day, Activity activity)
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
			
		protected string prepareScheduleItem(object input)
		{
			string thingy = input.ToString();
			return thingy.Substring(thingy.IndexOf(">") + 1,thingy.LastIndexOf("<")-thingy.IndexOf(">")-1);
		}

		protected News processNewsItem (string thingy, News scheduleNews) {
			scheduleNews.Title = "Vom Vertretungsplan:";
			scheduleNews.Summary += Helpers.AddSpaces(thingy);
			scheduleNews.Content = scheduleNews.Summary;
			return scheduleNews;
		}

		protected Data proceedScheduleItem (string thingy, Data individualEntry, int webColumn, int iteration, ICollection<Data> rowsData) {
			
			switch (webColumn)
			{
			case 0:
				if (thingy == VConfig.specialEvtAb)
				{ individualEntry.Veranstaltung = true; }
				break;
			case 1:
				int day = Convert.ToInt16(thingy.Substring(0, thingy.IndexOf(".")));
				string dayStr = thingy.Substring(thingy.IndexOf(".") + 1);
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
				individualEntry.Stunde = thingy;
				break;
			case 3:
				individualEntry.Vertreter = thingy;
				break;
			case 4:
				individualEntry.Fach = thingy;
				break;
			case 5:
				individualEntry.AltFach = thingy;
				break;
			case 6:
				individualEntry.Raum = thingy;
				break;
			case 7:
				individualEntry.Klasse = thingy;
				break;
			case 8:
				individualEntry.Lehrer = thingy;
				break;
			case 13:
				individualEntry.Notiz = thingy;
				break;
			case 14:
				individualEntry.EntfallStr = thingy;
				break;
			case 15:
				individualEntry.MitbeStr = thingy;
				break;
			default:
				break;
			}
			return individualEntry;
		}

		private void times_DownloadStringCompleted(string res)
		{
            if (res == "") { return; }

            string comp = res.Replace(" ", string.Empty);
			List<Data> resultCollection = new List<Data> ();
			//TO-DO: Parse VPlan
			//lastD = false;
			if (!silent)
				clearView();
			int needleCount = getNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
			string[] raw = getDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = processRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, preliminaryResult.DontImmediatelyRefresh);
				resultCollection.AddRange (preliminaryResult.ParsedRows);
			}
			if (preliminaryResult.DontImmediatelyRefresh != true)
			{
				refreshAll(resultCollection);
			}
		}
		private void news_DownloadStringCompleted(string res)
		{
            if (res == "") { return; }
			string comp = res.Replace (" ", string.Empty);
			//TO-DO: Parse VPlan
			//lastD = false;
			int needleCount = getNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
			string[] raw = getDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = processRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, preliminaryResult.DontImmediatelyRefresh, Activity.getNews);
				if(preliminaryResult.ParsedNews!=null &&(preliminaryResult.ParsedNews.Content!=null||preliminaryResult.ParsedNews.Summary!=null))
					addTheNews (preliminaryResult.ParsedNews);
			}
		}
        private void timesNext_DownloadStringCompleted(String res)
        {
            
            try
            {
                if (res == "")
                {
                    refreshAll(globData);
                    return;
                }
				string comp = res.Replace(" ", string.Empty);
                if (comp.IndexOf("NotFound") != -1 || comp.Length == 0)
                {
                    refreshAll(globData);
                    return;
                }
				List<Data> resultCollection = new List<Data> ();
                //TO-DO: Parse VPlan
				int needleCount = getNewsBoxesLength(comp);
				int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
				string[] raw = getDayArray (comp, daysAndNewsBoxes);
				InterstitialFetching preliminaryResult = new InterstitialFetching();
				foreach (var item in raw)
				{
					preliminaryResult = processRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, preliminaryResult.DontImmediatelyRefresh, Activity.ParseSecondSchedule);
					resultCollection.AddRange (preliminaryResult.ParsedRows);
				}

				globData.AddRange(resultCollection);
                refreshAll(globData);
            }
            catch
            {
                refreshAll(globData);
            }
        }
    }
}

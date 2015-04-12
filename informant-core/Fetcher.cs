using System;
using System.Collections.Generic;
using System.Globalization;
using UntisExp.Containers;
using UntisExp.EventHandlers;
using UntisExp.Interfaces;

namespace UntisExp
{
    /// <summary>
    /// This class provides methods for getting the info from WebUntis
    /// </summary>
    public class Fetcher
    {
        private readonly INetworkAccessor _networkAccessor;
        /// <summary>
        /// Fires if the view should be cleared of old entries
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event EventHandler<EventArgs> RaiseReadyToClearView;
        /// <summary>
        /// Fires if the desired schedule was retreived
        /// </summary>
        public event EventHandler<ScheduleEventArgs> RaiseRetreivedScheduleItems;
        /// <summary>
        /// Fires if news items were found
        /// </summary>
        public event EventHandler<NewsEventArgs> RaiseRetreivedNewsItem;
        /// <summary>
        /// Fires if the list of groups was retreived
        /// </summary>
        public event EventHandler<GroupEventArgs> RaiseRetreivedGroupItems;
        /// <summary>
        /// Fires if something went wrong and the UI should display an error message
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event EventHandler<ErrorMessageEventArgs> RaiseErrorMessage;

        private Action<List<Data>> _temporaryRefresh;
        private bool _rootToTemporary;

		/// <summary>
		/// Collections of retreived <see cref="Data"/>
		/// </summary>
        private List<Data> _globData;

		/// <summary>
		/// Group to query WebUntis for
		/// </summary>
		private int _group;

		/// <summary>
		/// Whether the method alerts if it fails or not
		/// </summary>
        private readonly bool _silent;

		/// <summary>
		/// MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles
		/// </summary>
        private readonly int _mode = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class which is pre-equipped for background operation. Will surpress all rows which do not contain a timetable element, e.g. the date row. Useful for background services.
        /// </summary>
        /// <param name="mode">The mode of operation: MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles</param>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        // ReSharper disable once UnusedMember.Global
        public Fetcher(int mode, INetworkAccessor networkAccessor = null)
        {
            _mode = mode;
            _silent = true;
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        public Fetcher(INetworkAccessor networkAccessor = null)
        {
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;
        }
			
        /// <summary>
        /// Downloads a list of groups asynchronously. Uses the callbacks from the constructor. Subscribe to <see cref="RaiseErrorMessage"/> and <see cref="RaiseRetreivedGroupItems"/>
        /// </summary>
        public void GetClasses()
        {
            _networkAccessor.DownloadData(VConfig.Url + VConfig.PathToNavbar, groups_DownloadStringCompleted, OnRaiseErrorMessageEvent, null, VConfig.GroupIErrorTtl, VConfig.GroupIErrorTxt, VConfig.GroupIErrorBtn);
        }

		/// <summary>
		/// Will parse the site for groups and fire <see cref="RaiseRetreivedGroupItems"/> once finished
		/// </summary>
		/// <param name="res">The HTML string</param>
        private void groups_DownloadStringCompleted(String res)
        {
            try
            {
                List<Group> groupObj = new List<Group>();
                string raw = res.Replace(" ", string.Empty);

                raw = raw.Substring(raw.IndexOf("varclasses=[", StringComparison.Ordinal) + "varclasses=[".Length);
                raw = raw.Substring(0, raw.IndexOf("];", StringComparison.Ordinal));
                raw = raw.Replace("\"", string.Empty);
                raw = raw.Replace("\n", string.Empty);
                string[] arr = raw.Split(',');
                int i = 1;
                foreach (var item in arr)
                {
                    groupObj.Add(new Group(i, item));
                    i++;
                }
                OnRaiseRetreivedGroupsItemsEvent(new GroupEventArgs(groupObj));
            }
            catch
            {
                // ignored
            }
        }

        private void Revert()
        {
            _rootToTemporary = false;
            _temporaryRefresh = null;
        }

        /// <summary>
        /// Downloads a list of events for the given group. Subscribe to <see cref="RaiseReadyToClearView"/>, <see cref="RaiseErrorMessage"/> and either <see cref="RaiseRetreivedScheduleItems"/> or <see cref="RaiseRetreivedNewsItem"/>, depending on which <see cref="Activity"/> you are performing
        /// </summary>
        /// <param name="group">The group number</param>
        /// <param name="activity">Optional. What <see cref="Activity"/> to perform. Will default to a check of the first and all upfollowing schedules</param>
        /// <param name="week">Optional, if it's not set, the current week will be fetched. Sets the calender week to get.</param>
        public void GetTimes(int group, Activity activity = Activity.ParseFirstSchedule, int week = -1)
        {

            _group = group;
            string weekStr = "";
			int length = group.ToString ().Length;
			string groupStr = "w";
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

			var nav = VConfig.Url + weekStr + "/w/" + groupStr + ".htm";
			if (activity == Activity.ParseSecondSchedule)
			{
                _networkAccessor.DownloadData(nav, timesNext_DownloadStringCompleted, OnRaiseErrorMessageEvent, Abort);
			} else if (activity == Activity.ParseFirstSchedule) {
                _networkAccessor.DownloadData(nav, times_DownloadStringCompleted, OnRaiseErrorMessageEvent, null, VConfig.EventIErrorTtl, VConfig.EventIErrorTxt, VConfig.EventIErrorBtn);
			} else if (activity == Activity.GetNews) {
                _networkAccessor.DownloadData(nav, news_DownloadStringCompleted, OnRaiseErrorMessageEvent, null, VConfig.EventIErrorTtl, VConfig.EventIErrorTxt, VConfig.EventIErrorBtn);
			}
		}

        /// <summary>
        /// This method fetches the schedules of more than one group. Subscribe to <see cref="RaiseReadyToClearView"/>, <see cref="RaiseErrorMessage"/> and either <see cref="RaiseRetreivedScheduleItems"/> or <see cref="RaiseRetreivedNewsItem"/>, depending on which <see cref="Activity"/> you are performing
        /// </summary>
        /// <param name="groups">An array of valid WebUntis group numbers.</param>
        /// <param name="week">For debugging purposes only. Which week to fetch for. Will default to -1 which means that the current week will be selected.</param>
        public void GetMultipleGroupTimes(int[] groups, int week = -1)
        {
            var tracker = new MulipleAwait(groups.Length);
            tracker.RaiseFinishedWaitingAndJoining += (sender, e) =>
            {
                OnRaiseRetreivedScheduleItemsEvent(e);
                Revert();
            };
            _temporaryRefresh = tracker.CallMe;
            _rootToTemporary = true;
            foreach (var group in groups)
            {
                GetTimes(group, Activity.ParseFirstSchedule, week);
            }
        }

		/// <summary>
        /// Will be called if subsequent downloads of <see cref="GetTimes"/> fail. Raises <see cref="RaiseRetreivedScheduleItems"/> event to write previous elements to the front-end
		/// </summary>
		private void Abort ()
		{
		    if (_rootToTemporary)
                _temporaryRefresh(_globData);
		    else
		        OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(_globData));
		}

		/// <summary>
		/// Counts how many news boxes are present in the WebUntis schedule
		/// </summary>
		/// <returns>The number of the newsboxes.</returns>
		/// <param name="input">The WebUntis HTML string</param>
		private int GetNewsBoxesLength(string input) {
			string haystack = input;
			string needle = VConfig.TitleOfMsgBox.Replace(" ", string.Empty);
			return (haystack.Length - input.Replace(needle, "").Length) / needle.Length;
		}

		/// <summary>
		/// Splits the WebUntis site into a array of day tables
		/// </summary>
		/// <returns>An array of the <c>string</c>s representing a table of a day</returns>
		/// <param name="input">The WebUntis HTML string</param>
		/// <param name="tables">How many tables to search for</param>
		private string [] GetDayArray(string input, int tables)
		{
			string[] result = new string[tables];
			for (int i = 0; i < tables; i++)
			{
				result[i] = input.Substring(input.IndexOf("<table", StringComparison.Ordinal), input.IndexOf("</table>", StringComparison.Ordinal) - input.IndexOf("<table", StringComparison.Ordinal) + 8);
				input = input.Substring(input.IndexOf("</table>", StringComparison.Ordinal) + 8);
			}
			return result;
		}

		private void times_DownloadStringCompleted(string res)
		{
            if (res == "") { return; }

            string comp = res.Replace(" ", string.Empty);
			List<Data> resultCollection = new List<Data> ();
			//TO-DO: Parse VPlan
			//lastD = false;
			if (!_silent)
				OnRaiseReadyToClearViewEvent();
			int needleCount = GetNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.ExpectedDayNum + needleCount;
			string[] raw = GetDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = InterstitialFetching.ProcessRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule);
			    if (preliminaryResult.HasToGetSecondSchedule)
			    {
			        _globData = preliminaryResult.ParsedRows;
                    GetTimes(_group, Activity.ParseSecondSchedule);
			    }
				resultCollection.AddRange (preliminaryResult.ParsedRows);
			}
			if (preliminaryResult.HasToGetSecondSchedule != true)
			{
                if (_rootToTemporary)
                    _temporaryRefresh(resultCollection);
                else
                    OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(resultCollection));
			}
		}
		private void news_DownloadStringCompleted(string res)
		{
            if (res == "") { return; }
			string comp = res.Replace (" ", string.Empty);
			//TO-DO: Parse VPlan
			//lastD = false;
			int needleCount = GetNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.ExpectedDayNum + needleCount;
			string[] raw = GetDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = InterstitialFetching.ProcessRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule, Activity.GetNews);
				if(preliminaryResult.ParsedNews!=null &&(preliminaryResult.ParsedNews.Content!=null||preliminaryResult.ParsedNews.Summary!=null))
					OnRaiseRetreivedNewsItemEvent(new NewsEventArgs(preliminaryResult.ParsedNews));
			}
		}
        private void timesNext_DownloadStringCompleted(string res)
        {
            
            try
            {
                if (res == "")
                {
                    if (_rootToTemporary)
                        _temporaryRefresh(_globData);
                    else
                        OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(_globData));
                    return;
                }
				string comp = res.Replace(" ", string.Empty);
                if (comp.IndexOf("NotFound", StringComparison.Ordinal) != -1 || comp.Length == 0)
                {
                    if (_rootToTemporary)
                        _temporaryRefresh(_globData);
                    else
                        OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(_globData));
                    return;
                }
				List<Data> resultCollection = new List<Data> ();
				int needleCount = GetNewsBoxesLength(comp);
				int daysAndNewsBoxes = VConfig.ExpectedDayNum + needleCount;
				string[] raw = GetDayArray (comp, daysAndNewsBoxes);
				InterstitialFetching preliminaryResult = new InterstitialFetching();
				foreach (var item in raw)
				{
                    preliminaryResult = InterstitialFetching.ProcessRow(item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule, Activity.ParseSecondSchedule);
					resultCollection.AddRange (preliminaryResult.ParsedRows);
				}

				_globData.AddRange(resultCollection);
                if (_rootToTemporary)
                    _temporaryRefresh(_globData);
                else
                    OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(_globData));
            }
            catch
            {
                if (_rootToTemporary)
                    _temporaryRefresh(_globData);
                else
                    OnRaiseRetreivedScheduleItemsEvent(new ScheduleEventArgs(_globData));
            }
        }

        /// <summary>
        /// Will raise a Clear event
        /// </summary>
        /// <param name="e">The event arguments that should be passed</param>
        protected virtual void OnRaiseReadyToClearViewEvent(EventArgs e = null)
        {
            if (e == null) e = new EventArgs();
            EventHandler<EventArgs> handler = RaiseReadyToClearView;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Will raise a Schedule event
        /// </summary>
        /// <param name="e">The event arguments that should be passed</param>
        protected virtual void OnRaiseRetreivedScheduleItemsEvent(ScheduleEventArgs e)
        {
            EventHandler<ScheduleEventArgs> handler = RaiseRetreivedScheduleItems;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Will raise a Groups event
        /// </summary>
        /// <param name="e">The event arguments that should be passed</param>
        protected virtual void OnRaiseRetreivedGroupsItemsEvent(GroupEventArgs e)
        {
            EventHandler<GroupEventArgs> handler = RaiseRetreivedGroupItems;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Will raise a News event
        /// </summary>
        /// <param name="e">The event arguments that should be passed</param>
        protected virtual void OnRaiseRetreivedNewsItemEvent(NewsEventArgs e)
        {
            EventHandler<NewsEventArgs> handler = RaiseRetreivedNewsItem;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Will raise an alarm event
        /// </summary>
        /// <param name="e">The event arguments that should be passed</param>
        protected virtual void OnRaiseErrorMessageEvent(ErrorMessageEventArgs e)
        {
            EventHandler<ErrorMessageEventArgs> handler = RaiseErrorMessage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// This class exists to manage the progress of fetching operations for multiple 
        /// </summary>
        class MulipleAwait
        {
            private List<Data> _receivedBits;
            private readonly List<List<Data>> _queue = new List<List<Data>>(); 
            public event EventHandler<ScheduleEventArgs> RaiseFinishedWaitingAndJoining;
            private readonly int _num;

            /// <summary>
            /// This constructor creates a new instance of this class with an expected num of entries 
            /// </summary>
            /// <param name="num">How many schedules to expect</param>
            public MulipleAwait(int num)
            {
                _num = num;
            }

            /// <summary>
            /// Will raise a Schedule event
            /// </summary>
            /// <param name="e">The event arguments that should be passed</param>
            protected virtual void OnRaiseFinishedWaitingAndJoining(ScheduleEventArgs e)
            {
                EventHandler<ScheduleEventArgs> handler = RaiseFinishedWaitingAndJoining;
                if (handler != null)
                {
                    handler(this, e);
                }
            }

            /// <summary>
            /// Callback for finished assembly of a schedule
            /// </summary>
            /// <param name="bit">The schedule</param>
            public void CallMe(List<Data> bit)
            {
                _queue.Add(bit);
                Finish();
            }

            private void Finish()
            {
                if (_queue.Count < _num) return;
                for (int i = _queue.Count - 1; i >= 0; i--)
                {
                    _receivedBits = _receivedBits == null ? _queue[i] : Helpers.JoinTwoDataLists(_receivedBits, _queue[i]);
                    _queue.RemoveAt(i);
                }

                OnRaiseFinishedWaitingAndJoining(new ScheduleEventArgs(_receivedBits));
            }

        }
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
            /// The parsed <see cref="Data"/> entries out of the table 
            /// </summary>
            public List<Data> ParsedRows;
            /// <summary>
            /// The parsed <see cref="News"/> entry out of the table
            /// </summary>
            public News ParsedNews;
            /// <summary>
            /// Whether the caller has to get another table from the network
            /// </summary>
            public bool HasToGetSecondSchedule;

            /// <summary>
            /// Given a string representing a table of a day in WebUntis' HTML, <see cref="ProcessRow"/> will return an object including the <see cref="Data"/> representations of each schedule value
            /// </summary>
            /// <returns>Object containing the current progress of parsing and the last <see cref="Data"/>-objects that were parsed</returns>
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
                InterstitialFetching result = new InterstitialFetching { HasToGetSecondSchedule = passDontImmediatelyRefresh };
                int daysRec = 0;
                if (item.IndexOf(VConfig.SearchNoAccess, StringComparison.Ordinal) == -1)
                {
                    string it = item.Replace("&nbsp;", String.Empty);
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
                            var mc = VConfig.CellSearch.Matches(w);
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
                    date = activity == Activity.ParseFirstSchedule ? date.AddDays(-1) : date.AddDays(1);
                }
                return date.AddDays(day);
            }

            /// <summary>
            /// Will construct a new empty object with default values for <see cref="UntisExp.Fetcher.InterstitialFetching.OuterLoopCursor"/> and <see cref="UntisExp.Fetcher.InterstitialFetching.HasToGetSecondSchedule"/>
            /// </summary>
            public InterstitialFetching()
            {
                HasToGetSecondSchedule = false;
                OuterLoopCursor = 0;
            }
        }

    }
}

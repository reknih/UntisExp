using System;
using System.Collections.Generic;
using System.Globalization;

namespace UntisExp
{
    /// <summary>
    /// This class provides methods for getting the info from WebUntis
    /// </summary>
    public class Fetcher
    {
		// Actions to be called back on lifetime events
        private INetworkAccessor _networkAccessor;
		private Action _clearView;
        private Action<String, String, String> _alert;
        private Action<List<Data>> _refreshAll;
		private Action<News> _addTheNews;
        private Action<List<Group>> _refreshSet;

		/// <summary>
		/// Collections of retreived <see cref="UntisExp.Data"/>
		/// </summary>
        private List<Data> _globData;

		/// <summary>
		/// Group to query WebUntis for
		/// </summary>
		protected int Group;

		/// <summary>
		/// Whether the method alerts if it fails or not
		/// </summary>
        private bool _silent;

		/// <summary>
		/// MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles
		/// </summary>
        private int _mode = 5;

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class which is pre-equipped for background operation. Will surpress all rows which do not contain a timetable element, e.g. the date row. Useful for background services.
        /// </summary>
        /// <param name="stop">Callback for the case that the fetching failed. Useful to stop the background service</param>
        /// <param name="refreshAll">Callback function that will be called once the <see cref="GetTimes"/> function and all of its callbacks are finished</param>
        /// <param name="mode">The mode of operation: MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles</param>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        public Fetcher(Action stop, Action<List<Data>> refreshAll, int mode, INetworkAccessor networkAccessor = null)
        {
            _alert = delegate { stop(); };
            _refreshAll = refreshAll;
            _mode = mode;
            _silent = true;
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="del">Callback function name for clearing the current list</param>
        /// <param name="alert">Callbck function name for displaying an error with title, text and dismiss-button</param>
        /// <param name="refeshAll">Callback function for updating the view with a List of event entries.</param>
        /// <param name="refreshOne">Callback function for adding one event entry to the view.</param>
        /// <param name="refreshSet">Callback function for updating the view with a List of group entries.</param>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        public Fetcher(Action del, Action<String, String, String> alert, Action<List<Data>> refeshAll, Action<Data> refreshOne = null, Action<List<Group>> refreshSet=null, INetworkAccessor networkAccessor = null)
        {
            _clearView = del;
            _alert = alert;
            _refreshAll = refeshAll;
            _refreshSet = refreshSet;
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="alert">Callbck function name for displaying an error with title, text and dismiss-button</param>
        /// <param name="refeshAll">Callback function for updating the view with a List of entries.</param>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        public Fetcher(Action<String, String, String> alert, Action<List<Group>> refeshAll, INetworkAccessor networkAccessor = null)
        {
            _alert = alert;
            _refreshSet = refeshAll;
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class which will instantly checks for news in the schedule
		/// </summary>
		/// <param name="refreshOne">Callback for returning one element</param>
		/// <param name="group">Which group to query for. It is kind of the same for all numbers, most of the time. Default is <c>5</c></param>
		/// <param name="week">Week to query news for. For debugging purposes. Will default to <c>-1</c> which means that the current schedule will be read.</param>
        /// <param name="networkAccessor">For testing purposes only. Will inject a <see cref="INetworkAccessor"/> into the class</param>
        public Fetcher(Action<News> refreshOne, int group = 5, int week = -1, INetworkAccessor networkAccessor = null)
		{
			_addTheNews = refreshOne;
			_alert = delegate {
			};
			GetTimes (group, Activity.getNews, week);
            if (networkAccessor == null) networkAccessor = new Networking();
            _networkAccessor = networkAccessor;

		}

			
        /// <summary>
        /// Downloads a list of groups asynchronously. Uses the callbacks from the constructor.
        /// </summary>
        public void GetClasses()
        {
			_networkAccessor.DownloadData (VConfig.url + VConfig.pathToNavbar, groups_DownloadStringCompleted, _alert, null, VConfig.groupIErrorTtl, VConfig.groupIErrorTxt, VConfig.groupIErrorBtn);
        }

		/// <summary>
		/// Will parse the site for groups and call <see cref="_refreshSet"/> once finished
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
                int i = 0;
                foreach (var item in arr)
                {
                    groupObj.Add(new Group(i, item));
                    i++;
                }
                _refreshSet(groupObj);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Downloads a list of events for the given group
        /// </summary>
        /// <param name="group">The group number</param>
        /// <param name="activity">Optional. What activity to perform. Will default to a check of the first and all upfollowing schedules</param>
        /// <param name="week">Optional, if it's not set, the current week will be fetched. Sets the calender week to get.</param>
        public void GetTimes(int group, Activity activity = Activity.ParseFirstSchedule, int week = -1)
        {

            Group = group;
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

			var nav = VConfig.url + weekStr + "/w/" + groupStr + ".htm";
			if (activity == Activity.ParseSecondSchedule) {
                _networkAccessor.DownloadData(nav, timesNext_DownloadStringCompleted, _alert, Abort);
			} else if (activity == Activity.ParseFirstSchedule) {
                _networkAccessor.DownloadData(nav, times_DownloadStringCompleted, _alert, null, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			} else if (activity == Activity.getNews) {
                _networkAccessor.DownloadData(nav, news_DownloadStringCompleted, _alert, null, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			}
		}

		/// <summary>
		/// Will be called if subsequent downloads of <see cref="GetTimes"/> fail. Uses <see cref="_refreshAll"/> callback to write previous elements to the front-end
		/// </summary>
		private void Abort () {
			_refreshAll (_globData);
		}

		/// <summary>
		/// Counts how many news boxes are present in the WebUntis schedule
		/// </summary>
		/// <returns>The number of the newsboxes.</returns>
		/// <param name="input">The WebUntis HTML string</param>
		private int GetNewsBoxesLength(string input) {
			string haystack = input;
			string needle = VConfig.titleOfMsgBox.Replace(" ", string.Empty);
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
				_clearView();
			int needleCount = GetNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
			string[] raw = GetDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = InterstitialFetching.ProcessRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule);
			    if (preliminaryResult.HasToGetSecondSchedule)
			    {
			        _globData = preliminaryResult.ParsedRows;
                    GetTimes(Group, Activity.ParseSecondSchedule);
			    }
				resultCollection.AddRange (preliminaryResult.ParsedRows);
			}
			if (preliminaryResult.HasToGetSecondSchedule != true)
			{
				_refreshAll(resultCollection);
			}
		}
		private void news_DownloadStringCompleted(string res)
		{
            if (res == "") { return; }
			string comp = res.Replace (" ", string.Empty);
			//TO-DO: Parse VPlan
			//lastD = false;
			int needleCount = GetNewsBoxesLength(comp);
			int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
			string[] raw = GetDayArray (comp, daysAndNewsBoxes);
			InterstitialFetching preliminaryResult = new InterstitialFetching();
			foreach (var item in raw)
			{
				preliminaryResult = InterstitialFetching.ProcessRow (item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule, Activity.getNews);
				if(preliminaryResult.ParsedNews!=null &&(preliminaryResult.ParsedNews.Content!=null||preliminaryResult.ParsedNews.Summary!=null))
					_addTheNews (preliminaryResult.ParsedNews);
			}
		}
        private void timesNext_DownloadStringCompleted(String res)
        {
            
            try
            {
                if (res == "")
                {
                    _refreshAll(_globData);
                    return;
                }
				string comp = res.Replace(" ", string.Empty);
                if (comp.IndexOf("NotFound", StringComparison.Ordinal) != -1 || comp.Length == 0)
                {
                    _refreshAll(_globData);
                    return;
                }
				List<Data> resultCollection = new List<Data> ();
                //TO-DO: Parse VPlan
				int needleCount = GetNewsBoxesLength(comp);
				int daysAndNewsBoxes = VConfig.expectedDayNum + needleCount;
				string[] raw = GetDayArray (comp, daysAndNewsBoxes);
				InterstitialFetching preliminaryResult = new InterstitialFetching();
				foreach (var item in raw)
				{
                    preliminaryResult = InterstitialFetching.ProcessRow(item, preliminaryResult.OuterLoopCursor, daysAndNewsBoxes, _mode, _silent, preliminaryResult.HasToGetSecondSchedule, Activity.ParseSecondSchedule);
					resultCollection.AddRange (preliminaryResult.ParsedRows);
				}

				_globData.AddRange(resultCollection);
                _refreshAll(_globData);
            }
            catch
            {
                _refreshAll(_globData);
            }
        }
    }
}

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
    /// This class provides methods for getting the info rom the site
    /// </summary>
    public class Fetcher
    {
        private Action del;
        private Action<String, String, String> alert;
        private Action<List<Data>> refreshAll;
		private Action<Data> refreshOne;
		private Action<News> addTheNews;
        private Action<List<Group>> refreshSet;
        private List<Data> globData;
		protected int iOuter;
		protected int stringcnt;
		protected bool lastD;
		protected int daysRec;
		protected int Group;
		protected int iInner;
		protected Data data;
		protected News news;
		protected List<Data> v1;
		protected bool doNot;
        private bool silent;
		protected int i;
        // MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide, MODE 5: Alles
        private int mode = 5;
        /// <summary>
        /// Initializes a new instance of the <see cref="UntisExp.Fetcher"/> class.
        /// </summary>
        /// <param name="_del">Callback function name for clearing the current list</param>
        /// <param name="_alert">Callbck function name for displaying an error with title, text and dismiss-button</param>
        /// <param name="_refeshAll">Callback function for updating the view with a List of entries.</param>
        /// <param name="_refreshOne">Callback function for adding one entry to the view.</param>
        public Fetcher(Action _del, Action<String, String, String> _alert, Action<List<Data>> _refeshAll, Action<Data> _refreshOne)
        {
            del = _del;
            alert = _alert;
            refreshAll = _refeshAll;
            refreshOne = _refreshOne;
        }
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
        public Fetcher(Action _del, Action<String, String, String> _alert, Action<List<Data>> _refeshAll, Action<Data> _refreshOne, Action<List<Group>> _refreshSet)
        {
            del = _del;
            alert = _alert;
            refreshAll = _refeshAll;
            refreshOne = _refreshOne;
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

		public Fetcher(Action<News> _refreshOne, int group = 5, int week = -1)
		{
			addTheNews = _refreshOne;
			alert = delegate(string arg1, string arg2, string arg3) {
				string test;
			};
			getTimes (group, Activity.getNews, week);
		}

			
        /// <summary>
        /// Downloads a list of groups asynchronously. Uses the callbacks from the constructor.
        /// </summary>
        public void getClasses()
        {
			Networking.DownloadData (VConfig.url + VConfig.pathToNavbar, groups_DownloadStringCompleted, alert, true, VConfig.groupIErrorTtl, VConfig.groupIErrorTxt, VConfig.groupIErrorBtn);
        }
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
            {
                if (silent != true)
                {
                }
                //alert("ERROR", + e.Message + " " + e.StackTrace, "");
            }
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
            if (group < 10)
            {
                groupStr = "w0000" + Convert.ToString(group);
            }
            else if (group < 100)
            {
                groupStr = "w000" + Convert.ToString(group);
            }
            else if (group < 1000)
            {
                groupStr = "w00" + Convert.ToString(group);
            }
            else if (group < 10000)
            {
                groupStr = "w0" + Convert.ToString(group);
            }
            else if (group < 100000)
            {
                groupStr = "w" + Convert.ToString(group);
            }
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            if (week == -1)
                week = cal.GetWeekOfYear(DateTime.Now, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
			if (activity == Activity.ParseSecondSchedule)
                week++;
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
				Networking.DownloadData (nav, timesNext_DownloadStringCompleted, alert);
			} else if (activity == Activity.ParseFirstSchedule) {
				Networking.DownloadData (nav, times_DownloadStringCompleted, alert, true, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			} else if (activity == Activity.getNews) {
				Networking.DownloadData (nav, news_DownloadStringCompleted, alert, true, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			}
		}

		protected string prepareText (string input) {
			return input.Replace(" ", string.Empty);
		}

		protected int getNewsBoxesLength(string input) {
			string haystack = input;
			string needle = VConfig.titleOfMsgBox.Replace(" ", string.Empty);
			return (haystack.Length - input.Replace(needle, "").Length) / needle.Length;
		}

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

		protected void processRow(string item, Activity activity = Activity.ParseFirstSchedule)
		{
			string it = item;
			if (item.IndexOf(VConfig.searchNoAccess) == -1)
			{
				if (iOuter == (stringcnt - 1))
					lastD = true;
				daysRec++;
				it = item.Replace("&nbsp;", String.Empty);
				MatchCollection mc;
				string searchInFront;
				if (activity == Activity.getNews) {
					searchInFront = "<tr>";
					if (news == null) {
						news = new News ();
						news.Image = "http://centrallink.de/sr/Blackboard.png";
						news.Source = new Uri (VConfig.url);
					}
				} else {
					searchInFront = "<trclass='list";
				}
				if (item.IndexOf(VConfig.noEventsText.Replace(" ", string.Empty)) == -1)
				{
					i = 0;
					while (it.IndexOf(searchInFront) != -1)
					{
						if (activity == Activity.getNews && news.Summary != null) {
							news.Summary += ", ";
						}
						if (i == 0)
							it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
						string w;
						data = new Data();
						w = it.Substring(it.IndexOf(searchInFront));
						w = w.Substring(0, w.IndexOf("</tr>"));
						it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
						mc = VConfig.cellSearch.Matches(w);
						iInner = 0;
						foreach (var thing in mc)
						{
							string compute = prepareScheduleItem (thing);
							if (activity != Activity.getNews) {
								proceedScheduleItem (compute);
							} else {
								proceedNewsItem (compute);
							}
						}
						if (activity != Activity.getNews) {
							data.refresh();
							v1.Add(data);
						}
						i++;
					}
				}
				else if (activity == Activity.ParseFirstSchedule)
				{
					if (!silent) refreshOne(new Data());
				}
			}
			iOuter++;
			if (iOuter == stringcnt && (daysRec == 1 && lastD) && mode != 0 && activity == Activity.ParseFirstSchedule)
			{
				getTimes(Group, Activity.ParseSecondSchedule);
				globData = v1;
				doNot = true;
			}
		}

		protected string prepareScheduleItem(object input)
		{
			string thingy = input.ToString();
			thingy = thingy.Substring(thingy.IndexOf(">") + 1, thingy.Length - thingy.IndexOf(">") - 1);
			thingy = thingy.Substring(0, thingy.IndexOf("<"));
			return thingy;
		}

		protected void proceedNewsItem (string thingy) {
			news.Title = "Vom Vertretungsplan:";
			news.Summary = news.Summary + Helpers.AddSpaces(thingy);
			news.Content = news.Summary;
		}

		protected void proceedScheduleItem (string thingy) {
			switch (iInner)
			{
			case 0:
				if (thingy == VConfig.specialEvtAb)
				{ data.Veranstaltung = true; }
				break;
			case 1:
				int day = Convert.ToInt16(thingy.Substring(0, thingy.IndexOf(".")));
				string dayStr = thingy.Substring(thingy.IndexOf(".") + 1);
				dayStr = dayStr.Replace(".", string.Empty);
				int month = Convert.ToInt16(dayStr);
				int year = DateTime.Now.Year;
				DateTime dt = new DateTime(year, month, day);
				data.Date = dt;
				if (i == 0 && !silent)
				{
					v1.Add(new Data(dt));
				}
				break;
			case 2:
				data.Stunde = thingy;
				break;
			case 3:
				data.Vertreter = thingy;
				break;
			case 4:
				data.Fach = thingy;
				break;
			case 5:
				data.AltFach = thingy;
				break;
			case 6:
				data.Raum = thingy;
				break;
			case 7:
				data.Klasse = thingy;
				break;
			case 8:
				data.Lehrer = thingy;
				break;
			case 13:
				data.Notiz = thingy;
				break;
			case 14:
				data.EntfallStr = thingy;
				break;
			case 15:
				data.MitbeStr = thingy;
				break;
			default:
				break;
			}
			iInner++;
		}

		protected void initPaser(bool two = false)
		{
			v1 = new List<Data>();
			iOuter = 0;
			daysRec = 0;
			doNot = false;
		}

		private void times_DownloadStringCompleted(string res)
		{
			string comp = prepareText(res);
			//TO-DO: Parse VPlan
			lastD = false;
			if (!silent)
				del();
			int needleCount = getNewsBoxesLength(comp);
			int stringcnt = VConfig.expectedDayNum + needleCount;
			string[] raw = getDayArray (comp, stringcnt);
			initPaser ();
			foreach (var item in raw)
			{
				processRow (item);
			}
			if (doNot != true)
			{
				refreshAll(v1);
			}
		}
		private void news_DownloadStringCompleted(string res)
		{
			string comp = prepareText(res);
			//TO-DO: Parse VPlan
			lastD = false;
			int needleCount = getNewsBoxesLength(comp);
			int stringcnt = VConfig.expectedDayNum + needleCount;
			string[] raw = getDayArray (comp, stringcnt);
			initPaser ();
			foreach (var item in raw)
			{
				processRow (item, Activity.getNews);
			}
			addTheNews (news);
		}
        private void timesNext_DownloadStringCompleted(String res)
        {
            try
            {
				string comp = prepareText(res);
                if (comp.IndexOf("NotFound") != -1 || comp.Length == 0)
                {
                    refreshAll(globData);
                    return;
                }
                //TO-DO: Parse VPlan
				int needleCount = getNewsBoxesLength(comp);
				int stringcnt = VConfig.expectedDayNum + needleCount;
				string[] raw = getDayArray (comp, stringcnt);
				initPaser();
                foreach (var item in raw)
                {
					processRow (item, Activity.ParseSecondSchedule);
                }
				globData.AddRange(v1);
                refreshAll(globData);
            }
            catch
            {
                refreshAll(globData);
            }
        }
    }
}

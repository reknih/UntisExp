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
		//protected bool lastD;
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
//                if (silent != true)
//                {
//                }
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
			int length = group.ToString ().Length;
			groupStr = "w";
			for (int i = 0; i < (5 - length); i++) {
				groupStr = groupStr + "0";
			}
			groupStr = groupStr + Convert.ToString (group);
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
                //if (iOuter == (stringcnt - 1))
                //    lastD = true;
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
				if ((item.IndexOf(VConfig.noEventsText.Replace(" ", string.Empty)) == -1)||activity==Activity.getNews)
				{
					i = 0;
					it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
					while (it.IndexOf(searchInFront) != -1)
					{
                        if (i == 0)
                        {
                            //news box should not be a day so count days here
                            daysRec++;
                        }
						if (activity == Activity.getNews && news.Summary != null) {
							news.Summary += ", ";
						}
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
                else 
                {
                    daysRec++;
                    if (!silent)
                    {
                        //Adds Date
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
                        date = date.AddDays(iOuter);
                        v1.Add(new Data(date));
                        //Adds no events message
                        v1.Add(new Data());

                    }
                }
			}
			iOuter++;
			if (iOuter == stringcnt && (daysRec == 1) && mode != 0 && activity == Activity.ParseFirstSchedule)
			{
				getTimes(Group, Activity.ParseSecondSchedule);
				globData = v1;
				doNot = true;
			}
		}

		protected string prepareScheduleItem(object input)
		{
			string thingy = input.ToString();
			return thingy.Substring(thingy.IndexOf(">") + 1,thingy.LastIndexOf("<")-thingy.IndexOf(">")-1);
		}

		protected void proceedNewsItem (string thingy) {
			news.Title = "Vom Vertretungsplan:";
			news.Summary += Helpers.AddSpaces(thingy);
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
            if (res == "") { return; }
//#if DEBUG
//            //Nur zum testen, ob zweite Woche geladen wird, da zum Halbjahr klassen geändert wurden und heute am Fr. noch Do. da ist.

//            //ToDo: Anschließend löschen!!!!!!
//            if (DateTime.Now < new DateTime(2015, 2, 3))
//            {
//                res = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">\r\n<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\"><meta http-equiv=\"expires\" content=\"0\"><meta name=\"keywords\" content=\"Stundenplan, timetable\">\r\n<meta name=\"GENERATOR\" content=\"Untis 2013\">\r\n<title>Untis 2013  STUNDENPLAN 14/15  CHRIST.-WIRTH-SCHULE USINGEN  1</title>\r\n<style type=\"text/css\">\r\na {color:#000000;}\r\n</style>\r\n<link rel=\"stylesheet\" href=\"../../untisinfo.css\" type=\"text/css\">\r\n</head>\r\n<body bgcolor=\"#FFFFFF\">\r\n<CENTER><font size=\"3\" face=\"Arial\">\r\n<BR><h2>GS/WM</h2><p><div id=\"vertretung\">\r\n<a name=\"1\"> </a><br><b>26.1. Montag</b> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"2\"> </a><br><a href=\"#1\">[ Montag ]</a> | <b>27.1. Dienstag</b> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"3\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <b>28.1. Mittwoch</b> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"4\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <b>29.1. Donnerstag</b> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"5\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <b>30.1. Freitag</b><p>\r\n<p>\r\n<table class=\"subst\" >\r\n<tr class='list'><th class=\"list\" align=\"center\">Art</th><th class=\"list\" align=\"center\">Datum</th><th class=\"list\" align=\"center\">Stunde</th><th class=\"list\" align=\"center\">Vertreter</th><th class=\"list\" align=\"center\">Fach</th><th class=\"list\" align=\"center\">(Fach)</th><th class=\"list\" align=\"center\">Raum</th><th class=\"list\" align=\"center\">Klasse(n)</th><th class=\"list\" align=\"center\">(Lehrer)</th><th class=\"list\" align=\"center\">(Klasse(n))</th><th class=\"list\" align=\"center\">(Raum)</th><th class=\"list\" align=\"center\">Vertr. von</th><th class=\"list\" align=\"center\">(Le.) nach</th><th class=\"list\" align=\"center\">Vertretungs-Text</th><th class=\"list\" align=\"center\">Entfall</th><th class=\"list\" align=\"center\">Mitbetreuung</th><th class=\"list\" align=\"center\">Kopplung.</th></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C25</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">HU</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C24</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">SP</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C26</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">HAR</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C27</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">PA</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C29</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ET</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">BA</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C17</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ET</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">NI</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">E13</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Sondereins.</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">GS</td><td class=\"list\" align=\"center\">KLALE</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">C23</td><td class=\"list\" align=\"center\">E1B</td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\" align=\"center\">1</td></tr>\r\n</table>\r\n<p>\r\n</div></font><font size=\"3\" face=\"Arial\">\r\nPeriode8   ++ 2014/15-I ++   PLAN-14-15-I\r\n</font></CENTER>\r\n</body>\r\n</html>\r\n\r\n"; 
//                }
//#endif
            string comp = res.Replace(" ", string.Empty);
			//TO-DO: Parse VPlan
			//lastD = false;
			if (!silent)
				del();
			int needleCount = getNewsBoxesLength(comp);
			stringcnt = VConfig.expectedDayNum + needleCount;
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
            if (res == "") { return; }
//#if DEBUG
//            //Nur zum testen, ob zweite Woche geladen wird, da zum Halbjahr klassen geändert wurden und heute am Fr. noch Do. da ist.

//            //ToDo: Anschließend löschen!!!!!!
//            if (DateTime.Now < new DateTime(2015, 2, 2))
//            {
//                res = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">\r\n<html>\r\n<head>\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\"><meta http-equiv=\"expires\" content=\"0\"><meta name=\"keywords\" content=\"Stundenplan, timetable\">\r\n<meta name=\"GENERATOR\" content=\"Untis 2013\">\r\n<title>Untis 2013  STUNDENPLAN 14/15  CHRIST.-WIRTH-SCHULE USINGEN  1</title>\r\n<style type=\"text/css\">\r\na {color:#000000;}\r\n</style>\r\n<link rel=\"stylesheet\" href=\"../../untisinfo.css\" type=\"text/css\">\r\n</head>\r\n<body bgcolor=\"#FFFFFF\">\r\n<CENTER><font size=\"3\" face=\"Arial\">\r\n<BR><h2>GS/WM</h2><p><div id=\"vertretung\">\r\n<a name=\"1\"> </a><br><b>26.1. Montag</b> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"2\"> </a><br><a href=\"#1\">[ Montag ]</a> | <b>27.1. Dienstag</b> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"3\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <b>28.1. Mittwoch</b> | <a href=\"#4\">[ Donnerstag ]</a> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"4\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <b>29.1. Donnerstag</b> | <a href=\"#5\">[ Freitag ]</a><p>\r\n<table class=\"subst\" >\r\n<tr><td align=\"center\" colspan=\"17\" >Vertretungen sind nicht freigegeben</td></tr>\r\n</table>\r\n<p>\r\n<a name=\"5\"> </a><br><a href=\"#1\">[ Montag ]</a> | <a href=\"#2\">[ Dienstag ]</a> | <a href=\"#3\">[ Mittwoch ]</a> | <a href=\"#4\">[ Donnerstag ]</a> | <b>30.1. Freitag</b><p>\r\n<p>\r\n<table class=\"subst\" >\r\n<tr class='list'><th class=\"list\" align=\"center\">Art</th><th class=\"list\" align=\"center\">Datum</th><th class=\"list\" align=\"center\">Stunde</th><th class=\"list\" align=\"center\">Vertreter</th><th class=\"list\" align=\"center\">Fach</th><th class=\"list\" align=\"center\">(Fach)</th><th class=\"list\" align=\"center\">Raum</th><th class=\"list\" align=\"center\">Klasse(n)</th><th class=\"list\" align=\"center\">(Lehrer)</th><th class=\"list\" align=\"center\">(Klasse(n))</th><th class=\"list\" align=\"center\">(Raum)</th><th class=\"list\" align=\"center\">Vertr. von</th><th class=\"list\" align=\"center\">(Le.) nach</th><th class=\"list\" align=\"center\">Vertretungs-Text</th><th class=\"list\" align=\"center\">Entfall</th><th class=\"list\" align=\"center\">Mitbetreuung</th><th class=\"list\" align=\"center\">Kopplung.</th></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C25</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">HU</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C24</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ER</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">SP</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C26</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">HAR</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C27</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">KR</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">PA</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C29</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ET</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">BA</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">C17</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list odd'><td class=\"list\" align=\"center\">Entfall</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">ET</td><td class=\"list\" align=\"center\">---</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">NI</td><td class=\"list\" align=\"center\">E1A, E1B, E1C, E1D, E1E, E1F</td><td class=\"list\" align=\"center\">E13</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">Entfall für Lehrer</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">x</td><td class=\"list\" align=\"center\"> </td><td class=\"list\" align=\"center\"> </td></tr>\r\n<tr class='list even'><td class=\"list\" align=\"center\">Sondereins.</td><td class=\"list\" align=\"center\">30.1.</td><td class=\"list\" align=\"center\">3</td><td class=\"list\" align=\"center\">GS</td><td class=\"list\" align=\"center\">KLALE</td><td class=\"list\"> </td><td class=\"list\" align=\"center\">C23</td><td class=\"list\" align=\"center\">E1B</td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\"> </td><td class=\"list\" align=\"center\">1</td></tr>\r\n</table>\r\n<p>\r\n</div></font><font size=\"3\" face=\"Arial\">\r\nPeriode8   ++ 2014/15-I ++   PLAN-14-15-I\r\n</font></CENTER>\r\n</body>\r\n</html>\r\n\r\n";
//            }
//#endif
			string comp = res.Replace (" ", string.Empty);
			//TO-DO: Parse VPlan
			//lastD = false;
			int needleCount = getNewsBoxesLength(comp);
			stringcnt = VConfig.expectedDayNum + needleCount;
			string[] raw = getDayArray (comp, stringcnt);
			initPaser ();
			foreach (var item in raw)
			{
				processRow (item, Activity.getNews);
			}
            if(news.Content!=null||news.Summary!=null)
			addTheNews (news);
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
                //TO-DO: Parse VPlan
				int needleCount = getNewsBoxesLength(comp);
				stringcnt = VConfig.expectedDayNum + needleCount;
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

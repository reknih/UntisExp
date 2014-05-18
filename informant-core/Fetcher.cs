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
        private Action<List<Group>> refreshSet;
        private List<Data> globData;

        private int Group;
        private bool silent;
        // MODE 0: Nur heute, MODE 1: Nur Morgen, MODE 2: Beide
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
        public void getTimes(int group, bool follow = false, int week = -1)
        {
            if (mode == 1)
                follow = true;
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
            if (follow == true)
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
			if (follow) {
				Networking.DownloadData (nav, timesNext_DownloadStringCompleted, alert);
			} else {
				Networking.DownloadData (nav, times_DownloadStringCompleted, alert, true, VConfig.eventIErrorTtl, VConfig.eventIErrorTxt, VConfig.eventIErrorBtn);
			}
		}
        private void times_DownloadStringCompleted(String res)
        {
            string comp = res;
            comp = comp.Replace(" ", string.Empty);
            //TO-DO: Parse VPlan
            int stringcnt = 5;
            if (!silent)
                del();
            string haystack = comp;
            string needle = VConfig.titleOfMsgBox.Replace(" ", string.Empty);
            int needleCount = (haystack.Length - comp.Replace(needle, "").Length) / needle.Length;
            stringcnt = stringcnt + needleCount;
            string[] raw = new string[stringcnt];
            for (int i = 0; i < stringcnt; i++)
            {
                raw[i] = comp.Substring(comp.IndexOf("<table"), comp.IndexOf("</table>") - comp.IndexOf("<table") + 8);
                comp = comp.Substring(comp.IndexOf("</table>") + 8);
            }
            List<Data> v1 = new List<Data>();
            int iOuter = 0;
            int daysRec = 0;
            bool doNot = false;
            foreach (var item in raw)
            {
                string it = item;
                if (item.IndexOf(VConfig.searchNoAccess) == -1)
                {
                    daysRec++;
                    it = item.Replace("&nbsp;", String.Empty);
                    MatchCollection mc;
                    if (item.IndexOf(VConfig.noEventsText.Replace(" ", string.Empty)) == -1)
                    {
                        int i = 0;
                        while (it.IndexOf("<trclass='list") != -1)
                        {
                            if (i == 0)
                                it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
                            string w;
                            Data data = new Data();

                            w = it.Substring(it.IndexOf("<trclass='list"));
                            w = w.Substring(0, w.IndexOf("</tr>"));
                            it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
                            mc = VConfig.cellSearch.Matches(w);
                            int iInner = 0;
                            foreach (var thing in mc)
                            {
                                string thingy = thing.ToString();
                                thingy = thingy.Substring(thingy.IndexOf(">") + 1, thingy.Length - thingy.IndexOf(">") - 1);
                                thingy = thingy.Substring(0, thingy.IndexOf("<"));
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
                            data.refresh();
                            v1.Add(data);
                            i++;
                        }
                    }
                    else
                    {
                        if (!silent) refreshOne(new Data());
                    }
                }
                iOuter++;
                if (iOuter == stringcnt && daysRec == 1 && mode != 0)
                {
                    getTimes(Group, true);
                    globData = v1;
                    doNot = true;
                }
            }
            if (doNot != true)
            {
                refreshAll(v1);
            }
        }
        private void timesNext_DownloadStringCompleted(String res)
        {
            try
            {
                string comp = res;
                comp = comp.Replace(" ", string.Empty);
                if (res.IndexOf("NotFound") != -1 || res.Length != 0)
                {
                    refreshAll(globData);
                    return;
                }
                //TO-DO: Parse VPlan
                int stringcnt = 5;
                string haystack = comp;
                string needle = VConfig.titleOfMsgBox.Replace(" ", string.Empty);
                int needleCount = (haystack.Length - comp.Replace(needle, "").Length) / needle.Length;
                stringcnt = stringcnt + needleCount;
                string[] raw = new string[stringcnt];
                for (int i = 0; i < stringcnt; i++)
                {
                    raw[i] = comp.Substring(comp.IndexOf("<table"), comp.IndexOf("</table>") - comp.IndexOf("<table") + 8);
                    comp = comp.Substring(comp.IndexOf("</table>") + 8);
                }
                int iOuter = 0;
                int daysRec = 0;
                foreach (var item in raw)
                {
                    string it = item;
                    if (item.IndexOf(VConfig.searchNoAccess) == -1)
                    {
                        daysRec++;
                        it = item.Replace("&nbsp;", String.Empty);
                        MatchCollection mc;
                        if (item.IndexOf(VConfig.noEventsText.Replace(" ", string.Empty)) == -1)
                        {
                            int i = 0;
                            while (it.IndexOf("<trclass='list") != -1)
                            {
                                if (i == 0)
                                    it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
                                string w;
                                Data data = new Data();
                                w = it.Substring(it.IndexOf("<trclass='list"));
                                w = w.Substring(0, w.IndexOf("</tr>"));
                                it = it.Substring(it.IndexOf("</tr>") + 5, it.Length - it.IndexOf("</tr>") - 5);
                                mc = VConfig.cellSearch.Matches(w);
                                int iInner = 0;
                                foreach (var thing in mc)
                                {
                                    string thingy = thing.ToString();
                                    thingy = thingy.Substring(thingy.IndexOf(">") + 1, thingy.Length - thingy.IndexOf(">") - 1);
                                    thingy = thingy.Substring(0, thingy.IndexOf("<"));
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
                                                globData.Add(new Data(dt));
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
                                data.refresh();
                                globData.Add(data);
                                i++;
                            }
                        }
                    }
                    iOuter++;
                }
                refreshAll(globData);
            }
            catch
            {
                refreshAll(globData);
            }
        }
    }
}

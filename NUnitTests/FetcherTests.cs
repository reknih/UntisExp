using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UntisExp;
using System.Web.Script.Serialization;
using UntisExp.Containers;
using UntisExp.EventHandlers;

namespace NUnitTests
{
    [TestFixture]
    public class FetcherTests
    {
        [Test]
        [Category("Callback driven async methods")]
        [Category("Network dependant integration test")]
        public void CallbackOfFetcherMethodWillBeCalled()
        {
            bool calledBack = false;
            List<Data> res = new List<Data>();
            var callback = new EventHandler<ScheduleEventArgs>((sender, locRes) =>
            {
                calledBack = true;
                res = locRes.Schedule;
            });
            var sut = new Fetcher();
            sut.RaiseRetreivedScheduleItems += callback;
            sut.GetTimes(12, Activity.ParseFirstSchedule, 13);
            for (int i = 0; (i < 20) && !calledBack; i++)
            {
                Thread.Sleep(1000);
            }
            Assert.IsTrue(res.Count > 1);
        }

        [Test]
        [Category("Callback driven async methods")]
        [Category("Network dependant integration test")]
        public void CallbackOfMultipleFetchingMethodWillBeCalled()
        {
            bool calledBack = false;
            List<Data> res = new List<Data>();
            var callback = new EventHandler<ScheduleEventArgs>((sender, locRes) =>
            {
                calledBack = true;
                res = locRes.Schedule;
            });
            var sut = new Fetcher();
            sut.RaiseRetreivedScheduleItems += callback;
            sut.GetMultipleGroupTimes(new[] { 12, 38, 6, 7 }, 13);
            for (int i = 0; (i < 30) && !calledBack; i++)
            {
                Thread.Sleep(1000);
            }
            Assert.IsTrue(res.Count > 1);
        }


        [Test]
        [Category("Mocked dependencies")]
        public void WillConstructRightUrlString()
        {
            var callback = new EventHandler<ScheduleEventArgs>((sender, locRes) =>
            {
            });
            var spy = new MockedNetworkAccessor();
            var sut = new Fetcher(spy);
            sut.RaiseRetreivedScheduleItems += callback;
            sut.GetTimes(12, Activity.ParseFirstSchedule, 13);
            Assert.AreEqual("http://vp.cws-usingen.de/Schueler/13/w/w00012.htm", spy.CalledUri);
        }

        [Test]
        [Category("Mocked dependencies")]
        public void WillConstructRightUrlStringForFollowups()
        {
            var spy = new MockedNetworkAccessor();
            var sut = new Fetcher(spy);
            sut.GetTimes(12, Activity.ParseSecondSchedule, 14);
            Assert.AreEqual("http://vp.cws-usingen.de/Schueler/15/w/w00012.htm", spy.CalledUri);
        }

        [Test]
        [Category("Selftest")]
        public void MockedNetworkInterfaceWillCallBack()
        {
            var sut = new MockedNetworkAccessor();
            bool isCalled = false;
            Action<string> callback = a =>
            {
                isCalled = true;
            };
            sut.DataToReturn = "Tests are awesome";
            sut.DownloadData("http://abc.de", callback);
            Assert.IsTrue(isCalled);
        }

        [Test]
        [Category("Mocked dependencies")]
        [Category("Integration test")]
        public void WillOutputTheCollectionOfData()
        {
            var serializer = new JavaScriptSerializer();
            List<Data> res = new List<Data>();
            var callback = new EventHandler<ScheduleEventArgs>((sender, locRes) =>
            {
                res = locRes.Schedule;
            });
            var spy = new MockedNetworkAccessor
            {
                DataToReturn =
                    "<!DOCTYPEHTMLPUBLIC\"-//IETF//DTDHTML//EN\">\n<html>\n<head>\n<metahttp-equiv=\"Content-Type\"content=\"text/html;charset=iso-8859-1\"><metahttp-equiv=\"expires\"content=\"0\"><metaname=\"keywords\"content=\"Stundenplan,timetable\">\n<metaname=\"GENERATOR\"content=\"Untis2013\">\n<title>Untis2013STUNDENPLAN14/15-IICHRIST.-WIRTH-SCHULEUSINGEN1</title>\n<styletype=\"text/css\">\na{color:#000000;}\n</style>\n<linkrel=\"stylesheet\"href=\"../../untisinfo.css\"type=\"text/css\">\n</head>\n<bodybgcolor=\"#FFFFFF\">\n<CENTER><fontsize=\"3\"face=\"Arial\">\n<BR><h2>LOH/LK</h2><p><divid=\"vertretung\">\n<aname=\"1\">&nbsp;</a><br><b>23.3.Montag</b>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"2\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<b>24.3.Dienstag</b>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"3\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<b>25.3.Mittwoch</b>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"4\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<b>26.3.Donnerstag</b>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"5\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<b>27.3.Freitag</b><p>\n<tableborder=\"3\"rules=\"all\"bgcolor=\"#F4F4F4\"cellpadding=\"3\"cellspacing=\"3\">\n<tr><thalign=\"center\"colspan=\"2\">NachrichtenzumTag</th></tr>\n<tr><td>Unterrichtsfrei&nbsp;</td><td>4-11Std.</td></tr>\n<tr><tdcolspan=\"2\">SCHÖNEFERIEN!!!</td></tr><br></table>\n<p>\n<tableclass=\"subst\">\n<trclass='list'><thclass=\"list\"align=\"center\">Art</th><thclass=\"list\"align=\"center\">Datum</th><thclass=\"list\"align=\"center\">Stunde</th><thclass=\"list\"align=\"center\">Vertreter</th><thclass=\"list\"align=\"center\">Fach</th><thclass=\"list\"align=\"center\">(Fach)</th><thclass=\"list\"align=\"center\">Raum</th><thclass=\"list\"align=\"center\">Klasse(n)</th><thclass=\"list\"align=\"center\">(Lehrer)</th><thclass=\"list\"align=\"center\">(Klasse(n))</th><thclass=\"list\"align=\"center\">(Raum)</th><thclass=\"list\"align=\"center\">Vertr.von</th><thclass=\"list\"align=\"center\">(Le.)nach</th><thclass=\"list\"align=\"center\">Vertretungs-Text</th><thclass=\"list\"align=\"center\">Entfall</th><thclass=\"list\"align=\"center\">Mitbetreuung</th><thclass=\"list\"align=\"center\">Kopplung.</th></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Vertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">1</td><tdclass=\"list\"align=\"center\">scli</td><tdclass=\"list\"align=\"center\">EK</td><tdclass=\"list\"align=\"center\">EK</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">ST</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">SP</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E27</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">KR</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">PA</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E34</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">KR</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">HAR</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E32</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">WA</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E25</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ET</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">NI</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E35</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ET</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">SPN</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E12</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">HU</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E26</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Raumvertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">BD</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">E25</td><tdclass=\"list\"align=\"center\">6E</td><tdclass=\"list\"align=\"center\">BD</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Statt-Vertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">BN</td><tdclass=\"list\"align=\"center\">FR</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n</table>\n<p>\n</div></font><fontsize=\"3\"face=\"Arial\">\nPeriode5++2014/15-II++PLAN-14-15-II\n</font></CENTER>\n</body>\n</html>"
            };
            var sut = new Fetcher(spy);
            sut.RaiseRetreivedScheduleItems += callback;
            sut.GetTimes(12, Activity.ParseFirstSchedule, 14);
            var exp = new List<Data>
            {
                new Data(new DateTime(2015, 3, 27)),
                new Data
                {
                    Lesson = "1",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "scli",
                    Subject = "EK",
                    OldSubject = "EK",
                    Room = "E22",
                    Group = "6F",
                    Teacher = "ST",
                    OutageStr = "",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "ER",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "SP",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "KR",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "PA",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "KR",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "HAR",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "ER",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "WA",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "ET",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "NI",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "ET",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "SPN",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "---",
                    Subject = "---",
                    OldSubject = "ER",
                    Room = "---",
                    Group = "6A,6B,6D,6E,6F,6C,6G",
                    Teacher = "HU",
                    OutageStr = "x",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "BD",
                    Subject = "ER",
                    OldSubject = "ER",
                    Room = "E25",
                    Group = "6E",
                    Teacher = "BD",
                    OutageStr = "",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh(),
                new Data
                {
                    Lesson = "3",
                    Date = new DateTime(2015, 3, 27),
                    Cover = "BN",
                    Subject = "FR",
                    OldSubject = "",
                    Room = "E22",
                    Group = "6F",
                    Teacher = "",
                    OutageStr = "",
                    CareStr = "",
                    Notice = "",
                    Event = false
                }.Refresh()
            };
            var comparableRes = serializer.Serialize(res);
            var comparableExp = serializer.Serialize(exp);

            Assert.AreEqual(comparableExp, comparableRes);
        }

        [Test]
        [Category("Mocked dependencies")]
        [Category("Integration test")]
        [Ignore]
        public void WillOutputTheRightNews()
        {
            var serializer = new JavaScriptSerializer();
            News res = new News();
            var callback = new EventHandler<NewsEventArgs>((sender, locRes) =>
            {
                res = locRes.News;
            });
            var spy = new MockedNetworkAccessor
            {
                DataToReturn =
                    "<!DOCTYPEHTMLPUBLIC\"-//IETF//DTDHTML//EN\">\n<html>\n<head>\n<metahttp-equiv=\"Content-Type\"content=\"text/html;charset=iso-8859-1\"><metahttp-equiv=\"expires\"content=\"0\"><metaname=\"keywords\"content=\"Stundenplan,timetable\">\n<metaname=\"GENERATOR\"content=\"Untis2013\">\n<title>Untis2013STUNDENPLAN14/15-IICHRIST.-WIRTH-SCHULEUSINGEN1</title>\n<styletype=\"text/css\">\na{color:#000000;}\n</style>\n<linkrel=\"stylesheet\"href=\"../../untisinfo.css\"type=\"text/css\">\n</head>\n<bodybgcolor=\"#FFFFFF\">\n<CENTER><fontsize=\"3\"face=\"Arial\">\n<BR><h2>LOH/LK</h2><p><divid=\"vertretung\">\n<aname=\"1\">&nbsp;</a><br><b>23.3.Montag</b>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"2\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<b>24.3.Dienstag</b>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"3\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<b>25.3.Mittwoch</b>|<ahref=\"#4\">[Donnerstag]</a>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"4\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<b>26.3.Donnerstag</b>|<ahref=\"#5\">[Freitag]</a><p>\n<tableclass=\"subst\">\n<tr><tdalign=\"center\"colspan=\"17\">Vertretungensindnichtfreigegeben</td></tr>\n</table>\n<p>\n<aname=\"5\">&nbsp;</a><br><ahref=\"#1\">[Montag]</a>|<ahref=\"#2\">[Dienstag]</a>|<ahref=\"#3\">[Mittwoch]</a>|<ahref=\"#4\">[Donnerstag]</a>|<b>27.3.Freitag</b><p>\n<tableborder=\"3\"rules=\"all\"bgcolor=\"#F4F4F4\"cellpadding=\"3\"cellspacing=\"3\">\n<tr><thalign=\"center\"colspan=\"2\">NachrichtenzumTag</th></tr>\n<tr><td>Unterrichtsfrei&nbsp;</td><td>4-11Std.</td></tr>\n<tr><tdcolspan=\"2\">SCHÖNEFERIEN!!!</td></tr><br></table>\n<p>\n<tableclass=\"subst\">\n<trclass='list'><thclass=\"list\"align=\"center\">Art</th><thclass=\"list\"align=\"center\">Datum</th><thclass=\"list\"align=\"center\">Stunde</th><thclass=\"list\"align=\"center\">Vertreter</th><thclass=\"list\"align=\"center\">Fach</th><thclass=\"list\"align=\"center\">(Fach)</th><thclass=\"list\"align=\"center\">Raum</th><thclass=\"list\"align=\"center\">Klasse(n)</th><thclass=\"list\"align=\"center\">(Lehrer)</th><thclass=\"list\"align=\"center\">(Klasse(n))</th><thclass=\"list\"align=\"center\">(Raum)</th><thclass=\"list\"align=\"center\">Vertr.von</th><thclass=\"list\"align=\"center\">(Le.)nach</th><thclass=\"list\"align=\"center\">Vertretungs-Text</th><thclass=\"list\"align=\"center\">Entfall</th><thclass=\"list\"align=\"center\">Mitbetreuung</th><thclass=\"list\"align=\"center\">Kopplung.</th></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Vertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">1</td><tdclass=\"list\"align=\"center\">scli</td><tdclass=\"list\"align=\"center\">EK</td><tdclass=\"list\"align=\"center\">EK</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">ST</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">SP</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E27</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">KR</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">PA</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E34</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">KR</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">HAR</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E32</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">WA</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E25</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ET</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">NI</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E35</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ET</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">SPN</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E12</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Entfall</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">---</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">HU</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E26</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">x</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listodd'><tdclass=\"list\"align=\"center\">Raumvertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">BD</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">ER</td><tdclass=\"list\"align=\"center\">E25</td><tdclass=\"list\"align=\"center\">6E</td><tdclass=\"list\"align=\"center\">BD</td><tdclass=\"list\"align=\"center\">6A,6B,6D,6E,6F,6C,6G</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n<trclass='listeven'><tdclass=\"list\"align=\"center\">Statt-Vertretung</td><tdclass=\"list\"align=\"center\">27.3.</td><tdclass=\"list\"align=\"center\">3</td><tdclass=\"list\"align=\"center\">BN</td><tdclass=\"list\"align=\"center\">FR</td><tdclass=\"list\">&nbsp;</td><tdclass=\"list\"align=\"center\">E22</td><tdclass=\"list\"align=\"center\">6F</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td><tdclass=\"list\"align=\"center\">&nbsp;</td></tr>\n</table>\n<p>\n</div></font><fontsize=\"3\"face=\"Arial\">\nPeriode5++2014/15-II++PLAN-14-15-II\n</font></CENTER>\n</body>\n</html>"
            };
            var sut = new Fetcher(spy);
            sut.RaiseRetreivedNewsItem += callback;
            sut.GetTimes(5, Activity.GetNews, 14);
            var exp = new News { Title = "Vom Vertretungsplan:", Source = new Uri(VConfig.Url), Image = "http://centrallink.de/sr/Blackboard.png", Summary = "Freitag, 3.4:\nUnterrichtsfrei 4- 1 1 Std.\n\nFreitag, 3.4:\nSCHÖNEFERIEN!!!", Content = "Freitag, 3.4:\nUnterrichtsfrei 4- 1 1 Std.\n\nFreitag, 3.4:\nSCHÖNEFERIEN!!!" };
            var comparableRes = serializer.Serialize(res);
            var comparableExp = serializer.Serialize(exp);

            Assert.AreEqual(comparableExp, comparableRes);
        }

        [Test]
        [Category("Mocked dependencies")]
        [Category("Returns")]
        public void WillReturnWithoutData()
        {
            var spy = new MockedNetworkAccessor
            {
                DataToReturn = ""
            };
            var sut = new Fetcher(spy);
            Assert.DoesNotThrow(() => { sut.GetTimes(12, Activity.ParseFirstSchedule, 14); });
        }

        [Test]
        [Category("Mocked dependencies")]
        [Category("Integration test")]
        public void GetGroupString()
        {
            var serializer = new JavaScriptSerializer();
            var res = new List<Group>();
            var callback = new EventHandler<GroupEventArgs>((sender, locRes) =>
            {
                res = locRes.Groups;
            });
            var spy = new MockedNetworkAccessor
            {
                DataToReturn = "<html>\n<head>\n<meta http-equiv=\"expires\" content=\"0\">\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=iso-8859-1\">\n<meta name=\"GENERATOR\" content=\"gp-Untis 2013\">\n<title>Navigation</title>\n<script language=\"JavaScript\" src=\"../untisscripts.js\"></script>\n<link rel=\"stylesheet\" href=\"../untisinfo.css\" type=\"text/css\">\n\n<script LANGUAGE=\"JavaScript\">\nvar topDir = \"w\";\n\nfunction PopulateElementOption(Form, entries, flag)\n{\n var idx = 0;\n if (flag & 1)\n {\n   Form.element[0] = new Option(\"- Alle -\", 0);\n   idx++;\n }\n if (flag == 0 || flag & 2)\n {\n  for (i = 0; i < entries.length; i++, idx++)\n  {\n    Form.element[idx] = new Option(entries[i], i+1);\n  }\n }\n if (idx > 0)\n {\n	Form.element.selectedIndex = 0;\n	doDisplayTimetable(Form, topDir);\n }\n}\n\n var classes = [\"DRUCK\",\"5A\",\"5B\",\"5C\",\"5D\",\"5E\",\"6A\",\"6B\",\"6C\",\"6D\",\"6E\",\"6F\",\"6G\",\"7A1\",\"7A2\",\"7N1\",\"7N2\",\"7N3\",\"7N4\",\"8A1\",\"8A2\",\"8N1\",\"8N2\",\"8N3\",\"9A\",\"9D\",\"9B\",\"9C1\",\"9C2\",\"E2\",\"Q2\",\"Q4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"abi-1\",\"abi-2\",\"abi-3\",\"abi-4\",\"abi-5\",\"abi-6\",\"abi-7\",\"abi-b\",\"abi-g\",\"8A\",\"DAF\"];\n var flcl = 2; var flte = 1;\nfunction ChangeStudentOptions(Form)\n{\n var type = Form.type[Form.type.selectedIndex].value;\n if (type != \"s\")\n    return;\n var nr = Form.classes[Form.classes.selectedIndex].value;\n Form.element.length = 0;\n var idx = 0;\n for (i = 0; i < students.length; i++)\n {\n   if (nr == 0 || studtable[i] == nr)\n   {\n      Form.element[idx] = new Option(students[i], i+1);\n      idx++;\n   }\n }\n	doDisplayTimetable(Form, topDir);\n}\n\nfunction ChangeElementOptions(Form)\n{\n setselclass(\"empty\");\n Form.element.length = 0;\n var type = Form.type[Form.type.selectedIndex].value;\n switch(type)\n {\n	case \"c\": PopulateElementOption(Form, classes, 0); break;\n	case \"w\": PopulateElementOption(Form, classes, flcl); break;\n	case \"t\": PopulateElementOption(Form, teachers, 0); break;\n	case \"v\": PopulateElementOption(Form, teachers, flte); break;\n	case \"r\": PopulateElementOption(Form, rooms, 0); break;\n	case \"f\": PopulateElementOption(Form, subjects, 0); break;\n	case \"g\": PopulateElementOption(Form, corridors, 0); break;\n	case \"s\": \n	{\n        setselclass(\"restore\");\n        PopulateElementOption(Form, students, 0); \n        break;\n    }\n }\n\n return;\n}\n\nfunction SelectElement(Form, name)\n{\n	var art = getParameter(parent.location.href, \"art\");\n	if (art != \"\")\n	{\n		for (var i = 0; i < Form.type.length; i++)\n		{\n			if (Form.type[i].value == art)\n			{\n				Form.type.selectedIndex = i;\n				break;\n			}\n		}\n	}\n	ChangeElementOptions(Form);\n	for (var i = 0; i < Form.element.length; i++)\n	{\n		if (Form.element[i].text == name)\n		{\n			Form.element.selectedIndex = i;\n			break;\n		}\n	}\n}\n\nfunction OnLoad(Form)\n{\n    setselclass(\"save\");\n    \n	var weeknr = WeekOfYear(new Date);\n	for (var i = 0; i < Form.week.options.length; i++)\n	{\n		if (Form.week.options[i].value == weeknr)\n			Form.week.options[i].selected = true;\n	}\n\n	var name = \"\";\n	try \n	{\n		name = getParameter(parent.location.href, \"name\");\n	}\n	catch (e) {};\n	if (name == \"\")\n	{\n		ChangeElementOptions(Form);\n		Form.element.selectedIndex = -1;\n		parent.main.location = \"../welcome.htm\";\n	}\n	else\n	{\n		SelectElement(Form, name);\n		doDisplayTimetable(Form, topDir);\n	}\n}\n\n</script>\n</head>\n\n<body class=\"nav\" onload=\"OnLoad(document.forms[0]);\">\n\n <table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" align=\"left\">\n\n  <tr bgcolor=\"#FFFFFF\">\n\n   <td>\n<form name=\"NavBar\" method=\"post\">\n\n    <table width=\"100%\" border=\"0\" cellspacing=\"2\" cellpadding=\"0\">\n\n     <!-- week selection -->\n     <td align=\"left\" class=\"tabelle\">\n      <span class=\"selection\">\n       <nobr>\n        Kalenderwoche<br>\n        <span class=\"absatz\">\n         &nbsp;<br>\n        </span>\n        <select name=\"week\" class=\"selectbox\" onChange=\"doDisplayTimetable(NavBar, topDir);\">\n<option value=\"13\">23.3.2015</option>\n<option value=\"14\">30.3.2015</option>\n<option value=\"15\">6.4.2015</option>\n<option value=\"16\">13.4.2015</option>\n        </select>\n       </nobr>\n      </span>\n     </td>\n\n     <!-- type selection -->\n     <td align=\"left\" class=\"tabelle\">\n      <span class=\"selection\">\n       <nobr>\n        Art<br>\n        <span class=\"absatz\">\n         &nbsp;<br>\n        </span>\n        <select name=\"type\" class=\"selectbox\" onChange=\"ChangeElementOptions(NavBar);\">\n<option value=\"w\">Ver-Kla</option>\n        </select>\n       </nobr>\n      </span>\n     </td>\n\n\n     <!-- element selection -->\n     <td align=\"left\" class=\"tabelle\">\n      <span class=\"selection\">\n       <nobr>\n        Element<br>\n        <span class=\"absatz\">\n         &nbsp;<br>\n        </span>\n        <select name=\"element\" class=\"selectbox\" onChange=\"doDisplayTimetable(NavBar, topDir);\">\n		<option value=\"1\">\n??_*\n		</option>\n        </select>\n       </nobr>\n      </span>\n     </td>\n\n  <td class=\"tabelle\" style=\"vertical-align: middle\">\n      <span class=\"selection\">\n       <nobr>\n		<a href=\"$\" onclick=\"return(doPrintTimetable(NavBar, topDir))\" ><img src=\"print.gif\" border=\"0\" /> Drucken</a>\n       </nobr>\n      </span>\n  </td>\n\n     <!--leeres Feld-->\n     <td width=\"100%\" class=\"tabelle\">\n     </td>\n\n\n     <!-- school info -->\n     <td align=\"right\" class=\"tabelle\">\n      <nobr>\n       <span class=\"schoolname\">\n        CHRIST.-WIRTH-SCHULE USINGEN<img src=\"punkt.gif\">D-61250, SCHLOSSPLATZ 1<br>\n       </span>\n       <span class=\"absatz\">\n        &nbsp;<br>\n       </span>\n       <span class=\"description\">\n        STUNDENPLAN 14/15-II<img src=\"punkt.gif\">13.04.2015<br>\n        Stand: 27.03.2015 07:59\n       </span>\n      </nobr>\n     </td>\n\n    </table>\n</form>\n\n   </td>\n\n  </tr>\n  \n </table>\n\n</body>\n</html>\n"
            };
            var sut = new Fetcher(spy);
            sut.RaiseRetreivedGroupItems += callback;
            sut.GetClasses();

            var exp = new List<Group>
            {
                new Group{ClassName = "DRUCK", Id = 1},
                new Group{ClassName = "5A", Id = 2},
                new Group{ClassName = "5B", Id = 3},
                new Group{ClassName = "5C", Id = 4},
                new Group{ClassName = "5D", Id = 5},
                new Group{ClassName = "5E", Id = 6},
                new Group{ClassName = "6A", Id = 7},
                new Group{ClassName = "6B", Id = 8},
                new Group{ClassName = "6C", Id = 9},
                new Group{ClassName = "6D", Id = 10},
                new Group{ClassName = "6E", Id = 11},
                new Group{ClassName = "6F", Id = 12},
                new Group{ClassName = "6G", Id = 13},
                new Group{ClassName = "7A1", Id = 14},
                new Group{ClassName = "7A2", Id = 15},
                new Group{ClassName = "7N1", Id = 16},
                new Group{ClassName = "7N2", Id = 17},
                new Group{ClassName = "7N3", Id = 18},
                new Group{ClassName = "7N4", Id = 19},
                new Group{ClassName = "8A1", Id = 20},
                new Group{ClassName = "8A2", Id = 21},
                new Group{ClassName = "8N1", Id = 22},
                new Group{ClassName = "8N2", Id = 23},
                new Group{ClassName = "8N3", Id = 24},
                new Group{ClassName = "9A", Id = 25},
                new Group{ClassName = "9D", Id = 26},
                new Group{ClassName = "9B", Id = 27},
                new Group{ClassName = "9C1", Id = 28},
                new Group{ClassName = "9C2", Id = 29},
                new Group{ClassName = "E2", Id = 30},
                new Group{ClassName = "Q2", Id = 31},
                new Group{ClassName = "Q4", Id = 32},
                new Group{ClassName = "5", Id = 33},
                new Group{ClassName = "6", Id = 34},
                new Group{ClassName = "7", Id = 35},
                new Group{ClassName = "8", Id = 36},
                new Group{ClassName = "9", Id = 37},
                new Group{ClassName = "abi-1", Id = 38},
                new Group{ClassName = "abi-2", Id = 39},
                new Group{ClassName = "abi-3", Id = 40},
                new Group{ClassName = "abi-4", Id = 41},
                new Group{ClassName = "abi-5", Id = 42},
                new Group{ClassName = "abi-6", Id = 43},
                new Group{ClassName = "abi-7", Id = 44},
                new Group{ClassName = "abi-b", Id = 45},
                new Group{ClassName = "abi-g", Id = 46},
                new Group{ClassName = "8A", Id = 47},
                new Group{ClassName = "DAF", Id = 48}
            };

            Assert.AreEqual(serializer.Serialize(exp), serializer.Serialize(res));
        }
    }
}
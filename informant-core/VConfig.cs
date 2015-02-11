using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UntisExp
{
	/// <summary>
	/// This static class holds options for the lib. It is prefilled to work with Untis of Christian-Wirth-Schule, Usingen
	/// </summary>
	public static class VConfig
	{
        public static string url            = "http://vp.cws-usingen.de/Schueler/";
        public static string feed           = "http://srdercws.wordpress.com/feed/";
        public static string pathToNavbar   = "frames/navbar.htm";
		public static string groupIErrorTtl = "Dumm, dumm, dumm.";
		public static string groupIErrorTxt = "Wir haben keine Verbndung zum Internet. Die brauchst du aber jetzt.";
		public static string groupIErrorBtn = "Ich komme wieder!";
		public static string eventIErrorTtl = "Aslack-Provider!";
		public static string eventIErrorTxt = "Wir haben keine Verbndung zum Internet gefunden.";
		public static string eventIErrorBtn = "Huuurra.";
		public static string titleOfMsgBox  = "Nachrichten zum Tag";
		public static string noEventsText   = "Keine Vertretungen";
		public static string searchNoAccess = "freigegeben";
		public static Regex  cellSearch      = new Regex("<t{1}d{1}.*?>.*?</td>");
		public static string specialEvtAb   = "Veranst.";
		public static Dictionary<string, string> lessonAbbr = new Dictionary<string, string>();
		public static string noPageErrTtl   = "Du bist nicht schuld";
		public static string noPageErrTxt   = "Die Vertretungsplanseite wurde nicht gefunden.";
		public static string noPageErrBtn   = "Na hoffentlich!";
        public static string unknownErrTtl  = "Houston, wir haben ein Problem.";
		public static string unknownErrTxt  = "Und ich hoffte, dass das nie von jemandem gelesen wird: Unbekannter Fehler!";
        public static string unknownErrBtn  = "Si, Si";
        public static string[] successJokes = new string [] {"Und fertig!", "Heute schon Bertie gesehen?", "Sacrebleu", "Arrriba!", "Dein Tag könnte sich soeben verbessert haben", "Käsekuchen"};
        public static int    expectedDayNum = 5;

		public static void Populate () {
			try {
			lessonAbbr.Add ("DE", "Deutsch");
			lessonAbbr.Add ("EN", "Englisch");
			lessonAbbr.Add ("EN2", "Englisch");
			lessonAbbr.Add ("ET", "Ethik");
			lessonAbbr.Add ("ER", "Ev. Religion");
			lessonAbbr.Add ("KR", "Kath. Religion");
			lessonAbbr.Add ("MA", "Mathe");
			lessonAbbr.Add ("BIO", "Bio");
            lessonAbbr.Add ("BI", "Bio");
			lessonAbbr.Add ("CH", "Chemie");
			lessonAbbr.Add ("PH", "Physik");
			lessonAbbr.Add ("POWI", "PoWi");
			lessonAbbr.Add ("GE", "Geschichte");
			lessonAbbr.Add ("MU", "Musik");
			lessonAbbr.Add ("SPA4", "Spanisch");
			lessonAbbr.Add ("SPA3", "Spanisch");
			lessonAbbr.Add ("FR", "Französich");
			lessonAbbr.Add ("FR2", "Französich");
			lessonAbbr.Add ("SP", "Sport");
			lessonAbbr.Add ("SPA", "Spanisch");
			lessonAbbr.Add ("DS", "Darstellendes Spiel");
			lessonAbbr.Add ("KU", "Kunst");
			lessonAbbr.Add ("IN", "Informatik");
			lessonAbbr.Add ("LA2", "Latein");
			lessonAbbr.Add ("LA", "Latein");
			lessonAbbr.Add ("IT", "Italiensich");
            lessonAbbr.Add("WU", "Wahlunterricht");
            lessonAbbr.Add("KLALE", "Klassenlehrerstunde");
            }
            catch
            {
			}
		}
	}
}
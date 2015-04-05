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
	    /// <summary>
	    /// The WebUntis url to fetch from
	    /// </summary>
	    public const string Url = "http://vp.cws-usingen.de/Schueler/";

	    /// <summary>
	    /// The Url to fetch news from
	    /// </summary>
	    public const string Feed = "http://srdercws.wordpress.com/feed/";

	    /// <summary>
	    /// A path to WebUntis nav bar frame relative to <see cref="VConfig.Url"/>
	    /// </summary>
	    public const string PathToNavbar = "frames/navbar.htm";

	    /// <summary>
	    /// Title for the error message if the groups could not be determined
	    /// </summary>
	    public const string GroupIErrorTtl = "Dumm, dumm, dumm.";

	    /// <summary>
	    /// Body for the error message if the groups could not be determined
	    /// </summary>
	    public const string GroupIErrorTxt = "Wir haben keine Verbndung zum Internet. Die brauchst du aber jetzt.";

	    /// <summary>
	    /// Button caption for the error message if the groups could not be determined
	    /// </summary>
	    public const string GroupIErrorBtn = "Ich komme wieder!";

	    /// <summary>
        /// Title for the error message if the events feed could not be found
	    /// </summary>
	    public const string EventIErrorTtl = "Aslack-Provider!";

	    /// <summary>
        /// Body for the error message if the events feed could not be found
	    /// </summary>
	    public const string EventIErrorTxt = "Wir haben keine Verbndung zum Internet gefunden.";

	    /// <summary>
	    /// Button caption for the error message if the events feed site could not be found
	    /// </summary>
	    public const string EventIErrorBtn = "Huuurra.";

        /// <summary>
        /// Message box title to search for on the WebUntis schedules
        /// </summary>
	    public const string TitleOfMsgBox = "Nachrichten zum Tag";

        /// <summary>
        /// Text if there are no reschedulings on the WebUntis page
        /// </summary>
	    public const string NoEventsText = "Keine Vertretungen";

        /// <summary>
        /// String to search for to check whether the day was blocked
        /// </summary>
	    public const string SearchNoAccess = "freigegeben";

        /// <summary>
        /// A RegExp determining that the schedules table cells will match
        /// </summary>
	    public static readonly Regex  CellSearch = new Regex("<t{1}d{1}.*?>.*?</td>");

        /// <summary>
        /// String determining a special event
        /// </summary>
	    public const string SpecialEvtAb = "Veranst.";

        /// <summary>
        /// List of abbreviations for the lessons.
        /// </summary>
	    public static readonly Dictionary<string, string> LessonAbbr = new Dictionary<string, string>
	    {
	        {"DE", "Deutsch"},
            {"EN", "Englisch"},
            {"EN2", "Englisch"},
            {"ET", "Ethik"},
            {"ER", "Ev. Religion"},
            {"KR", "Kath. Religion"},
            {"MA", "Mathe"},
            {"BIO", "Bio"},
            {"BI", "Bio"},
            {"CH", "Chemie"},
            {"PH", "Physik"},
            {"POWI", "PoWi"},
            {"GE", "Geschichte"},
            {"MU", "Musik"},
            {"SPA4", "Spanisch"},
            {"SPA3", "Spanisch"},
            {"FR", "Französich"},
            {"FR2", "Französich"},
            {"SP", "Sport"},
            {"SPA", "Spanisch"},
            {"DS", "Darstellendes Spiel"},
            {"KU", "Kunst"},
            {"IN", "Informatik"},
            {"LA2", "Latein"},
            {"LA", "Latein"},
            {"IT", "Italiensich"},
            {"WU", "Wahlunterricht"},
            {"KLALE", "Klassenlehrerstunde"}
	    };

        /// <summary>
        /// Title for the error message if the WebUntis site could not be found
        /// </summary>
        public const string NoPageErrTtl = "Du bist nicht schuld";

        /// <summary>
        /// Body for the error message if the WebUntis site could not be found
        /// </summary>
        public const string NoPageErrTxt = "Die Vertretungsplanseite wurde nicht gefunden.";

        /// <summary>
        /// Button caption for the error message if the WebUntis site could not be found
        /// </summary>
        public const string NoPageErrBtn = "Na hoffentlich!";

        /// <summary>
        /// Title for the error message if some serious shit happened
        /// </summary>
        public const string UnknownErrTtl = "Houston, wir haben ein Problem.";

        /// <summary>
        /// Body for the error message if some serious shit happened
        /// </summary>
	    public const string UnknownErrTxt = "Und ich hoffte, dass das nie von jemandem gelesen wird: Unbekannter Fehler!";

        /// <summary>
        /// Button caption for the error message if some serious shit happened
        /// </summary>
        public const string UnknownErrBtn = "Si, Si";

        /// <summary>
        /// Jokes that may be displayed shortly after loading completed (E.g. in Windows Phone's status bar)
        /// </summary>
        // ReSharper disable once UnusedMember.Global
	    public static string[] SuccessJokes = {"Und fertig!", "Heute schon Bertie gesehen?", "Sacrebleu", "Arrriba!", "Dein Tag könnte sich soeben verbessert haben", "Käsekuchen"};

        /// <summary>
        /// How many days are in a week
        /// </summary>
        public const int ExpectedDayNum = 5;
	}
}
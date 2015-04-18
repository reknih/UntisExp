using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using NodaTime;

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
        #if LEHRER
	    public const string Url = "http://vp.cws-usingen.de/Lehrer/";
        #else
        public const string Url = "http://vp.cws-usingen.de/Schueler/";
        #endif

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
        /// A RegExp determining that the schedule table cells describing the timeslot should match for time serialization
        /// </summary>
        public static readonly Regex TimeSearch = new Regex("^\\d(\\s*-\\s*\\d)?");
        
        /// <summary>
        /// String determining a special event
        /// </summary>
	    public const string SpecialEvtAb = "Veranst."; 
        
        /// <summary>
        /// The text to welcome new users to the App
        /// </summary>
#if LEHRER
        public const string WelcomeText = "Hallo und danke für den Download der App! Wenn Sie hier einen Fehler finden schreiben Sie ihn uns doch bitte, denn es ist alles noch ganz neu hier.";

#else
        public const string WelcomeText = "Hallo und danke für den Download der App! Wir schicken dich jetzt zur Klassenauswahl, die App merkt sich danach diese Klasse. Wenn du einen Fehler findest schreib ihn uns doch bitte. Denn es ist alles noch ganz neu hier. Wir wünschen viel Ausfall!";
#endif

        
#if LEHRER
        ///<summary>
        ///If the app is deployed for teacher text to enter password
        /// </summary>
        public const String EnterPw = "Bitte geben Sie das Passwort ein, dass Sie erhalten haben, um sich in dieser App als Lehrer zu identifizieren.";
        ///<summary>
        ///If the app is deployed for teacher the password
        /// </summary>
        public const String Password = "IchLehrer";
#endif

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
        /// Describes when a specific lesson time slot starts
        /// </summary>
        public static readonly Dictionary<int, LocalTime> LessonStart = new Dictionary<int, LocalTime>
        {
            {1, new LocalTime(7,55)},
            {2, new LocalTime(8,45)},
            {3, new LocalTime(9,30)},
            {4, new LocalTime(10,35)},
            {5, new LocalTime(11,35)},
            {6, new LocalTime(12,20)},
            {7, new LocalTime(13,10)},
            {8, new LocalTime(14,00)},
            {9, new LocalTime(14,45)},
            {10, new LocalTime(15,40)},
            {11, new LocalTime(16,25)},
            {12, new LocalTime(17,15)}
        };

        /// <summary>
        /// Describes when a specific lesson time slot ends
        /// </summary>
        public static readonly Dictionary<int, LocalTime> LessonEnd = new Dictionary<int, LocalTime>
        {
            {1, new LocalTime(8,40)},
            {2, new LocalTime(9,30)},
            {3, new LocalTime(10,30)},
            {4, new LocalTime(11,20)},
            {5, new LocalTime(12,20)},
            {6, new LocalTime(13,00)},
            {7, new LocalTime(13,50)},
            {8, new LocalTime(14,45)},
            {9, new LocalTime(15,30)},
            {10, new LocalTime(16,25)},
            {11, new LocalTime(17,10)},
            {12, new LocalTime(18,00)}
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
	    public static readonly string[] SuccessJokes = {"Und fertig!", "Heute schon Bertie gesehen?", "Sacrebleu", "Arrriba!", "Dein Tag könnte sich soeben verbessert haben", "Käsekuchen"};

        /// <summary>
        /// How many days are in a week
        /// </summary>
        public const int ExpectedDayNum = 5;
	}
}
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace UntisExp
{
	/// <summary>
	/// Object wich can hold either headings or info for an schedule update
	/// </summary>
	public class Data
	{
		/// <summary>
		/// Creates an empty Data-object. Fill it via the properties, then call refresh on that instance. If no changes were made, the object has an empty state. (No events for that day.)
		/// </summary>
		public Data()
		{
			Line1 = "Keine Vertretungen.";
			Line2 = "";
			Veranstaltung = false;
			Head = true;
			VConfig.Populate ();
		}
		/// <summary>
		/// Creates a heading for the given day.
		/// </summary>
		/// <param name="date">Date object</param>
		public Data(DateTime date)
		{
			Line1 = date.ToString("dddd", new System.Globalization.CultureInfo("de-DE")) + ", " + Convert.ToString(date.Day) + "." + Convert.ToString(date.Month);
			Line2 = "";
			Head = true;
			VConfig.Populate ();
		}
		/// <summary>
		/// Creates a Data object with some fields prefilled
		/// </summary>
		/// <param name="std">Time, preformatted</param>
		/// <param name="fach">Subject</param>
		/// <param name="lehr">Old teacher</param>
		/// <param name="vertr">New teacher</param>
		/// <param name="raum">Room.</param>
		/// <param name="notiz">Note</param>
		public Data(string std, string fach, string lehr, string vertr, string raum, string notiz)
		{
			Fach = fach;
			Lehrer = lehr;
			Vertreter = vertr;
			Stunde = std;
			Raum = raum;
			Notiz = notiz;
			VConfig.Populate ();
			refresh();
		}

		/// <summary>
		/// Initializes a new instance with an generic heading
		/// </summary>
		/// <param name="head">Text</param>
		public Data(string head)
		{
			Head = true;
			Line1 = head;
			Line2 = "";
			VConfig.Populate ();
		}

		/// <summary>
		/// To be called when poperties were changed. Spells the subjects out and determines the type of event
		/// </summary>
		public void refresh()
		{
			Head = false;
			Fach = faecherSchreib(Fach);
			AltFach = faecherSchreib(AltFach);
            Regex emptycheck = new Regex(@"^\s*$");
			if (!Helpers.IsEmpty(EntfallStr))
			{
				Entfall = true;
			}
			else
			{
				Entfall = false;
			}
			if (!Helpers.IsEmpty(MitbeStr))
			{
				Mitbetreung = true;
                PrintMitbet = "Ja";
			}
			else
			{
				Mitbetreung = false;
                PrintMitbet = "Nein";
			}
			Line1 = Stunde + ". Std: " + Fach;

			if (Entfall == true)
			{
				Line1 = Stunde + ". Std: " + AltFach;
				Line2 = AltFach + " bei " + Lehrer + " entfällt. ";
			}
			else if (Mitbetreung == true)
			{
				Line2 = Fach + " bei " + Lehrer + " wird durch " + Vertreter + " mitbetreut. | " + Raum;
			}
			else if (Veranstaltung == true)
			{
				Line1 = Stunde + ". Std: Veranstaltung";
				if (Raum != "")
				{
					Line2 = "Raum: " + Raum + " | " + "Mit " + Lehrer + " und " + Klasse;
				}
				else {
					Line2 = "Mit " + Lehrer + " und " + Klasse;
				}
			}
			else if (Helpers.IsEmpty(Lehrer) || !Helpers.IsEmpty(Vertreter))
			{
				Line2 = "Bei " + Vertreter + " in " + Raum;
			}
			else if (Fach != AltFach)
			{
				Line2 = Fach + " bei " + Vertreter + " statt " + AltFach;
			}
			else if (Vertreter != Lehrer)
			{
				Line2 = Vertreter + " vertritt " + Lehrer + " | " + Raum;
			}
			else
			{
				Line2 = "Raum: " + Raum + " | " + Lehrer;
			}
			if (!Helpers.IsEmpty(Notiz))
			{
				Notiz = Helpers.AddSpaces (Notiz);
				Line2 = Notiz + "; " + Line2;
			}
		}
		private string faecherSchreib(string fach)
		{
			if (fach != null)
				fach = fach.ToUpper();
			if (VConfig.lessonAbbr.ContainsKey (fach))
				fach = VConfig.lessonAbbr [fach];
			return fach;
		}
		public string Fach { get; set; }

		public string AltFach { get; set; }

		public string Stunde { get; set; }

		public string Lehrer { get; set; }

		public string Vertreter { get; set; }

		public string Notiz { get; set; }

		public string Raum { get; set; }

		public string EntfallStr { get; set; }

		public bool Entfall { get; set; }

		public string MitbeStr { get; set; }

        public bool Mitbetreung { get; set; }
        public string PrintMitbet { get; set; }

		public bool Veranstaltung { get; set; }

		public string Line1 { get; set; }

		public string Klasse { get; set; }

		public string Line2 { get; set; }

		public DateTime Date { get; set; }

		public bool Head { get; set; }
	}
}

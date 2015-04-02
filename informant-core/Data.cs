using System;
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
			Event = false;
            DateHeader = false;
			Head = true;
			VConfig.Populate ();
		} 
		/// <summary>
		/// Creates a heading for the given day.
		/// </summary>
		/// <param name="date">Date object</param>
		public Data(DateTime date)
		{
			Head = true;
            DateHeader = true;
			Line1 = date.ToString("dddd", new CultureInfo("de-DE")) + ", " + Convert.ToString(date.Day) + "." + Convert.ToString(date.Month);
			Line2 = "";
			VConfig.Populate ();
		}
       
		/// <summary>
		/// Creates a Data object with some fields prefilled
		/// </summary>
		/// <param name="std">Time, preformatted</param>
		/// <param name="subject">Subject</param>
		/// <param name="teacher">Old teacher</param>
		/// <param name="cover">New teacher</param>
		/// <param name="room">Room.</param>
		/// <param name="notice">Note</param>
		public Data(string std, string subject, string teacher, string cover, string room, string notice)
		{
			Subject = subject;
			Teacher = teacher;
			Cover = cover;
			Lesson = std;
			Room = room;
			Notice = notice;
			VConfig.Populate ();
			Refresh();
            DateHeader = false;
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
            DateHeader = false;
		}

		/// <summary>
		/// To be called when poperties were changed. Spells the subjects out and determines the type of event
		/// </summary>
		public Data Refresh()
		{
			Head = false;
			Subject = faecherSchreib(Subject);
			OldSubject = faecherSchreib(OldSubject);
			if (!Helpers.IsEmpty(OutageStr))
			{
				Outage = true;
			}
			else
			{
				Outage = false;
			}
			if (!Helpers.IsEmpty(CareStr))
			{
				Cared = true;
              //  PrintMitbet = "Ja";
			}
			else
			{
				Cared = false;
               // PrintMitbet = "Nein";
			}
		    if (!Helpers.IsEmpty(Subject))
		    {
		        Line1 = Lesson + ". Std: " + Subject;
		    }
		    else
		    {
		        Line1 = Lesson + ". Std: " + OldSubject;
		    }

		    if (Outage)
			{
				Line1 = Lesson + ". Std: " + OldSubject;
				Line2 = OldSubject + " bei " + Teacher + " entfällt.";
			}
			else if (Cared)
			{
				Line2 = Subject + " bei " + Teacher + " wird durch " + Cover + " mitbetreut.";
			    if (Room != null)
			    {
			        Line2 += " | " + Room;
			    }
			}
			else if (Event)
			{
				Line1 = Lesson + ". Std: Veranstaltung";
			    Line2 = "";
				if (!string.IsNullOrEmpty(Room))
				{
					Line2 = "Raum: " + Room + " | ";
				}
				Line2 += "Mit " + Teacher;
                if (!string.IsNullOrWhiteSpace(Group))
                {
                    Line2 += " und " + Group;
                }
			}
			else if (Helpers.IsEmpty(Teacher) && !Helpers.IsEmpty(Cover))
			{
			    Line2 = "Bei " + Cover;
			    if (!string.IsNullOrEmpty(Room))
			    {
                    Line2 += " in " + Room;
			    }
			}
			else if (Subject != OldSubject)
			{
				Line2 = Subject + " bei " + Cover + " statt " + OldSubject;
                if (!string.IsNullOrEmpty(Room))
                {
                    Line2 += " | " + Room;
                }
			}
			else if (Cover != Teacher)
			{
				Line2 = Cover + " vertritt " + Teacher;
                if (!string.IsNullOrEmpty(Room))
                {
                    Line2 += " | " + Room;
                }
			}
			else
			{
				Line2 = Teacher;
                if (!string.IsNullOrEmpty(Room))
                {
                    Line2 += " | " + Room;
                }
			}
			if (!Helpers.IsEmpty(Notice))
			{
				Notice = Helpers.AddSpaces (Notice);
				Line2 = Notice + "; " + Line2;
			}
            return this;
		}
		private string faecherSchreib(string fach)
		{
		    if (fach == null)
		        return null;
			fach = fach.ToUpper();
			if (VConfig.LessonAbbr.ContainsKey (fach))
				fach = VConfig.LessonAbbr [fach];
			return fach;
		}

        /// <summary>
        /// Which new subject is on the schedule
        /// </summary>
		public string Subject { get; set; }

        /// <summary>
        /// The subject that was originally scheduled for this lesson
        /// </summary>
		public string OldSubject { get; set; }

        /// <summary>
        /// The time for the lesson
        /// </summary>
		public string Lesson { get; set; }

        /// <summary>
        /// The previously scheduled teacher
        /// </summary>
		public string Teacher { get; set; }

        /// <summary>
        /// The cover for the old teacher
        /// </summary>
		public string Cover { get; set; }

        /// <summary>
        /// Schedule notices for the lesson
        /// </summary>
		public string Notice { get; set; }

        /// <summary>
        /// The room for the lesson
        /// </summary>
		public string Room { get; set; }

        /// <summary>
        /// The presentation of the outage on the website
        /// </summary>
		public string OutageStr { get; set; }

        /// <summary>
        /// Whether the lesson will be held or not
        /// </summary>
		public bool Outage { get; set; }

        /// <summary>
        /// The presentation of the care state of the lesson on the website
        /// </summary>
		public string CareStr { get; set; }

        /// <summary>
        /// Whether a teacher of another class will look after the lesson's attendees
        /// </summary>
        public bool Cared { get; set; }

        /// <summary>
        /// Whether the lesson is replaced with an special event
        /// </summary>
		public bool Event { get; set; }

        /// <summary>
        /// Presentation of the content, first line
        /// </summary>
		public string Line1 { get; set; }

        /// <summary>
        /// The name of the group to which the rescheduling applies
        /// </summary>
		public string Group { get; set; }

        /// <summary>
        /// Presentation of the content, details
        /// </summary>
		public string Line2 { get; set; }

        /// <summary>
        /// The date of the rescheduled lesson
        /// </summary>
		public DateTime Date { get; set; }

        /// <summary>
        /// Whether this object is an heading
        /// </summary>
		public bool Head { get; set; }

        /// <summary>
        /// Whether this object is an heading containing a date
        /// </summary>
        public bool DateHeader { get; set; }
	}
}

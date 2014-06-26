using System;
using System.Text;

namespace UntisExp
{
	public class Helpers
	{
		public Helpers ()
		{
		}
		public static string TruncateWithPreservation(string s, int len)
		{
			s = s.Replace ("\n", "");
			string[] parts = s.Split(' ');
			StringBuilder sb = new StringBuilder();

			foreach (string part in parts)
			{
				if (sb.Length + part.Length > len)
					break;

				sb.Append(' ');
				sb.Append(part);
			}

			return sb.ToString();
		}
	}
}


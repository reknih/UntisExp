using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UntisExp
{
	public class Helpers
	{
		public Helpers ()
		{
		}
		public static string TruncateWithPreservation(string s, int len)
		{
			s = s.Replace ("\n", " ");
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
		public static string AddSpaces (string s) {
			string Result = Regex.Replace(s, "((?<=\\p{Ll})\\p{Lu})|((?!\\A)\\p{Lu}(?>\\p{Ll})|\\d)", " $0");
			Result = Regex.Replace(Result, "(\\.(?=\\S)|:(?=\\S))(.)", "$1 $2");
			return Result;
		}
	}
}


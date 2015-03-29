using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace UntisExp
{
	public class Helpers
	{
	    public static string TruncateWithPreservation(string s, int len)
		{
			s = s.Replace ("\n", " ");
	        len++;
			string[] parts = s.Split(' ');
			StringBuilder sb = new StringBuilder();

			foreach (string part in parts)
			{
				if (sb.Length + part.Length > len)
					break;

				sb.Append(' ');
				sb.Append(part);
			}

			return sb.ToString().Substring(1);
		}
		public static string AddSpaces (string s) {
			string result = Regex.Replace(s, "((?<=\\p{Ll})\\p{Lu})|((?!\\A)\\p{Lu}(?>\\p{Ll})|\\d)", " $0");
			result = Regex.Replace(result, "(\\.(?=\\S)|:(?=\\S))(.)", "$1 $2");
			return result;
		}
#if WINDOWS_PHONE || DEBUG
        public static string getRandomArrayItem(string [] array) {
            var rand = new Random();
            int pos = rand.Next(array.Length);
            return array[pos];
        }
#endif
#if WINDOWS_APP || DEBUG
        public static int[] getTodayTomorrowNum (List<Data> source) {
            List<int> headIndexes = new List<int>();
            int currIndex = 0;
            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (source[i].Head && !source[i].DateHeader)
                {
                    source.RemoveAt(i);
                }
            }
            foreach (var item in source)
            {
                if (item.DateHeader)
                {
                    headIndexes.Add(currIndex);
                }
                currIndex++;
            }
            if (headIndexes.Count == 1)
            {
                return new[] { source.Count - 1, 0 };
            }
            List<Data>[] listlist = new List<Data>[2];
            currIndex = 0;
            foreach (var item in headIndexes)
            {
                int length;
                try
                {
                    length = headIndexes[currIndex + 1] - item - 1;
                }
                catch {
                    length = (source.Count - 1) - item;
                }
                listlist[currIndex] = source.GetRange(item + 1, length);
                if (listlist[currIndex] == null)
                    listlist[currIndex] = new List<Data>();
                currIndex++;
                if (currIndex >= 2)
                    break;
            }
            return new[] { listlist[0].Count, listlist[1].Count };
        }
#endif
		public static bool IsEmpty(string s)
		{
			Regex emptycheck = new Regex(@"^\s*$");
		    if (s == null) return true;
			if (emptycheck.Matches(s).Count > 0 || s == "")
			{
				return true;
			}
			return false;
		}
	}
}


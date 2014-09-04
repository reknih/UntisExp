using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
        public static string getRandomArrayItem(string [] array) {
            var rand = new Random();
            int pos = rand.Next(array.Length);
            return array[pos];
        }
        /*public static int[] getTodayTomorrowNum (List<Data> source) {
            List<int> headIndexes = new List<int>();
            int currIndex = 0;
            foreach (var item in source)
            {
                if (item.Head == true)
                {
                    headIndexes.Add(currIndex);
                }
                currIndex++;
            }
            if (headIndexes.Count == source.Count)
            {
                return new int[] { 0, 0 };
            }
            else if (headIndexes.Count == 1)
            {
                return new int[] { (source.Count - 1), 0 };
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
                currIndex++;
                if (currIndex == 2)
                    break;
            }
            return new int[] { listlist[0].Count, listlist[1].Count };
        }*/
		public static bool IsEmpty(string s)
		{
			Regex emptycheck = new Regex(@"^\s*$");
			if (emptycheck.Matches(s).Count > 0 || s == "")
			{
				return true;
			}
			return false;
		}
	}
}


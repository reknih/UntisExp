using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace UntisExp
{
    /// <summary>
    /// Some useful methods
    /// </summary>
	public static class Helpers
	{
        /// <summary>
        /// Will truncate a string to a length but never cut words of. Super handy!!
        /// </summary>
        /// <param name="s">The original string</param>
        /// <param name="len">Minimum length</param>
        /// <returns>The truncated string</returns>
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

        /// <summary>
        /// Tries to guess where spaces belong in a closed up string and insert them
        /// </summary>
        /// <param name="s">The closed up string</param>
        /// <returns>The string with guesstimated spaces inserted</returns>
		public static string AddSpaces (string s) {
			string result = Regex.Replace(s, "((?<=\\p{Ll})\\p{Lu})|((?!\\A)\\p{Lu}(?>\\p{Ll})|\\d)", " $0");
			result = Regex.Replace(result, "(\\.(?=\\S)|:(?=\\S))(.)", "$1 $2");
			return result;
		}
#if WINDOWS_PHONE || DEBUG
        /// <summary>
        /// Will get a random item out of a string array
        /// </summary>
        /// <param name="array">The source array</param>
        /// <returns>One random item</returns>
        public static string GetRandomArrayItem(string [] array) {
            var rand = new Random();
            int pos = rand.Next(array.Length);
            return array[pos];
        }
#endif

        /// <summary>
        /// Will get how much entries for today and how much for another day are in a set of <see cref="UntisExp.Data"/> objects
        /// </summary>
        /// <param name="source">The set of <see cref="Data"/> objects</param>
        /// <returns>An int array with two entries representing the two counts</returns>
        public static int[] GetTodayTomorrowNum (List<Data> source) {
            List<int> headIndexes = new List<int>();
            int currIndex = 0;
            int maxIndex = 0;
            source = CleanEntries(source);
            foreach (var item in source)
            {
                if (item.DateHeader)
                {
                    headIndexes.Add(currIndex);
                    maxIndex++;
                }
                currIndex++;
            }
            if (headIndexes.Count == 1)
            {
                return new[] { source.Count - 1, 0 };
            }
            List<Data>[] listlist = new List<Data>[maxIndex];
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
                if (currIndex >= maxIndex)
                    break;
            }
            var res = new int[maxIndex];
            for (int i = 0; i < maxIndex; i++)
            {
                res[i] = listlist[i].Count;
            }
            return res;
        }

        /// <summary>
        /// Will remove all meaningless entries from an list of data
        /// </summary>
        /// <param name="toClean">The dirty list</param>
        /// <returns>The cleaned list</returns>
        private static List<Data> CleanEntries(List<Data> toClean)
        {
            var result = toClean;
            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (result[i].Head && !result[i].DateHeader)
                {
                    result.RemoveAt(i);
                }
            }
            return result;
        } 

        /// <summary>
        /// Should be used  to concatenate two Lists of VPlan data.
        /// </summary>
        /// <param name="l1">The first original list</param>
        /// <param name="l2">The second original list</param>
        /// <returns>The concatenated lists</returns>
		public static List<Data> JoinTwoDataLists(List<Data> l1, List<Data> l2)
        {
            l1 = CleanEntries(l1);
            l2 = CleanEntries(l2);

            var l1Specs = GetTodayTomorrowNum(l1);
            var l2Specs = GetTodayTomorrowNum(l2);

            var l1Meaningful = l1Specs.Any(spec => spec != 0);
            if (!l1Meaningful) return l2;

            var l2Meaningful = l2Specs.Any(spec => spec != 0);
            if (!l2Meaningful) return l1;

		    List<Data> l1Headings = new List<Data>();
		    List<Data> l2Headings = new List<Data>();

		    List<List<Data>> l1Sections = new List<List<Data>>();
		    List<List<Data>> l2Sections = new List<List<Data>>();

		    int l1Cursor = 0;
            for (int i = 0; i < l1Specs.Length; i++)
            {
                var toExpect = l1Specs[i];
                if (toExpect == 0) break;

                l1Headings.Add(l1[l1Cursor]);
                l1Cursor++;

                l1Sections.Add(new List<Data>());

                l1Sections[i].AddRange(l1.GetRange(l1Cursor, toExpect));
                l1Cursor = l1Cursor + toExpect;
            }

		    int l2Cursor = 0;
            for (int i = 0; i < l2Specs.Length; i++)
            {
                var toExpect = l2Specs[i];
                if (toExpect == 0) break;

                l2Headings.Add(l2[l2Cursor]);
                l2Cursor++;

                l2Sections.Add(new List<Data>());

                l2Sections[i].AddRange(l2.GetRange(l2Cursor, toExpect));
                l2Cursor = l2Cursor + toExpect;
            }

            List<List<Data>> outputSections = new List<List<Data>>();
		    List<Data> output = new List<Data>();
		    var added = new bool[l2Headings.Count];

		    for (int i = 0; i < l1Headings.Count; i++)
		    {
                outputSections.Add(new List<Data>());
		        bool wasJoined = false;
		        for (int j = 0; j < l2Headings.Count; j++)
		        {
		            if (l1Headings[i].Date.Date == l2Headings[j].Date.Date)
		            {
		                outputSections.Last().Add(l1Headings[i]);
                        outputSections.Last().AddRange(l1Sections[i]);
                        outputSections.Last().AddRange(l2Sections[j]);
		                added[j] = true;
		                wasJoined = true;
                        break;
		            }
		        }
		        if (!wasJoined)
		        {
		            outputSections.Last().Add(l1Headings[i]);
		            outputSections.Last().AddRange(l1Sections[i]);
		        }
		    }
		    for (int i = 0; i < l2Headings.Count; i++)
		    {
		        if (!added[i])
		        {
		            outputSections.Add(new List<Data>());
                    outputSections.Last().Add(l2Headings[i]);
                    outputSections.Last().AddRange(l2Sections[i]);
                }
		    }

		    var orderedOutputSections = from element in outputSections
		        orderby element.First().Date
		        select element;

		    foreach (var section in orderedOutputSections)
		    {
		        section.Sort();
                output.AddRange(section);
		    }

            return output;
		}

        /// <summary>
        /// Checks whether a string is empty
        /// </summary>
        /// <param name="s">Source</param>
        /// <returns>The result of the check</returns>
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


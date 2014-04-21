using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UntisExp
{
	/// <summary>
	/// This class represents an Untis-group
	/// </summary>
	public class Group
	{
		/// <summary>
		/// Generates an empty group object
		/// </summary>
		public Group() { }

		/// <summary>
		/// Cretes a group object with some info
		/// </summary>
		/// <param name="id">Untis-ID</param>
		/// <param name="name">Display-name</param>
		public Group(int id, string name) {
			ID = id;
			ClassName = name;
		}

		public string ClassName { get; set; }
		public override string ToString()
		{
			return ClassName;
		}
		public int ID { get; set; }
	}
}
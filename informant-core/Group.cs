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
			Id = id;
			ClassName = name;
		}

        /// <summary>
        /// The name of the group
        /// </summary>
		public string ClassName { get; set; }

        /// <summary>
        /// Gets a string representation of the object
        /// </summary>
        /// <returns>The groups name</returns>
		public override string ToString()
		{
			return ClassName;
		}
        /// <summary>
        /// Running number of the group on the website
        /// </summary>
		public int Id { get; set; }
	}
}
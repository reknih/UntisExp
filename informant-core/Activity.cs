using System;

namespace UntisExp
{
    /// <summary>
    /// Helps <see cref="UntisExp.Fetcher">-related methods to keep track of what they're doing</see>
    /// </summary>
	public enum Activity
	{
        /// <summary>
        /// Will start parsing in the beginning
        /// </summary>
		ParseFirstSchedule,
        /// <summary>
        /// Will continue parsing on next page
        /// </summary>
		ParseSecondSchedule,
        /// <summary>
        /// Will get news instead
        /// </summary>
		GetNews
	}
}


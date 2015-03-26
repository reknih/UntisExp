using System;

namespace UntisExp
{
	/// <summary>
	/// Platform independant description for preference management.
	/// Should be implemented for greater unification across UntisExp-Clients
	/// Will eventually be introduced as mandantory in either <c>Fetcher</c> or <c>VConfig</c>
	/// </summary>
	public interface ISettings
	{
		/// <summary>
		/// Writes to a persistent preferences dictionary. Has to be implemented system-wise.
		/// </summary>
		/// <param name="key">Key for the entry</param>
		/// <param name="value">Value only reliably supports string, int, bool, float and long types.</param>
		void write (string key, object value);

		/// <summary>
		/// Returns a value from a persistent preferences dictionary.
		/// </summary>
		/// <returns>The value of the key. Will return null if the key is not assigned to a value</returns>
		/// <param name="key">Key to search for</param>
		object read (string key);
	}
}


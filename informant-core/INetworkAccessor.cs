using System;
using System.IO;

namespace UntisExp
{
    /// <summary>
    /// An interface describing classes interacting with the WWW
    /// </summary>
    public interface INetworkAccessor
    {

        /// <summary>
        /// This method should download text files from the WWW.
        /// </summary>
        /// <param name="url">Rescource identifier for the rescource which shall be downloaded</param>
        /// <param name="callback">Will be called with the body text that was contained in the rescource as an argument</param>
        /// <param name="alertmet">Optional. Will be called with error message as arguments</param>
        /// <param name="returnOnError">Optional. Will be called if network operation failed</param>
        /// <param name="aHead">Optional. Standard error message head</param>
        /// <param name="aBody">Optional. Standard error message caption</param>
        /// <param name="aBtn">Optional. Standard error message button title</param>
        void DownloadData(string url, Action<String> callback, Action<string, string, string> alertmet = null, Action returnOnError = null, string aHead = "", string aBody = "", string aBtn = "");

        /// <summary>
        /// Should download a rescource from the internet and return it to a callback as a stream
        /// </summary>
        /// <param name="url">Rescource identifier for the rescource which shall be downloaded</param>
        /// <param name="callback">Will be called with the stream yielded from the rescource as an argument</param>
        /// <param name="alertmet">Optional. Will be called with error message as arguments</param>
        /// <param name="alerting">Optional. Wether the operation will alert on failures or just silently terminate. Defaults to no.</param>
        /// <param name="aHead">Optional. Standard error message head</param>
        /// <param name="aBody">Optional. Standard error message caption</param>
        /// <param name="aBtn">Optional. Standard error message button title</param>
        void DownloadLegacyStream(string url, Action<Stream> callback, Action<string, string, string> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "");
    }
}

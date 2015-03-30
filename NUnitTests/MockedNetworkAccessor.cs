using System.IO;
using UntisExp;

namespace NUnitTests
{
    public class MockedNetworkAccessor : INetworkAccessor
    {
        public bool CalledDownloadData = false;
        public string CalledUri;
        public string DataToReturn;
        public bool CalledDownloadStream = false;
        public Stream StreamToReturn;

        public void DownloadData(string url, System.Action<string> callback, System.Action<string, string, string> alertmet = null, System.Action returnOnError = null, string aHead = "", string aBody = "", string aBtn = "")
        {
            CalledUri = url;
            CalledDownloadData = true;
            if (DataToReturn != null) callback(DataToReturn);
        }

        public void DownloadLegacyStream(string url, System.Action<Stream> callback, System.Action<string, string, string> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
        {
            CalledUri = url;
            CalledDownloadStream = true;
            if (StreamToReturn != null) callback(StreamToReturn);
        }
    }
}
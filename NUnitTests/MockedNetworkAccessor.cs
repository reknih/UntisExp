using System;
using System.IO;
using UntisExp;

namespace NUnitTests
{
    public class MockedNetworkAccessor : INetworkAccessor
    {
        public bool CalledDownloadData;
        public string CalledUri;
        public string DataToReturn;
        public bool CalledDownloadStream;
        public Stream StreamToReturn;

        public void DownloadData(string url, Action<string> callback, Action<string, string, string> alertmet = null, Action returnOnError = null, string aHead = "", string aBody = "", string aBtn = "")
        {
            CalledUri = url;
            CalledDownloadData = true;
            if (DataToReturn != null) 
                callback(DataToReturn);
        }

        public void DownloadLegacyStream(string url, Action<Stream> callback, Action<string, string, string> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
        {
            CalledUri = url;
            CalledDownloadStream = true;
            if (StreamToReturn != null) callback(StreamToReturn);
        }
    }
}
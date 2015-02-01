using System;
using System.Net;
using System.IO;
using System.Text;

namespace UntisExp
{
	public static class Networking
	{
        public static Action<String> defCallback;
        public static Action<Stream> defStreamCallback;
        public static Action<string, string, string> defAlert;
		public static void DownloadData(string url, Action<String> callback, Action<string, string, string> alertmet, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
		{
			defCallback = callback;
			defAlert = alertmet;
            
			try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
				DoWithResponse(request, (response) =>
					{
						string body;
						if (url.IndexOf(VConfig.url) != -1) {
							body = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1")).ReadToEnd();
						} else {
							body = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
						}
                        try
                        {
                            body = WebUtility.HtmlDecode(body);
                        }
                        catch
                        {
                        }
						defCallback(body);
					});
			}
			catch
			{
				try
				{
					var request = (HttpWebRequest)WebRequest.Create(url);
					request.BeginGetResponse(new AsyncCallback(FinishRequest), request);
				}
				catch
				{
					if (alerting)
						alertmet(aHead, aBody, aBtn);
				}
			}
		}
        public static void DownloadLegacyStream(string url, Action<Stream> callback, Action<string, string, string> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
        {
            defStreamCallback = callback;
            defAlert = alertmet;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.BeginGetResponse(new AsyncCallback(FinishStreamRequest), request);
            }
            catch
            {
                if (alerting)
                    alertmet(aHead, aBody, aBtn);
            }
        }

		public static void DoWithResponse(HttpWebRequest request, Action<HttpWebResponse> responseAction)
		{
			Action wrapperAction = () =>
			{
				request.BeginGetResponse(new AsyncCallback((iar) =>
					{
						try {
                            var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
							responseAction(response);
						} catch (Exception e) {
							if (e.HResult != -2147467261 && e.HResult != -2146233086)
							defAlert(VConfig.noPageErrTtl, VConfig.noPageErrTxt, VConfig.noPageErrBtn);
						}
					}), request);
			};
			wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
				{
					var action = (Action)iar.AsyncState;
					action.EndInvoke(iar);
				}), wrapperAction);
		}
		public static string GetBody(IAsyncResult result)
		{
            string body = "";
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("ISO-8859-1"));
                body = readStream.ReadToEnd();
                try
                {
                    body = WebUtility.HtmlDecode(body);
                }
                catch
                {
                }

            }
            catch {
                defAlert(VConfig.noPageErrTtl, VConfig.noPageErrTxt, VConfig.noPageErrBtn);
            }
			return body;
		}
        public static void FinishRequest(IAsyncResult result)
        {
            string body = GetBody(result);
            if (body != "")
                defCallback(body);
        }
        public static void FinishStreamRequest(IAsyncResult result)
        {
            HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
            Stream receiveStream = response.GetResponseStream();
            defStreamCallback(receiveStream);
        }
    }
}


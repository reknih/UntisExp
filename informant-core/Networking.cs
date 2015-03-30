using System;
using System.Net;
using System.IO;
using System.Text;

namespace UntisExp
{
	/// <summary>
	/// This static class includes methods for interacting with the internet
	/// </summary>
	public static class Networking
	{

        /// <summary>
        /// This method can be used to download text files from the WWW asynchronously.
        /// </summary>
        /// <param name="url">Rescource identifier for the rescource which shall be downloaded</param>
        /// <param name="callback">Will be called with the body text that was contained in the rescource as an argument</param>
        /// <param name="alertmet">Optional. Will be called with error message as arguments</param>
        /// <param name="returnOnError">Optional. Will be called if network operation failed</param>
        /// <param name="aHead">Optional. Standard error message head</param>
        /// <param name="aBody">Optional. Standard error message caption</param>
        /// <param name="aBtn">Optional. Standard error message button title</param>
		public static void DownloadData(string url, Action<String> callback, Action<string, string, string> alertmet = null, Action returnOnError = null, string aHead = "", string aBody = "", string aBtn = "")
		{
            bool alerting = alertmet != null;

            try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
				DoWithResponse(request,alerting,returnOnError,alertmet, response =>
					{
						string body;
						if (url.IndexOf(VConfig.url, StringComparison.Ordinal) != -1) {
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
                            // ignored
                        }
					    callback(body);
					});
			}
			catch
			{
				try
				{ 
					var request = (HttpWebRequest)WebRequest.Create(url);
					request.BeginGetResponse(result => {FinishRequest(callback, alertmet, result);}, new object[] {request, alerting});
				}
				catch
				{
					if (alerting)
						alertmet(aHead, aBody, aBtn);
				}
			}
		}


        /// <summary>
        /// Will download a rescource from the internet asynchonously and returns it to a callback as a stream
        /// </summary>
        /// <param name="url">Rescource identifier for the rescource which shall be downloaded</param>
        /// <param name="callback">Will be called with the stream yielded from the rescource as an argument</param>
        /// <param name="alertmet">Optional. Will be called with error message as arguments</param>
        /// <param name="alerting">Optional. Wether the operation will alert on failures or just silently terminate. Defaults to no.</param>
        /// <param name="aHead">Optional. Standard error message head</param>
        /// <param name="aBody">Optional. Standard error message caption</param>
        /// <param name="aBtn">Optional. Standard error message button title</param>
        public static void DownloadLegacyStream(string url, Action<Stream> callback, Action<string, string, string> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.BeginGetResponse(result => {FinishStreamRequest(result, callback, alertmet);}, request);
            }
            catch
            {
                if (alerting && alertmet != null)
                    alertmet(aHead, aBody, aBtn);
            }
        }

	    private static void DoWithResponse(HttpWebRequest request, bool alerting, Action onError, Action<string,string,string> alert, Action<HttpWebResponse> responseAction)
		{
			Action wrapperAction = () =>
			{
				request.BeginGetResponse(iar =>
				{
				    try {
				        var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
				        responseAction(response);
				    } catch {			
				        if (onError != null)
				            onError();
				        if(alerting)
				            alert(VConfig.noPageErrTtl, VConfig.noPageErrTxt, VConfig.noPageErrBtn);
				    }
				}, request);
			};
			wrapperAction.BeginInvoke(iar =>
			{
			    var action = (Action)iar.AsyncState;
			    action.EndInvoke(iar);
			}, wrapperAction);
		}

	    private static string GetBody(Action<string,string,string> alert, IAsyncResult result)
		{
            Object[] param = (Object[])result.AsyncState;
            string body = "";
            try
            {
                var httpWebRequest = param[0] as HttpWebRequest;
                if (httpWebRequest != null)
                {
                    HttpWebResponse response = httpWebRequest.EndGetResponse(result) as HttpWebResponse;
                    if (response != null)
                    {
                        Stream receiveStream = response.GetResponseStream();
                        StreamReader readStream = new StreamReader(receiveStream, Encoding.GetEncoding("ISO-8859-1"));
                        body = readStream.ReadToEnd();
                    }
                    else
                    {
                        if (alert != null) alert(VConfig.unknownErrTtl, VConfig.unknownErrTxt, VConfig.unknownErrBtn);
                    }
                }
                try
                {
                    body = WebUtility.HtmlDecode(body);
                }
                catch
                {
                    // ignored
                }
            }
            catch {
                if ((bool)param[1])
                {
                    alert(VConfig.noPageErrTtl, VConfig.noPageErrTxt, VConfig.noPageErrBtn);
                }
            }
			return body;
		}

	    private static void FinishRequest(Action<string> callback,Action<string,string,string> alert, IAsyncResult result)
        {
            string body = GetBody(alert, result);
                callback(body);
        }

	    private static void FinishStreamRequest(IAsyncResult result, Action<Stream> streamCallback, Action<string,string,string> alert)
	    {
	        HttpWebRequest request = result.AsyncState as HttpWebRequest;
	        if (request != null)
	        {
	            HttpWebResponse response = request.EndGetResponse(result) as HttpWebResponse;
	            if (response != null)
	            {
	                Stream receiveStream = response.GetResponseStream();
	                streamCallback(receiveStream);
	            }
	            else
	            {
	                if (alert != null) alert(VConfig.unknownErrTtl, VConfig.unknownErrTxt, VConfig.unknownErrBtn);
	            }
	        }
	        else
	        {
                if (alert != null) alert(VConfig.unknownErrTtl, VConfig.unknownErrTxt, VConfig.unknownErrBtn);
	        }
	    }
	}
}


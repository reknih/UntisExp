using System;
using System.Net;
using System.IO;
using System.Text;
using UntisExp.EventHandlers;
using UntisExp.Interfaces;

namespace UntisExp
{
	/// <summary>
	/// This static class includes methods for interacting with the internet
	/// </summary>
	public class Networking : INetworkAccessor
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
		public void DownloadData(string url, Action<String> callback, Action<ErrorMessageEventArgs> alertmet = null, Action returnOnError = null, string aHead = "", string aBody = "", string aBtn = "")
		{
            bool alerting = alertmet != null;

            try
			{
				var request = (HttpWebRequest)WebRequest.Create(url);
                #if LEHRER
                if(url.IndexOf(VConfig.Url, StringComparison.Ordinal) != -1)
                {
                    request.Credentials = new NetworkCredential("cwslehrer","u51n63n");
                }          
                #endif
				DoWithResponse(request, alerting, returnOnError, alertmet, response =>
					{
					    var body = url.IndexOf(VConfig.Url, StringComparison.Ordinal) != -1 ? new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1")).ReadToEnd() : new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
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
			catch (Exception e)
			{
				try
				{ 
					var request = (HttpWebRequest)WebRequest.Create(url);
                    if (url.IndexOf(VConfig.Url, StringComparison.Ordinal) != -1)
                    {
                        request.Credentials = new NetworkCredential("cwslehrer", "u51n63n");
                    } 
					request.BeginGetResponse(result => {FinishRequest(callback, alertmet, result);}, new object[] {request, alerting});
				}
				catch
				{
					if (alerting)
						alertmet(new ErrorMessageEventArgs(aHead, aBody, aBtn));
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
        public void DownloadLegacyStream(string url, Action<Stream> callback, Action<ErrorMessageEventArgs> alertmet = null, bool alerting = false, string aHead = "", string aBody = "", string aBtn = "")
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.BeginGetResponse(result => {FinishStreamRequest(result, callback, alertmet);}, request);
            }
            catch
            {
                if (alerting && alertmet != null)
                    alertmet(new ErrorMessageEventArgs(aHead, aBody, aBtn));
            }
        }

	    private void DoWithResponse(HttpWebRequest request, bool alerting, Action onError, Action<ErrorMessageEventArgs> alert, Action<HttpWebResponse> responseAction)
		{
			Action wrapperAction = () =>
			request.BeginGetResponse (iar => {
				try {
					var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse (iar);
					responseAction (response);
					} catch (WebException e) {
					if (onError != null)
						onError ();
					if (alerting)
						alert (new ErrorMessageEventArgs (VConfig.NoPageErrTtl, VConfig.NoPageErrTxt, VConfig.NoPageErrBtn));
				}
			}, request);
			wrapperAction.BeginInvoke(iar =>
			{
			    var action = (Action)iar.AsyncState;
			    action.EndInvoke(iar);
			}, wrapperAction);
		}

	    private string GetBody(Action<ErrorMessageEventArgs> alert, IAsyncResult result)
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
                        if (alert != null) alert(new ErrorMessageEventArgs(VConfig.UnknownErrTtl, VConfig.UnknownErrTxt, VConfig.UnknownErrBtn));
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
                    alert(new ErrorMessageEventArgs(VConfig.NoPageErrTtl, VConfig.NoPageErrTxt, VConfig.NoPageErrBtn));
                }
            }
			return body;
		}

	    private void FinishRequest(Action<string> callback,Action<ErrorMessageEventArgs> alert, IAsyncResult result)
        {
            string body = GetBody(alert, result);
                callback(body);
        }

	    private void FinishStreamRequest(IAsyncResult result, Action<Stream> streamCallback, Action<ErrorMessageEventArgs> alert)
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
	                if (alert != null) alert(new ErrorMessageEventArgs(VConfig.UnknownErrTtl, VConfig.UnknownErrTxt, VConfig.UnknownErrBtn));
	            }
	        }
	        else
	        {
                if (alert != null) alert(new ErrorMessageEventArgs(VConfig.UnknownErrTtl, VConfig.UnknownErrTxt, VConfig.UnknownErrBtn));
            }
	    }
	}
}


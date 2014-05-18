using System;
using System.Net;
using System.IO;
using System.Text;

namespace UntisExp
{
	public static class Networking
	{
		public static Action<String> defCallback;
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
						var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
						callback(body);
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
			HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
			Stream receiveStream = response.GetResponseStream();
			StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
			string body = readStream.ReadToEnd();
			return body;
		}
		public static void dummy (string a1, string a2, string a3){
		}
		public static void FinishRequest(IAsyncResult result)
		{
			string body = Networking.GetBody(result);
			defCallback(body);
		}
	}
}


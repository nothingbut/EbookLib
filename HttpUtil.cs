/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2012/9/27
 * Time: 8:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Net;
using System.Text;

namespace EbookLib {
	/// <summary>
	/// Description of HttpHelper.
	/// </summary>
	public class HttpUtil {
		public static Encoding GetHtmlFromUrl(string url, ref string html) {
			if (string.IsNullOrEmpty(url))
				throw new ArgumentNullException("url", "Parameter is null or empty");

			try {
				HttpWebRequest request = GenerateHttpWebRequest(url);
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
				{
					Encoding encode = CommonUtil.getEncoding(response.CharacterSet);
					// Get the response stream.
					Stream responseStream = response.GetResponseStream();
					using (StreamReader reader =
					       new StreamReader(responseStream, encode)) {
						html = reader.ReadToEnd();
						return encode;
					}
				}
			}
			catch (Exception ex) {
				LogUtil.Error("Download url: " + url + " failed as " + ex.ToString());
				return null;
			}
		}
		
		private static HttpWebRequest GenerateHttpWebRequest(string UriString) {
			// Get a Uri object.
			Uri Uri = new Uri(UriString);
			// Create the initial request.
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(Uri);
			// Return the request.
			return httpRequest;
		}
	}
}

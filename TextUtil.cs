/*
 * 由SharpDevelop创建。
 * 用户： chshi
 * 日期: 2012/12/12
 * 时间: 17:17
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EbookLib {
	/// <summary>
	/// Description of TextUtil.
	/// </summary>
	public class TextUtil {
		const string ContentStart = "<!--BookContent Start-->";
		const string ContentEnd = "<!--BookContent End-->";
		const string EndOfParagraph = "";
		const string ImgUrlRegex = "\\<img src=\"[^\\>]+\\>";
		
		public static string getContent(string raw) {
			if (raw.Equals(""))
				throw new Exception("Content is empty");
			string content = raw;
			int nBegin = content.IndexOf(ContentStart);
			if (nBegin == -1)
				throw new Exception("Start string not found!");
			nBegin += ContentStart.Length;
			int nEnd = content.IndexOf(ContentEnd);
			if (nBegin == -1)
				throw new Exception("End string not found!");
			if (nBegin > nEnd)
				throw new Exception("Start & end string overlapped!");
			return content.Substring(nBegin , nEnd - nBegin);
		}
		
		public static List<string> getImgList(string content) {
			List<string> imgList = new List<string>();
			if (content == null)
				return imgList;
			Regex regImgHref = new Regex(ImgUrlRegex, RegexOptions.IgnoreCase);
			foreach (Match match in regImgHref.Matches(content)) {
				string strImgFull = match.Value;
				int first = strImgFull.IndexOf('"'), last = strImgFull.LastIndexOf('"');
				if (first == -1 || last == -1)
					continue;
				first ++;
				if (first > last)
					continue;
				imgList.Add(strImgFull.Substring(first, last - first));
			}
			return imgList;
		}
		
		public static string arrangeContent(string content) {
			string strLineBreak = "</p>\r\n<p>";
			string temp = Regex.Replace(content, @"[\x01-\x1F,\x7F]", string.Empty).Replace("&nbsp;&nbsp;", "　");
			return Regex.Replace(content, @"[\x01-\x1F,\x7F]", string.Empty).Replace("&nbsp;&nbsp;", "　")
					.Replace("<br>", strLineBreak).Replace("</br>", strLineBreak).Replace("<br >",strLineBreak)
					.Replace("<BR>", strLineBreak).Replace("<br />", strLineBreak).Replace("<P>", strLineBreak)
					.Replace("()",string.Empty).Replace("<center>",string.Empty).Replace("</center>",string.Empty)
				    .Replace("&#39;","\"").Replace("&amp;","&");
		}
		
		public static bool isEmpty(string content) {
			content = content.Replace("\r"," ").Replace("\n"," ").Replace("\t"," ");
			return (content.Trim().Length == 0);
		}
	}
}

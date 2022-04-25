/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2013/2/1
 * Time: 10:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using EbookLib;

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;

namespace EbookLib.WebDonwload
{
	public class CommonHelper {
		public enum TokenMatch {
			Token_Full,
			Token_Incl,
			Token_Excl,
			Token_Pre,
			Token_Appd,
		};
		
		public static NodeList getNodeList(string html) {
			Lexer lexer = new Lexer(html);
			Parser parser = new Parser(lexer);
			NodeList htmlNodes = parser.Parse(null);
			return htmlNodes;
		}
		
		public static INode getFirstTagByToken(NodeList nodeList, string token, TokenMatch rule) {
			if (nodeList == null) return null;
			for (int i = 0; i < nodeList.Count; i ++) {
				INode htmlNode = nodeList[i];
				if (htmlNode is ITag) {
					ITag tag = (ITag)htmlNode;
					string content = tag.GetText();
					if (isMatch(content, token, rule))
						return htmlNode;
					INode tempNode = getFirstTagByToken(htmlNode.Children, token, rule);
					if (tempNode != null) return tempNode;
				}
			}
			return null;
		}
		
		public static bool isMatch(string content, string token, TokenMatch rule) {
			switch (rule) {
				case TokenMatch.Token_Full:
				case TokenMatch.Token_Incl:
					return content.Contains(token);
				case TokenMatch.Token_Excl:
					return !content.Contains(token);
				case TokenMatch.Token_Pre:
					return content.StartsWith(token);
				case TokenMatch.Token_Appd:
					return content.EndsWith(token);
				default:
					return false;
			}
		}
		
		public static string getValue(string content, int start) {
			Regex regex = new Regex(HtmlToken.ValueRegex);
			Match match = regex.Match(content, start);
			if (!match.Success) return null;
			return match.Groups[0].Value;
		}
		
		public static string preprocess(string content) {
			return content.Replace("&nbsp;", " ").Replace("<br />", "\r\n").Replace("<br>", "\r\n");
		}
		
		public static bool isExclude(INode node) {
			if (!(node is ITag))
				return false;
			ITag tag = (ITag)node;
			string text = tag.GetText();
			if (text.StartsWith("script"))
				return true;
			if (text.StartsWith("meta"))
				return true;
			if (text.StartsWith("link"))
			    return true;
			if (text.StartsWith("div id=\"top_nav_wrap\""))
			    return true;
			return false;
		}
	}
	
	public class WebPage
	{
		private string url;
		private string html;
		private Encoding encode;
		private NodeList nodeList;
		
		public WebPage(string url) {
			this.url = url;
			this.html = null;
		}
		
		public bool init() {
			encode = HttpUtil.GetHtmlFromUrl(url, ref html);
			if (encode == null) return false;

			html = CommonHelper.preprocess(html);
			nodeList = CommonHelper.getNodeList(html);
			if (nodeList == null) {
				cleanup();
				return false;
			}
			
			return true;
		}
		
		public void cleanup() {
			url = null;
			html = null;
			nodeList = null;
		}
		
		public void dumpHtml(string filename) {
			File.WriteAllText(filename, html);
		}
		
		public void dumpAllNodes(string filename) {
			File.WriteAllText(filename,"", encode);
			dumpNodeList(filename, nodeList);
		}
		
		private static void dumpNodeList(string filename, NodeList nList) {
			if (nList == null)
				return;
			for (int i = 0; i < nList.Count; i ++) {
				INode htmlNode = nList[i];
				if (CommonHelper.isExclude(htmlNode))
					continue;
				string text = htmlNode.GetText();
				if (htmlNode is IText) {
					if (!TextUtil.isEmpty(text))
						File.AppendAllText(filename, "TXT:" + text + "\r\n");
					continue;
				}
				File.AppendAllText(filename, "\r\n");
				File.AppendAllText(filename, "TAG:" + htmlNode.GetText() + "{\r\n");
				dumpNodeList(filename, htmlNode.Children);
				File.AppendAllText(filename, "}\r\n");
			}
		}
	}
}

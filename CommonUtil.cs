/*
 * 由SharpDevelop创建。
 * 用户： chshi
 * 日期: 2012/12/11
 * 时间: 8:32
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace EbookLib {
	/// <summary>
	/// Description of CommonConfig.
	/// </summary>
	public static class CommonUtil {
		public const int LOG_LEVEL_INFO = 1;
		public const int LOG_LEVEL_WARN = 2;
		public const int LOG_LEVEL_ERROR = 3;
		public const int LOG_LEVEL_FATAL = 4;

		public static Encoding GB2312 = Encoding.GetEncoding("GB2312");
		public static Encoding UTF8 = Encoding.UTF8;
		
		public static Encoding getEncoding(string encoding) {
			switch (encoding) {
				case "ISO-8859-1":
					return GB2312;
				case "UTF-8":
					return UTF8;
				default:
					return UTF8;
			}
			
		}
		
		public static void init(string filePath = "config.xml") {
		}
		
		private static string strRootPath = "Z:\\books\\";
		public static string getRootPath() {
			return strRootPath;
		}
		
		private static string strArchPath = "Z:\\books\\archive\\";
		public static string getArchPath() {
			return strArchPath;
		}
		
		private static string strGenPath = "Z:\\books\\gen\\";
		public static string getGenPath() {
			return strGenPath;
		}
		
		private static string strTempPath = getRootPath() + "temp\\";
		public static string getTempPath() {
			return strTempPath;
		}
		
		private static string strTemplPath = getRootPath() + "templ\\";
		public static string getTemplatePath() {
			return strTemplPath;
		}
		
		private static string strLogFile = "Z:\\books\\Books.log";
		public static string getLogFile() {
			return strLogFile;
		}
		
		public static int logLevel() {
			return LOG_LEVEL_INFO;
		}
		
		public static string getTempOEBPS() {
			return getTempPath() + "OEBPS\\";
		}
		
		public static string getTempMETA() {
			return getTempPath() + "META-INF\\";
		}
		
		public static string getMd5Hash(string input) {
			byte[] buffer = MD5.Create().ComputeHash(Encoding.Default.GetBytes(input));
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < buffer.Length; i++)	{
				builder.Append(buffer[i].ToString("x2"));
			}
			return builder.ToString();
		}
		
		#region tianya download
		public static int Invalid_ID = -1;
		public static string XMLFileHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
		public static string BookShelf_FileName = "BookShelf.xml";
		public static string BookInfo_FileName = "BookInfo.xml";
		public static int MaxPageNo = 1000;
		public static int MinContentSize = 300;
		
		private static string strTyRootPath = getRootPath() + "Tianya\\";
		public static string getTyRootPath() {
			return strTyRootPath;
		}
		
		public static string getBookPath(string rootPath, int bookID) {
			return rootPath + bookID.ToString().PadLeft(4, '0') + "\\";
		}
		
		public static string getPageFile(string bookPath, int pageNo) {
			return bookPath + pageNo.ToString().PadLeft(3, '0') + ".xml";
		}
		#endregion
	}

	public static class HtmlToken {
		public static string EncodeToken = "charset";
		public static string ValueStart = "=\"";
		public static string ValueEnd = "\"";
		public static string ValueRegex ="[A-Za-z0-9_\\-]+";
	}
	
	public static class TianyaToken {
		public static string TitleToken = "";
		public static string OwnerToken = "";
		public static string SectionToken = "";
		public static string ContentToken = "";
		public static string AuthorOnwerToken = "";
		public static string AuthorOtherToken = "";
	}
	
}

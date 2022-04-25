/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2012/12/10
 * Time: 9:03
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using EbookLib.WebDonwload;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using Winista.Text.HtmlParser;
using Winista.Text.HtmlParser.Lex;
using Winista.Text.HtmlParser.Util;
using Winista.Text.HtmlParser.Tags;
using Winista.Text.HtmlParser.Filters;

namespace EbookLib {
	class Program {
		public static void Main(string[] args) {
			LogUtil.Cleanup();
			// add some method 
			bookHandler();
			LogUtil.Info("Test Done");
		}
		
		private static void bookHandler() {
			MDBUtil util = new MDBUtil(CommonUtil.getRootPath() + "pim.mdb");
			util.init();

			BookShelf shelf = new BookShelf();
			shelf.init();
//			cleanupAll(shelf);
			genEpub(shelf);
		}

		private static void cleanupAll(BookShelf shelf) {
			BookFuncDelegate funcDelegate = new BookFuncDelegate(BookUtil.processingBook);
			shelf.getShelf().processBooks(funcDelegate);
		}
		
		private static void genEpub(BookShelf shelf, string title = "N/A") {
			Hashtable parameters = new Hashtable();
			parameters["bookTitleContain"] = "";
			List<string> titleList = new List<string>();
			titleList.Add(title);
			parameters["bookTitleList"] = titleList;
			BookPickDelegate pickByTitleContain = new BookPickDelegate(BookUtil.checkBookNotInList);
			
			List<BookInfo> bookList = shelf.pickBooks(pickByTitleContain, parameters, "root");
			foreach (BookInfo book in bookList) {
				if (File.Exists(FileUtil.getGenPath(book) + book.getTitle() + ".epub")) {
					continue;
				}
				book.initContent();
				BookUtil.genEpub(book);
			}
		}

		public static void webPageTest(string url = TianyaTestUrl) {
			WebPage wp = new WebPage(url);
			if (wp.init()) {
				wp.dumpAllNodes("d:\\Downloads\\node.txt");
			}
		}
		
		const string TianyaTestUrl = "http://bbs.tianya.cn/post-no05-187612-1.shtml";
		const string TocTestUrl = "http://www.siluke.com/0/82/82336/";
		const string ContentTestUrl = "http://www.siluke.com/0/82/82336/12017248.html";
	}
}
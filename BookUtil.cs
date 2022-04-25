/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2012/12/13
 * Time: 14:42
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace EbookLib {
	public delegate void BookFuncDelegate(BookInfo book);
	public delegate bool BookPickDelegate(BookInfo book, Hashtable parameters);

	/// <summary>
	/// Description of BookUtil.
	/// </summary>
	public class BookUtil {
		#region common util
		public static void dumpBookTitle(BookInfo book) {
			LogUtil.Error(book.getTitle());
		}
		
		public static void processingBook(BookInfo book) {
			book.initContent();
//			archieveBook(book);
			cleanupBook(book);
		}
		
		public static void archieveBook(BookInfo book) {
			book.exportInfo(book.getPath() + @"\archieve.htm");
			string targetFile = CommonUtil.getArchPath() + book.getID() + ".zip";
			
			if (File.Exists(targetFile))
				File.Delete(targetFile);
            new FastZip(new FastZipEvents()).CreateZip(CommonUtil.getArchPath() + book.getID() + ".zip", 
			                                           book.getPath(), true, null);
			File.Delete(book.getPath() + @"\archieve.htm");
		}
		
		public static void validBook(BookInfo book) {
			string strCover = book.getCoverFile();
			if (!strCover.Equals("") && !File.Exists(book.getPath() + strCover)) {
				LogUtil.Error("Cover file for: " + book.getTitle() + " not found");
			}
			
			foreach (BookVolumn volumn in book.getAllContents()) {
				foreach (BookChapter chapter in volumn.getChapters()) {
					string fullFileName = book.getPath() + chapter.getId() + ".htm";
					if (!File.Exists(fullFileName)) {
						LogUtil.Error(book.getTitle() + ":" + chapter.getTitle() + " not found");
						continue;
					}
					try {
						copyImageFile(chapter, book.getPath(), null);
					} catch (Exception ex) {
						LogUtil.Error("For " + book.getTitle() + ":" + chapter.getTitle() + " Error: " + ex.Message);
					}
				}
			}
		}
		
		public static void checkImgChapter(BookInfo book) {
			foreach (BookVolumn volumn in book.getAllContents()) {
				foreach (BookChapter chapter in volumn.getChapters()) {
					string fullFileName = book.getPath() + chapter.getId() + ".htm";
					if (!File.Exists(fullFileName)) {
						LogUtil.Error(book.getTitle() + ":" + chapter.getTitle() + " not found");
						continue;
					}
					try {
						if (copyImageFile(chapter, book.getPath(), null))
							LogUtil.Info(book.getTitle() + ":" + chapter.getTitle() + " is a Image Chapter");
					} catch (Exception ex) {
						LogUtil.Error("For " + book.getTitle() + ":" + chapter.getTitle() + " Error: " + ex.Message);
					}
				}
			}
		}
		
		public static void cleanupBook(BookInfo book) {
			string tempDir = CommonUtil.getRootPath() + @"\temp\";
			try {
				if (Directory.Exists(tempDir)) {
					Directory.Delete(tempDir, true);
				}
				Directory.CreateDirectory(tempDir);
			} catch (Exception ex) {
				LogUtil.Error(ex.Message);
			}
			
			foreach (BookVolumn volumn in book.getAllContents()) {
				foreach (BookChapter chapter in volumn.getChapters()) {
					string fullFileName = book.getPath() + chapter.getId() + ".htm";
					if (!File.Exists(fullFileName)) {
						LogUtil.Error(book.getTitle() + ":" + chapter.getTitle() + " not found");
						continue;
					}
					try {
						File.Copy(fullFileName, tempDir + chapter.getId() + ".htm");
						copyImageFile(chapter, book.getPath(), tempDir);
						chapter.cleanup();
					} catch (Exception ex) {
						LogUtil.Error("For " + book.getTitle() + ":" + chapter.getTitle() + " Error: " + ex.Message);
					}
				}
			}
			
			string strCover = book.getCoverFile();
			if (!strCover.Equals("")) {
				strCover = book.getPath() + strCover;
				if (!File.Exists(strCover))
					LogUtil.Error("Cover file for: " + book.getTitle() + " not found");
				else {
					try {
						File.Copy(strCover, tempDir + book.getCoverFile());
					} catch (Exception ex) {
						LogUtil.Error("Error for " + book.getTitle() + " as " + ex.Message);
					}
				}
			}
			try {
				Directory.Delete(book.getPath(), true);
				Directory.Move(tempDir, book.getPath());
			} catch (Exception ex) {
				LogUtil.Error(ex.Message);
			}
		}
		
		public static bool checkBookByID(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookID"))
				return false;
			if (book.getID() != parameters["bookID"].ToString())
				return false;
			return true;
		}
		
		public static bool checkBookByTitle(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookTitle"))
				return false;
			if (book.getTitle() != parameters["bookTitle"].ToString())
				return false;
			return true;
		}

		public static bool checkBookByTitlePrefix(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookTitlePrefix"))
				return false;
			if (book.getTitle().IndexOf(parameters["bookTitlePrefix"].ToString()) == 0)
				return true;
			return false;
		}

		public static bool checkBookByTitleContain(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookTitleContain"))
				return false;
			if (book.getTitle().Contains(parameters["bookTitleContain"].ToString()))
				return true;
			return false;
		}
		
		public static bool checkBookInList(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookTitleList"))
				return false;
			List<string> bookList = (List<string>)(parameters["bookTitleList"]);
			if (bookList.Remove(book.getTitle()) )
			    return true;
			return false;
		}
		
		public static bool checkBookNotInList(BookInfo book, Hashtable parameters) {
			if (!parameters.ContainsKey("bookTitleList"))
				return false;
			List<string> bookList = (List<string>)(parameters["bookTitleList"]);
			if (bookList.Contains(book.getTitle()) )
			    return false;
			return true;
		}
		
		private static bool copyImageFile(BookChapter chapter, string bookPath, string tempPath) {
			if (!chapter.loadChapter(bookPath))
				return false;
			foreach (string strImgFile in chapter.getImgList()) {
				if (!File.Exists(bookPath + strImgFile))
					throw new Exception("Image file " + strImgFile + " not found");
				if (tempPath != null)
					File.Copy(bookPath + strImgFile, tempPath + strImgFile.ToLower() );
			}
			return true;
		}
		#endregion
		
		#region common gen
		private static void createCoverPage(BookInfo book, string tempPath = null) {
			if (tempPath == null)
				tempPath = CommonUtil.getTempOEBPS();
			string strFileName = tempPath + "cover.html";
			StreamWriter FileWriteVOL = new StreamWriter(File.OpenWrite(strFileName), Encoding.UTF8);
			FileWriteVOL.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">");
			FileWriteVOL.WriteLine("<head>");
			FileWriteVOL.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			FileWriteVOL.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"main.css\"/>");
			FileWriteVOL.WriteLine("<title>{0}</title>", book.getTitle());
			FileWriteVOL.WriteLine("</head>");
			FileWriteVOL.WriteLine("<body>");
			FileWriteVOL.WriteLine("<div>");
			FileWriteVOL.WriteLine("<p><center><h1>{0}</h1></center></p>", book.getTitle());
			FileWriteVOL.WriteLine("<p><center><h3>{0}</h3></center></p>", book.getAuthor());
			if (File.Exists(book.getPath() + book.getCoverFile())) {
				FileWriteVOL.WriteLine("<p><center><img src=\"{0}\"/></center></p>", book.getCoverFile().ToLower());
				if (!File.Exists(tempPath + book.getCoverFile())) {
					File.Copy(book.getPath() + book.getCoverFile(), tempPath + book.getCoverFile().ToLower());
				}
			}
			FileWriteVOL.WriteLine("<p></p><center><!--BookContent Start-->");
			FileWriteVOL.WriteLine("<p>{0}</p>", book.getBrief().Replace("\r\n","</p><p>").Replace("　　",""));
			FileWriteVOL.WriteLine("<!--BookContent Start--></center>");
			FileWriteVOL.WriteLine("</div>");
			FileWriteVOL.WriteLine("</body>");
			FileWriteVOL.WriteLine("</html>");
			FileWriteVOL.Flush();
			FileWriteVOL.Close();
		}
		
		private static string createVolumnPage(BookInfo book, int volID, string tempPath = null) {
			if (tempPath == null)
				tempPath = CommonUtil.getTempOEBPS();

			string strFileName = "tmp_v_" + volID.ToString();
			BookVolumn vol = book.getAllContents()[volID];
			StreamWriter FileWriteVOL = new StreamWriter(File.OpenWrite(tempPath + strFileName + ".html"), Encoding.UTF8);
			FileWriteVOL.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">");
			FileWriteVOL.WriteLine("<head>");
			FileWriteVOL.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			FileWriteVOL.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"main.css\"/>");
			FileWriteVOL.WriteLine("<title>{0}</title>", vol.getTitle());
			FileWriteVOL.WriteLine("</head>");
			FileWriteVOL.WriteLine("<body>");
			FileWriteVOL.WriteLine("<div>");
			FileWriteVOL.WriteLine("<center><h1>{0}</h1></center><p></p>", vol.getTitle());
			FileWriteVOL.WriteLine("<!--BookContent Start-->");
			FileWriteVOL.WriteLine("<center><p></p><p></p></center>");
			FileWriteVOL.WriteLine("<center>{0}</center><p></p>", vol.getChapters()[0].getTitle());
			if (vol.getChapters().Count > 1)
			{
				FileWriteVOL.WriteLine("<center>~</center><p></p>");
				FileWriteVOL.WriteLine("<center>{0}</center><p></p>", book.getAllContents()[volID].getChapters()[vol.getChapters().Count -1].getTitle());
			}
			FileWriteVOL.WriteLine("<!--BookContent Start-->");
			FileWriteVOL.WriteLine("</div>");
			FileWriteVOL.WriteLine("</body>");
			FileWriteVOL.WriteLine("</html>");
			FileWriteVOL.Flush();
			FileWriteVOL.Close();
			
			return strFileName;
		}
		
		private static string prepareChapter(BookChapter chapter, string bookPath, string tempPath = null) {
			if (tempPath == null)
				tempPath = CommonUtil.getTempOEBPS();
			
			string strFileName = chapter.getId();
			StreamWriter FileWriteChapt = new StreamWriter(File.OpenWrite(CommonUtil.getTempOEBPS() + strFileName + ".html"), Encoding.UTF8);
			
			FileWriteChapt.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"zh-CN\">");
			FileWriteChapt.WriteLine("<head>");
			FileWriteChapt.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
			FileWriteChapt.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"main.css\"/>");
			FileWriteChapt.WriteLine("<title>{0}</title>", chapter.getTitle());
			FileWriteChapt.WriteLine("</head>");
			FileWriteChapt.WriteLine("<body>");
			FileWriteChapt.WriteLine("<div>");
			FileWriteChapt.WriteLine("<h2>{0}</h2><p></p>", chapter.getTitle());
			
			chapter.loadChapter(bookPath);
			if (chapter.getContent() == null) {
				throw new Exception("Chapter " + chapter.getTitle() + " is Empty");
			}
			string content = "<!--BookContent Start--><p>" + TextUtil.arrangeContent(chapter.getContent()) + "</p><!--BookContent End--></div></body></html>";
			FileWriteChapt.Write(content);
			FileWriteChapt.Close();
			
			return strFileName;
		}
		#endregion
		
		#region epub gen
		private static void prepareDirectory() {
			if (Directory.Exists(CommonUtil.getTempPath()) ) {
				Directory.Delete(CommonUtil.getTempPath(), true);
			}
			Directory.CreateDirectory(CommonUtil.getTempPath());
			Directory.CreateDirectory(CommonUtil.getTempOEBPS());
			Directory.CreateDirectory(CommonUtil.getTempMETA());
		}
		
		private static void prepareEpubContent(BookInfo book) {
			string sTempOEBPSDir = CommonUtil.getTempOEBPS();
			
			string strBookID = "BookConvert-" + CommonUtil.getMd5Hash(book.getTitle() + "-" + book.getAuthor()).Substring(8, 8);

			StreamWriter FileWriteOPF = new StreamWriter(File.OpenWrite(sTempOEBPSDir + "content.opf"), Encoding.UTF8);
			StreamWriter FileWriteNCX = new StreamWriter(File.OpenWrite(sTempOEBPSDir + "toc.ncx"), Encoding.UTF8);
			
			#region init of toc.ncx
			FileWriteNCX.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
			FileWriteNCX.WriteLine();
			FileWriteNCX.WriteLine("<!DOCTYPE ncx");
			FileWriteNCX.WriteLine("PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
			FileWriteNCX.WriteLine("<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">");
			FileWriteNCX.WriteLine("<head>");
			FileWriteNCX.WriteLine("<meta name=\"cover\" content=\"cover\"/>");
			FileWriteNCX.WriteLine("<meta name=\"dtb:uid\" content=\"isbn:" + strBookID + "\"/>");
			FileWriteNCX.WriteLine("<meta name=\"dtb:depth\" content=\"-1\"/>");
			FileWriteNCX.WriteLine("<meta name=\"dtb:totalPageCount\" content=\"0\"/>");
			FileWriteNCX.WriteLine("<meta name=\"dtb:maxPageNumber\" content=\"0\"/>");
			FileWriteNCX.WriteLine("</head>");
			FileWriteNCX.WriteLine();
			FileWriteNCX.WriteLine("<docTitle>");
			FileWriteNCX.WriteLine("<text>" + HttpUtility.HtmlEncode(book.getTitle()) + "</text>");
			FileWriteNCX.WriteLine("</docTitle>");
			FileWriteNCX.WriteLine("<docAuthor>");
			FileWriteNCX.WriteLine("<text>" + HttpUtility.HtmlEncode(book.getAuthor()) + "</text>");
			FileWriteNCX.WriteLine("</docAuthor>");
			FileWriteNCX.WriteLine();
			FileWriteNCX.WriteLine("<navMap>");
			#endregion
			
			#region init of content.opf
			FileWriteOPF.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>");
			FileWriteOPF.WriteLine();
			FileWriteOPF.WriteLine("<!DOCTYPE package");
			FileWriteOPF.WriteLine("  PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
			FileWriteOPF.WriteLine("<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"2.0\" unique-identifier=\"bookid\">");
			FileWriteOPF.WriteLine("<metadata>");
			FileWriteOPF.WriteLine("<dc:identifier xmlns:dc=\"http://purl.org/dc/elements/1.1/\" id=\"bookid\">" + strBookID + "</dc:identifier>");
			FileWriteOPF.WriteLine("<dc:title xmlns:dc=\"http://purl.org/dc/elements/1.1/\">" + HttpUtility.HtmlEncode(book.getTitle()) + "</dc:title>");
			FileWriteOPF.WriteLine("<dc:creator xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:opf=\"http://www.idpf.org/2007/opf\" opf:file-as=\"no name\">" + HttpUtility.HtmlEncode(book.getAuthor()) + "</dc:creator>");
			FileWriteOPF.WriteLine("<dc:date xmlns:dc=\"http://purl.org/dc/elements/1.1/\">2011</dc:date>");
			string subject = "";
			foreach (string folder in book.getFolders()) {
				subject += folder + ",";
			}
			FileWriteOPF.WriteLine("<dc:subject xmlns:dc=\"http://purl.org/dc/elements/1.1/\">" + HttpUtility.HtmlEncode(subject) + "</dc:subject>");
			FileWriteOPF.WriteLine("<dc:publisher xmlns:dc=\"http://purl.org/dc/elements/1.1/\">BookConvertor by Nothingbut</dc:publisher>");
			FileWriteOPF.WriteLine("<dc:description xmlns:dc=\"http://purl.org/dc/elements/1.1/\">" + HttpUtility.HtmlEncode(book.getBrief()) + "</dc:description>");
			FileWriteOPF.WriteLine("<dc:language xmlns:dc=\"http://purl.org/dc/elements/1.1/\">简体中文</dc:language>");
			FileWriteOPF.WriteLine("<meta name=\"cover\" content=\"cover-image\"/>");
			FileWriteOPF.WriteLine("</metadata>");
			FileWriteOPF.WriteLine("<manifest>");
			#endregion
			
			switch (Path.GetExtension(book.getCoverFile()).ToLower()) {
				case ".bmp":
					FileWriteOPF.WriteLine("<item id=\"cover-image\" href=\"{0}\" media-type=\"image/bmp\"/>", book.getCoverFile().ToLower());
					break;
				case ".jpg":
					FileWriteOPF.WriteLine("<item id=\"cover-image\" href=\"{0}\" media-type=\"image/jpeg\"/>", book.getCoverFile().ToLower());
					break;
				case ".png":
					FileWriteOPF.WriteLine("<item id=\"cover-image\" href=\"{0}\" media-type=\"image/png\"/>", book.getCoverFile().ToLower());
					break;
			}

			FileWriteOPF.WriteLine("<item id=\"ncxtoc\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");
			createCoverPage(book);
			FileWriteOPF.WriteLine("<item id=\"coverpage\" href=\"cover.html\" media-type=\"application/xhtml+xml\"/>");

			bool bNeedVolumn = (book.getAllContents().Count > 1);
			
			foreach (BookVolumn vol in book.getAllContents() ) {
				if (bNeedVolumn) {
					vol.setFname(createVolumnPage(book, vol.getID()));
					FileWriteOPF.WriteLine("<item id=\"" + vol.getFname() + "\" href=\"" + vol.getFname() + ".html\" media-type=\"application/xhtml+xml\"/>");
				}
				
				foreach (BookChapter chapter in vol.getChapters() ) {
					string strChapFile = prepareChapter(chapter, book.getPath());
					FileWriteOPF.WriteLine("<item id=\"" + strChapFile + "\" href=\"" + strChapFile + ".html\" media-type=\"application/xhtml+xml\"/>");
					if (copyImageFile(chapter, book.getPath(), CommonUtil.getTempOEBPS()) )	{
						foreach (string imgFile in chapter.getImgList()) {
							string strExt = Path.GetExtension(imgFile).Substring(1);
							if (strExt == "jpg") strExt = "jpeg";
							string strPure = Path.GetFileNameWithoutExtension(imgFile);
							FileWriteOPF.WriteLine("<item id=\"" + strPure + "\" href=\"" + imgFile + "\" media-type=\"image/" + strExt + "\"/>");
						}
					}
					chapter.cleanup();
				}
			}

			FileWriteOPF.WriteLine("</manifest>");
			FileWriteOPF.WriteLine("<spine toc=\"ncxtoc\">");
			FileWriteOPF.WriteLine("<itemref idref=\"coverpage\" linear=\"yes\"/>");
			
			FileWriteNCX.WriteLine("<navPoint id=\"coverpage\" playOrder=\"1\">");
			FileWriteNCX.WriteLine("<navLabel><text>" + HttpUtility.HtmlEncode("封面") + "</text></navLabel>");
			FileWriteNCX.WriteLine("<content src=\"cover.html\"/>");
			FileWriteNCX.WriteLine("</navPoint>");
			
			#region fill volumn and chapter info
			int indexOfAll = 1;
			foreach (BookVolumn vol in book.getAllContents() ) {
				if (bNeedVolumn) {
					indexOfAll ++;
					FileWriteOPF.WriteLine("<itemref idref=\"" + vol.getFname() + "\" linear=\"yes\"/>");
					FileWriteNCX.WriteLine("<navPoint id=\"" + vol.getFname() + "\" playOrder=\"" + indexOfAll.ToString() + "\">");
					FileWriteNCX.WriteLine("<navLabel><text>" + HttpUtility.HtmlEncode(vol.getTitle()) + "</text></navLabel>");
					FileWriteNCX.WriteLine("<content src=\"" + vol.getFname() + ".html\"/>");
				}
				foreach (BookChapter chapter in vol.getChapters() ) {
					indexOfAll ++;
					FileWriteOPF.WriteLine("<itemref idref=\"" + chapter.getId() + "\" linear=\"yes\"/>");
					FileWriteOPF.WriteLine("<reference type=\"text\" title=\"" + HttpUtility.HtmlEncode(chapter.getTitle()) + "\"  href=\"" + chapter.getId() + ".html\"/>");
					FileWriteNCX.WriteLine("<navPoint id=\"" + chapter.getId() + "\" playOrder=\"" + indexOfAll.ToString() + "\">");
					FileWriteNCX.WriteLine("<navLabel><text>" + HttpUtility.HtmlEncode(chapter.getTitle()) + "</text></navLabel>");
					FileWriteNCX.WriteLine("<content src=\"" + chapter.getId() + ".html\"/>");
					FileWriteNCX.WriteLine("</navPoint>");
				}
				if (bNeedVolumn) {
					FileWriteNCX.WriteLine("</navPoint>");
				}
			}
			#endregion
			
			FileWriteOPF.WriteLine("</spine>");
			FileWriteOPF.WriteLine("<guide>");
			FileWriteOPF.WriteLine("<reference type=\"cover\" title=\"封面\"  href=\"cover.html\"/>");
			FileWriteOPF.WriteLine("</guide>");
			FileWriteOPF.WriteLine("</package>");
			FileWriteOPF.Flush();
			FileWriteOPF.Close();
			
			FileWriteNCX.WriteLine("</navMap>");
			FileWriteNCX.WriteLine("</ncx>");
			FileWriteNCX.Flush();
			FileWriteNCX.Close();
		}
		
		private static void prepareMiscFilesforEpub() {
			string sTempDir = CommonUtil.getTempPath();

			
			if (!File.Exists(CommonUtil.getTemplatePath() + "mimetype")) {
				throw new Exception("template file : minetype not found");
			}
			File.Copy(CommonUtil.getTemplatePath() + "mimetype", CommonUtil.getTempPath() +  "mimetype");
			
			if (!File.Exists(CommonUtil.getTemplatePath() + "container.xml")) {
				throw new Exception("template file : container.xml not found");
			}
			File.Copy(CommonUtil.getTemplatePath() + "container.xml", CommonUtil.getTempMETA() + "container.xml");

			if (!File.Exists(CommonUtil.getTemplatePath() + "oxen4kobo.css")) {
				throw new Exception("template file : main.css not found");
			}
			File.Copy(CommonUtil.getTemplatePath() + "oxen4kobo.css", CommonUtil.getTempOEBPS() + "main.css");
		}
		
		private static void packEpub(BookInfo book) {
            new FastZip(new FastZipEvents()).CreateZip(FileUtil.getGenPath(book) + book.getTitle() + ".epub", 
			                                           CommonUtil.getTempPath(), true, null);
			Directory.Delete(CommonUtil.getTempPath(), true);
		}
		
		public static void genEpub(BookInfo book) {
			try {
				prepareDirectory();
				prepareMiscFilesforEpub();
				prepareEpubContent(book);
				packEpub(book);
			}
			catch (Exception ex) {
				LogUtil.Error(book.getTitle() + " is invalid due to : " + ex.ToString());
			}
		}
		#endregion
	}
}

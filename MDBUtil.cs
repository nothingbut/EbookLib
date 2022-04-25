/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2012/12/10
 * Time: 9:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace EbookLib {
	/// <summary>
	/// Description of EbookDB.
	/// </summary>
	public class MDBUtil {
		private string m_strDBFile;
		private string m_strUsedFile;
		private static OleDbConnection m_dbConn;

		public MDBUtil (string dbFileName) {
			m_strDBFile = dbFileName;
			m_strUsedFile = Path.GetDirectoryName(m_strDBFile) + @"\UsedDb" + Path.GetExtension(m_strDBFile);
			m_dbConn = null;
		}

		public static OleDbConnection getDBConn() {
			if (m_dbConn == null)
				throw new Exception("DB connection not initialized");
			return m_dbConn;
		}
		
		private void openDB() {
			if (m_dbConn == null)
				m_dbConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + m_strDBFile);
			m_dbConn.Open();
		}

		private void closeDB() {
			m_dbConn.Close();
		}

		private bool backupFile() {
			if (m_strDBFile == null || m_strUsedFile == null) {
				return false;
			}
			if (!File.Exists(m_strDBFile))
				return false;

			try {
				if (File.Exists(m_strUsedFile)) {
					File.Delete(m_strUsedFile);
				}
				File.Copy(m_strDBFile, m_strUsedFile);
			} catch (Exception ex) {
				System.Console.WriteLine(ex.Message);
				return false;
			}
			return true;
		}

		private void restoreFile() {
			if (m_strDBFile == null || m_strUsedFile == null) {
				return;
			}
			if (!File.Exists(m_strUsedFile))
				return;

			try {
				File.Copy(m_strUsedFile, m_strDBFile);
				File.Delete(m_strUsedFile);
			} catch (Exception ex) {
				System.Console.WriteLine(ex.Message);
				return;
			}
		}

		public bool init() {
			if (m_dbConn != null)
				return true;
			
			if (backupFile() == false)
				return false;
			try {
				openDB();
			} catch (Exception ex) {
				System.Console.WriteLine (ex.Message);
				cleanup();
				return false;
			}
			return true;

		}

		public void cleanup() {
			if (m_dbConn != null) {
				closeDB();
				m_dbConn = null;
			}
			restoreFile();
		}
	}
	
	public class ShelfFromDB {
		const string strBookList = "select NovelID,NovelName,LB from book_novel order by LB,NovelName";
		const string strCatagory = "select DM,MC,TopDM from dic_noveltype order by DM";
		
		public static Hashtable getAllBookInfo(BookShelf shelf) {
			Hashtable allBookInfo = new Hashtable();
			OleDbConnection dbConn = MDBUtil.getDBConn();
			DataSet dsBookList = new DataSet();
			try {
				OleDbDataAdapter adapter = new OleDbDataAdapter(strBookList, dbConn);
				adapter.Fill(dsBookList);
			} catch (Exception ex) {
				System.Console.WriteLine(ex.Message);
				return allBookInfo;
			}
			for (int index = 0; index < dsBookList.Tables[0].Rows.Count; index ++) {
				BookInfo book = new BookInfo(dsBookList.Tables[0].Rows[index][0].ToString().Trim(),
				                             dsBookList.Tables[0].Rows[index][1].ToString().Trim(),
				                             shelf, 
				                             dsBookList.Tables[0].Rows[index][2].ToString().Trim());
				allBookInfo[book.getID()] = book;
			}
			return allBookInfo;
		}
		
		public static Hashtable getAllCatagories(BookShelf shelf) {
			Hashtable allCatagories = new Hashtable();
			OleDbConnection dbConn = MDBUtil.getDBConn();
			DataSet dsCataList = new DataSet();
			try {
				OleDbDataAdapter adapter = new OleDbDataAdapter(strCatagory, dbConn);
				adapter.Fill(dsCataList);
			} catch (Exception ex) {
				System.Console.WriteLine(ex.Message);
				return allCatagories;
			}
			for (int index = 0; index < dsCataList.Tables[0].Rows.Count; index ++) {
				BookCatagory catagory = new BookCatagory(dsCataList.Tables[0].Rows[index][0].ToString().Trim(),
				                             dsCataList.Tables[0].Rows[index][1].ToString().Trim(),
				                             shelf,
				                             dsCataList.Tables[0].Rows[index][2].ToString().Trim());
				allCatagories[catagory.getID()] = catagory;
			}
			return allCatagories;
		}
	}
	
	public class BookFromDB {
		const string strBookInfo = "select Author, ListUrl, NeedDelUrl, Brief, BookImg from book_novel where NovelID = '$1'";

		public static Hashtable getBookDetail(string bookID) {
			Hashtable retList = new Hashtable();
			OleDbConnection dbConn = MDBUtil.getDBConn();
			DataSet dsBookInfo = new DataSet();
			
			OleDbDataAdapter adapter = new OleDbDataAdapter(strBookInfo.Replace("$1", bookID), dbConn);
			adapter.Fill(dsBookInfo);
			
			if (dsBookInfo.Tables[0].Rows.Count != 1)
				throw new Exception("Invalid book ID as " + bookID);
			
			retList["Author"] = dsBookInfo.Tables[0].Rows[0][0].ToString().Trim();
			retList["ListUrl"] = dsBookInfo.Tables[0].Rows[0][1].ToString().Trim();
			retList["NeedDelUrl"] = dsBookInfo.Tables[0].Rows[0][2].ToString().Trim();
			retList["Brief"] = dsBookInfo.Tables[0].Rows[0][3].ToString().Trim();
			retList["BookImg"] = dsBookInfo.Tables[0].Rows[0][4].ToString().Trim();
			
			return retList;
		}
	}
	
	public class ChaptFromDB {
		const string strChapterList = "select id, Title ,Volume, ComeFrom from book_NovelContent where NovelID = '$1' order by Displayorder";
		
		public static List<BookChapter> getChapterList(string bookID) {
			List<BookChapter> listChapter = new List<BookChapter>();
			
			OleDbConnection dbConn = MDBUtil.getDBConn();
			DataSet dsChapter = new DataSet();
			
			OleDbDataAdapter adapter = new OleDbDataAdapter(strChapterList.Replace("$1", bookID), dbConn);
			adapter.Fill(dsChapter);
			
			for (int index = 0; index < dsChapter.Tables[0].Rows.Count; index ++) {
				listChapter.Add(new BookChapter(dsChapter.Tables[0].Rows[index][0].ToString().Trim(),
				                                dsChapter.Tables[0].Rows[index][1].ToString().Trim(),
				                                dsChapter.Tables[0].Rows[index][2].ToString().Trim(),
				                                dsChapter.Tables[0].Rows[index][3].ToString().Trim()) );
			}
			return listChapter;
		}
	}
}

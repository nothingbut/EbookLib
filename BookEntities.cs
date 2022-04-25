/*
 * 由SharpDevelop创建。
 * 用户： chshi
 * 日期: 2012/12/10
 * 时间: 13:02
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EbookLib {
	/// <summary>
	/// Description of BookEntities.
	/// </summary>
	/// 
	public abstract class BookEntity {
		private string id;
		private string title;
		private string pid;
		private BookShelf shelf;
		private List<BookEntity> entityList;
		
		public BookEntity(string id, string title, BookShelf shelf, string pid = null) {
			this.id = id;
			this.title = title;
			this.pid = pid;
			this.shelf = shelf;
			this.entityList = new List<BookEntity>();
		}

		public string getPID() {
			return pid;
		}
		
		public string getID() {
			return id;
		}
		
		public string getTitle() {
			return title;
		}

		public string getGraceTitle() {
			return title.Replace("-", "");
		}
		public BookShelf getShelf() {
			return shelf;
		}
		
		public BookEntity getParentEntity() {
			return shelf.getByID(pid);
		}
		/*		
				public string getPName() {
					BookEntity entity = getParentEntity();
					if (entity == null)
						return "empty";
					return entity.getTitle();
				}
		*/
		public Stack<string> getFolders() {
			Stack<string> pathStack = new Stack<string>();
			BookEntity parent = getParentEntity();
			while (parent != null) {
				pathStack.Push(parent.getGraceTitle());
				parent = parent.getParentEntity();
			}

			return pathStack;
		}

		public void addEntityToList(BookEntity entity) {
			entityList.Add(entity);
		}
		
		public List<BookEntity> getEntityList()	{
			return entityList;
		}
		
		public string dump() {
			string buffer = toString() + "\r\n";
			foreach (BookEntity entity in entityList) {
				buffer += entity.dump();
			}
			return buffer;
		}
		
		public void processBooks(BookFuncDelegate func) {
			foreach (BookEntity entity in entityList) {
				if (entity.GetType().ToString() == "EbookLib.BookInfo")
					func((BookInfo)entity);
				else
					entity.processBooks(func);
			}
		}
		
		public List<BookInfo> pickBooks(BookPickDelegate pick, Hashtable parameters) {
			List<BookInfo> bookList = new List<BookInfo>();
			foreach (BookEntity entity in entityList) {
				if (entity.GetType().ToString() == "EbookLib.BookInfo") {
					BookInfo book = (BookInfo)entity;
					if (pick(book, parameters))
					    bookList.Add(book);
				} else {
					foreach (BookInfo book in entity.pickBooks(pick, parameters))
						bookList.Add(book);
				}
			}
			return bookList;
		}
		
		public BookEntity pickCatagory(string title) {
			if (this.title == title)
				return this;
			if (entityList == null)
				return null;
			foreach (BookEntity entity in entityList) {
				if (entity is BookCatagory) {
					BookEntity result = entity.pickCatagory(title);
					if (result != null)
						return result;
				}
			}
			return null;
		}
		
		public abstract string toString();
	}
	
	public class BookShelf {
		private RootCatagory shelf;
		private Hashtable entityMap;
			
		public BookShelf() {
			shelf = null;
			entityMap = new Hashtable();
		}
		
		public void init() {
			if (shelf != null)
				return;
			load();
		}
		
		private void load() {
			shelf = new RootCatagory("root", this);

			Hashtable books = ShelfFromDB.getAllBookInfo(this);
			Hashtable catagories = ShelfFromDB.getAllCatagories(this);
			
			foreach (DictionaryEntry de in catagories) {
				BookCatagory catagory = (BookCatagory)de.Value;
				if (catagory.getPID() == null || catagory.getPID().Equals(""))
					shelf.addEntityToList(catagory);
				else {
					BookCatagory parent = (BookCatagory)catagories[catagory.getPID()];
					parent.addEntityToList(catagory);
				}
				entityMap[catagory.getID()] = catagory;
			}
			
			foreach (DictionaryEntry bEntity in books) {
				BookInfo book = (BookInfo)bEntity.Value;
				book.init();
				if (book.getPID() == null || book.getPID().Equals(""))
					shelf.addEntityToList(book);
				else {
					BookCatagory parent = (BookCatagory)catagories[book.getPID()];
					parent.addEntityToList(book);
				}
				entityMap[book.getID()] = book;
			}
		}
		
		public RootCatagory getShelf() {
			return shelf;
		}
		
		public BookEntity getByID(string id) {
			if (!entityMap.ContainsKey(id))
				return null;
			return (BookEntity)entityMap[id];
		}
		
		public string dump() {
			string buffer = shelf.dump();
			return buffer;
		}

		public void processBooks(BookFuncDelegate func, string catagoryName = "root") {
			if (catagoryName == "root") {
				shelf.processBooks(func);
				return;
			}
			
			foreach (BookEntity entity in entityMap.Values) {
				if (entity.getTitle() == catagoryName) {
					entity.processBooks(func);
					return;
				}
			}
		}
		
		public List<BookInfo> pickBooks(BookPickDelegate pick, Hashtable parameters, string catagoryName = "root") {
			BookEntity entity = shelf.pickCatagory(catagoryName);
			if (entity == null)
				return null;
			return entity.pickBooks(pick, parameters);
		}
		
	}
	
	public class BookCatagory : BookEntity
	{
		public BookCatagory(string id, string title, BookShelf shelf, string pid) : base(id, title, shelf, pid) {
		}

		public override string toString() {
			return getTitle().Replace("-","");
		}
	}
	
	public class RootCatagory : BookCatagory {
		bool isInitialized = false;
		public RootCatagory(string id, BookShelf shelf) : base(id, "root", shelf, null) {
			System.Diagnostics.Debug.Assert(!isInitialized);
			isInitialized = true;
		}
	}
	
	public class BookInfo : BookEntity {
		private string url { get; set; }
		private string author { get; set; }
		private string brief { get; set; }
		private string path { get; set; }
		private string coverFile { get; set; }
		
		private List<BookVolumn> volumnList;
		
		public BookInfo(string id, string title, BookShelf shelf, string pid) : base(id, title, shelf, pid) {
			this.path = CommonUtil.getRootPath() + @"\chm\" + id.ToString().PadLeft(6, '0') + @"\";
			volumnList = null;
		}
		
		public void init() {
			Hashtable bookDetail = null;
			try {
				bookDetail = BookFromDB.getBookDetail(getID());
			} catch (Exception ex) {
				System.Console.WriteLine(ex.Message);
			}
			this.url = bookDetail["ListUrl"].ToString();
			this.author = bookDetail["Author"].ToString();
			this.brief = bookDetail["Brief"].ToString();
			this.coverFile = bookDetail["BookImg"].ToString();
		}
		
		private void loadContent() {
			volumnList = new List<BookVolumn>();
			List<BookChapter> chapters = ChaptFromDB.getChapterList(getID());
			string strVolumnName = null;
			BookVolumn volumn = null;
			foreach (BookChapter chapter in chapters) {
				if (chapter.getVolumn() != strVolumnName) {
					strVolumnName = chapter.getVolumn();
					volumn = new BookVolumn(volumnList.Count, strVolumnName);
					volumnList.Add(volumn);
				}
				volumn.addChapter(chapter);
			}
		}
		
		public void initContent() {
			if (volumnList == null)
				loadContent();
		}
		
		public string dumpContent() {
			string buffer = "";
			foreach (BookVolumn volumn in volumnList) {
				buffer += volumn.getTitle() + "\r\n";
				foreach (BookChapter chapter in volumn.getChapters()) {
					buffer += "\t" + chapter.getTitle() + "\r\n";
				}
			}
			return buffer;
		}
		
		public void exportInfo(string filename) {
		}
		
		public List<BookVolumn> getAllContents() {
			return volumnList;
		}
		
		public string getUrl() {
			return url;
		}
		
		public string getAuthor() {
			return author;
		}
		
		public string getBrief() {
			return brief;
		}
		
		public string getPath() {
			return path;
		}
		
		public string getCoverFile() {
			return coverFile;
		}
		
		public override string toString() {
			return getTitle();
		}
	}
	
	public class BookVolumn {
		private int id;
		private string title;
		private string fname;
		private List<BookChapter> chapterList;
		
		public BookVolumn(int id, string title) {
			this.id = id;
			this.title = title;
			chapterList = new List<BookChapter>();
		}
		
		public void addChapter(BookChapter chapter) {
			chapterList.Add(chapter);
		}
		
		public int getID() {
			return id;
		}
		
		public string getTitle() {
			return title;
		}
		
		public string getFname() {
			return fname;
		}
		
		public void setFname(string fname) {
			this.fname = fname;
		}
		
		public List<BookChapter> getChapters() {
			return chapterList;
		}
	}
	
	public class BookChapter {
		private string id;
		private string title;
		private string volumn;
		private string url;
		private string content;
		private List<string> imgList;
		private long size;
		
		public BookChapter(string id, string title, string volumn, string url = "") {
			this.id = id;
			this.title = title;
			this.volumn = volumn;
			this.url = url;
			this.content = null;
			this.imgList = null;
			this.size = 0;
		}
		
		public bool loadChapter(string bookPath, bool bForced = false) {
			if (size > 0 && bForced == false)
				return withImg();
			string filename = bookPath + id + ".htm";
			if (!File.Exists(filename))
				return false;
			content = TextUtil.getContent(File.ReadAllText(filename, Encoding.Default));
			imgList = TextUtil.getImgList(content);
			size = content.Length;
			return withImg();
		}
		
		public void cleanup() {
			content = null;
			imgList = null;
			size = 0;
		}
		
		public string getId() {
			return id;
		}
		
		public string getTitle() {
			return title;
		}
		
		public string getVolumn() {
			return volumn;
		}
		
		public string getUrl() {
			return url;
		}
		
		public string getContent() {
			return content;
		}
		
		public List<string> getImgList() {
			return imgList;
		}
		
		public long getSize() {
			return size;
		}
		
		public bool withImg() {
			if (imgList.Count > 0)
				return true;
			return false;
		}
	}
}

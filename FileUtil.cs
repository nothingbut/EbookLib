/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2013/1/30
 * Time: 13:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EbookLib {
	/// <summary>
	/// Description of FileUtil.
	/// </summary>
	public class FileUtil {
		public static string getGenPath(BookInfo book) {
			string genPath = CommonUtil.getGenPath();
			if (!Directory.Exists(genPath))
				Directory.CreateDirectory(genPath);

			BookEntity entity = book;
			Stack<string> pathStack = new Stack<string>();
			do {
				if ((entity = entity.getParentEntity()) == null)
					break;
				pathStack.Push(entity.getGraceTitle());
			} while (true);
			
			while (pathStack.Count > 0) {
				genPath = genPath + pathStack.Pop() + @"\";
				if (!Directory.Exists(genPath))
					Directory.CreateDirectory(genPath);
			}
			
			return genPath;
		}
	}
}

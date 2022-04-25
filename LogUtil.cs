/*
 * Created by SharpDevelop.
 * User: chshi
 * Date: 2012/12/14
 * Time: 9:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;

namespace EbookLib {
	/// <summary>
	/// Description of LogUtil.
	/// </summary>
	public class LogUtil {
		public static void Cleanup() {
			if (File.Exists(CommonUtil.getLogFile()))
				File.Delete(CommonUtil.getLogFile());
		}
		
		public static void None(string msg) {
			if (CommonUtil.logLevel() <= CommonUtil.LOG_LEVEL_INFO)
				File.AppendAllText(CommonUtil.getLogFile(), msg + "\r\n");
		}
		
		public static void Info(string msg) {
			if (CommonUtil.logLevel() <= CommonUtil.LOG_LEVEL_INFO)
				File.AppendAllText(CommonUtil.getLogFile(), "INFO: " + msg + "\r\n");
		}
		
		public static void Warn(string msg) {
			if (CommonUtil.logLevel() <= CommonUtil.LOG_LEVEL_WARN)
				File.AppendAllText(CommonUtil.getLogFile(), "WARN: " + msg + "\r\n");
		}
		
		public static void Error(string msg) {
			if (CommonUtil.logLevel() <= CommonUtil.LOG_LEVEL_ERROR)
				File.AppendAllText(CommonUtil.getLogFile(), "ERROR: " + msg + "\r\n");
		}
		
		public static void Fatal(string msg) {
			if (CommonUtil.logLevel() <= CommonUtil.LOG_LEVEL_FATAL)
				File.AppendAllText(CommonUtil.getLogFile(), "FATAL: " + msg + "\r\n");
		}
	}
}

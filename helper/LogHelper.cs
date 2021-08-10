using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Utils.helper
{
    public static class LogHelper
    {
        /// <summary>
        /// 写线程
        /// </summary>
        private static readonly Thread WriteThread;
        /// <summary>
        /// 信息队列
        /// </summary>
        private static readonly Queue<string> MsgQueue;
        /// <summary>
        /// Debug信息队列
        /// </summary>
        private static readonly Queue<string> DebugQueue = new Queue<string>();
        /// <summary>
        /// Error消息队列
        /// </summary>
        private static readonly Queue<string> ErrorQueue = new Queue<string>();
        /// <summary>
        /// 对象锁
        /// </summary>
        private static readonly object FileLock;
        /// <summary>
        /// 调试日志锁
        /// </summary>
        private static readonly object DebugLock;
        /// <summary>
        /// Trace日志锁
        /// </summary>
        private static readonly object TraceLock;
        /// <summary>
        /// 错误日志锁
        /// </summary>
        private static readonly object ErrorLock;
        /// <summary>
        /// 写debug文件线程
        /// </summary>
        private static Thread WriteDebugThread;
        /// <summary>
        /// 编写错误信息线程
        /// </summary>
        private static Thread WriteErrorThread;
        /// <summary>
        /// log文件路径
        /// </summary>
        private static readonly string LogFilePath;
        /// <summary>
        /// degub文件路径
        /// </summary>
        private static readonly string DebugFilePath;
        /// <summary>
        /// Error文件路径
        /// </summary>
        private static readonly string ErrorFilePath;
        /// <summary>
        /// 构造函数
        /// </summary>
        static LogHelper()
        {
            FileLock = new object();
            DebugLock = new object();
            TraceLock = new object();
            ErrorLock = new object();
            MsgQueue = new Queue<string>();
            WriteThread = new Thread(WriteMsg);
            WriteThread.IsBackground = true;
            WriteThread.Start();
            WriteDebugThread = new Thread(WriteDebug);
            WriteDebugThread.Start();
            WriteErrorThread = new Thread(WriteError);
            WriteErrorThread.Start();
            LogFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log\\";
            DebugFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "debug\\";
            ErrorFilePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "error\\";
        }
        /// <summary>
        /// 将Log类型文件写入日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteLog(string msg)
        {
            Monitor.Enter(MsgQueue);
            MsgQueue.Enqueue(string.Format("{0} {1} {2}", "[Info]", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss:fff") + "]", msg));
            Monitor.Exit(MsgQueue);
        }
        /// <summary>
        /// 写Debug类型日志
        /// </summary>
        /// <param name="msg"></param>
        public static void WriteDebug(string msg)
        {
            Monitor.Enter(DebugQueue);
            MsgQueue.Enqueue(string.Format("{0} {1} {2}", "[debug]", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss:fff") + "]", msg));
            Monitor.Exit(DebugQueue);
        }
        /// <summary>
        /// 写入Error类型日志
        /// </summary>
        /// <param name="error"></param>
        public static void WriteError(string error)
        {
            Monitor.Enter(ErrorQueue);
            MsgQueue.Enqueue(string.Format("{0} {1} {2}", "[error]", "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:sss:fff") + "]", error));
            Monitor.Exit(ErrorQueue);
        }
        /// <summary>
        /// 循环写入Log类型日志
        /// </summary>
        private static void WriteMsg()
        {
            while (true)
            {
                if (MsgQueue.Count > 0)
                {
                    Monitor.Enter(MsgQueue);
                    string msg = MsgQueue.Dequeue();
                    Monitor.Exit(MsgQueue);

                    Monitor.Enter(FileLock);
                    if (!Directory.Exists(LogFilePath))
                    {
                        Directory.CreateDirectory(LogFilePath);
                    }
                    string fileName = LogFilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    var logStreamWriter = new StreamWriter(fileName, true);
                    logStreamWriter.WriteLine(msg);
                    logStreamWriter.Close();
                    Monitor.Exit(FileLock);
                    if (GetFileSize(fileName) > 1024 * 5)
                    {
                        CopyToBak(fileName);
                    }
                }
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 循环写入Debug类型日志
        /// </summary>
        private static void WriteDebug()
        {
            while (true)
            {
                if (DebugQueue.Count > 0)
                {
                    Monitor.Enter(DebugQueue);
                    string msg = DebugQueue.Dequeue();
                    Monitor.Exit(DebugQueue);

                    Monitor.Enter(DebugLock);
                    if (!Directory.Exists(DebugFilePath))
                    {
                        Directory.CreateDirectory(DebugFilePath);
                    }
                    string fileName = DebugFilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    var logStreamWriter = new StreamWriter(fileName, true);

                    logStreamWriter.WriteLine(msg);
                    logStreamWriter.Close();
                    Monitor.Exit(DebugLock);
                    if (GetFileSize(fileName) > 1024 * 5)
                    {
                        CopyToBak(fileName);
                    }
                }
                Thread.Sleep(1);
            }
        }
        /// <summary>
        /// 循环写入Error类型日志
        /// </summary>
        private static void WriteError()
        {
            while (true)
            {
                if (ErrorQueue.Count > 0)
                {
                    Monitor.Enter(ErrorQueue);
                    string msg = ErrorQueue.Dequeue();
                    Monitor.Exit(ErrorQueue);
                    Monitor.Enter(ErrorLock);
                    if (!Directory.Exists(ErrorFilePath))
                    {
                        Directory.CreateDirectory(ErrorFilePath);
                    }
                    string fileName = ErrorFilePath + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
                    var logStreamWriter = new StreamWriter(fileName, true);

                    logStreamWriter.WriteLine(msg);
                    logStreamWriter.Close();
                    Monitor.Exit(ErrorLock);
                    if (GetFileSize(fileName) > 1024 * 5)
                    {
                        CopyToBak(fileName);
                    }
                }
                Thread.Sleep(1);
            }
        }
        private static long GetFileSize(string fileName)
        {
            long strRe = 0;
            if (File.Exists(fileName))
            {
                Monitor.Enter(FileLock);
                var myFs = new FileStream(fileName, FileMode.Open);
                strRe = myFs.Length / 1024;
                myFs.Close();
                myFs.Dispose();
                Monitor.Exit(FileLock);
            }
            return strRe;
        }
        
        private static void CopyToBak(string sFileName)
        {
            int fileCount = 0;
            string sBakName = "";
            Monitor.Enter(FileLock);
            do
            {
                fileCount++;
                sBakName = sFileName + "." + fileCount + ".BAK";
            }
            while (File.Exists(sBakName));

            File.Copy(sFileName, sBakName);
            File.Delete(sFileName);
            Monitor.Exit(FileLock);
        }
    }
}

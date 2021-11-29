using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UtilsHelper.helper
{
    /*
            服务端使用方法：
            1.全局申明对象 public ServerSocket ss;
            2.启动监听：
                ss = new ServerSocket();
                ss.RciEvent += new EventHandler<ReceiveClientInfo>(ReceiveDataEvent);
                ss.ServerStart("192.168.2.123", 1478);
            3.定义接收数据事件
            public void ReceiveDataEvent(object sender, ReceiveClientInfo rci)
            {
                Socket clientSocket = rci.clientSocket;
                string receiveData = rci.receiveData;  
                if (receiveData != "")
                {
                    if (receiveData == "112233445566")
                    {
                        ss.ServerSend(clientSocket, textBox1.Text.Trim());
                    }
                }
            }
            4.停止监听：ss.ServerStop();
    */
    public class ServerSocket
    {
        public Socket serverSocket;
        public Thread acceptThread;
        public Thread disConnectThread;
        public Dictionary<IntPtr, Socket> sockets = new Dictionary<IntPtr, Socket>();
        public Dictionary<IntPtr, Thread> clientsThread = new Dictionary<IntPtr, Thread>();
        public event EventHandler<ReceiveClientInfo> RciEvent;
        public ServerSocket()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void ServerStart(string host, int port, int timeout = 3)
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint point = new IPEndPoint(ip, port);

            serverSocket.Bind(point);
            serverSocket.ReceiveTimeout = timeout * 1000;
            serverSocket.Listen(100);

            acceptThread = new Thread(ClientAccept);
            acceptThread.IsBackground = true;
            acceptThread.Start();

            disConnectThread = new Thread(ClientDisConnection);
            disConnectThread.IsBackground = true;
            disConnectThread.Start();
        }
        public void ServerStop()
        {
            if (serverSocket != null)
            {
                serverSocket.Dispose();
                serverSocket.Close();

                disConnectThread.Abort();
                foreach (Thread t in clientsThread.Values)
                {
                    t.Abort();
                }
                acceptThread.Abort();
            }
        }
        public void ClientAccept()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = serverSocket.Accept();
                    if (!sockets.ContainsKey(clientSocket.Handle))
                    {
                        sockets.Add(clientSocket.Handle, clientSocket);

                        Thread ct = new Thread(ReceiveClientDataHandler);
                        ct.IsBackground = true;
                        ct.Start(clientSocket);
                        clientsThread.Add(clientSocket.Handle, ct);
                    }
                }
                catch
                {
                    serverSocket.Close();
                }
                Thread.Sleep(100);
            }
        }
        public void ReceiveClientDataHandler(object cs)
        {
            Socket clientSocket = cs as Socket;
            while (true)
            {
                string receiveData;
                try
                {
                    receiveData = ServerReceive(clientSocket);
                }
                catch (Exception)
                {
                    receiveData = "";
                }
                ReceiveClientInfo rci = new ReceiveClientInfo();
                rci.clientSocket = clientSocket;
                rci.receiveData = receiveData;
                // 暴露出去的方法
                if (RciEvent != null)
                {
                    RciEvent(this, rci);
                }
                Thread.Sleep(100);
            }
        }
        public string ServerReceive(Socket clientSocket)
        {
            string receiveStr = string.Empty;
            byte[] recvBytes = new byte[1024];
            int bytesRec;
            try
            {
                bytesRec = clientSocket.Receive(recvBytes, recvBytes.Length, 0);
            }
            catch (Exception)
            {
                bytesRec = 0;
            }
            for (int i = 0; i < bytesRec; i++)
            {
                receiveStr += Convert.ToString(recvBytes[i], 16);
            }
            return receiveStr;
        }
        public void ServerSend(Socket clientSocket, string sendStr)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < sendStr.Length / 2; i++)
            {
                list.Add(Convert.ToByte(sendStr.Substring(i * 2, 2), 16));
            }
            int bytesSent = clientSocket.Send(list.ToArray());
        }
        public void ClientDisConnection()
        {
            while (true)
            {
                try
                {
                    if (sockets.Count > 0)
                    {
                        foreach (IntPtr key in sockets.Keys)
                        {
                            if (sockets[key].Poll(1000, SelectMode.SelectRead))
                            {
                                sockets[key].Shutdown(SocketShutdown.Both);
                                sockets[key].Close();
                                sockets.Remove(key);

                                clientsThread[key].Abort();
                                clientsThread.Remove(key);
                                break;
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(100);
            }
        }
    }

    public class ReceiveClientInfo : EventArgs
    {
        public Socket clientSocket;
        public string receiveData;
    }
}

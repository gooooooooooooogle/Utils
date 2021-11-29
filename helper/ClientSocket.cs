using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UtilsHelper.helper
{
    public class ClientSocket
    {
        public Socket clientSocket = null;
        public ClientSocket()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void ClientConnect(Socket clientSocket, string host, int port, int timeout = 2)
        {
            IPAddress ip = IPAddress.Parse(host);
            clientSocket.ReceiveTimeout = timeout * 1000;
            clientSocket.Connect(ip, port);
        }
        public void ClientSend(Socket clientSocket, string sendStr)
        {
            List<byte> list = new List<byte>();
            for (int i = 0; i < sendStr.Length / 2; i++)
            {
                list.Add(Convert.ToByte(sendStr.Substring(i * 2, 2), 16));
            }
            int bytesSent = clientSocket.Send(list.ToArray());
        }
        public string ClientReceive(Socket clientSocket)
        {
            string receiveStr = string.Empty;
            int bytesRec;
            byte[] bytes = new byte[1024];
            try
            {
                bytesRec = clientSocket.Receive(bytes);
            }
            catch (Exception)
            {
                bytesRec = 0;
            }

            for (int i = 0; i < bytesRec; i++)
            {
                receiveStr += Convert.ToString(bytes[i], 16);
            }
            return receiveStr;
        }
        public void ClientDisConnect(Socket clientSocket)
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
    }
}

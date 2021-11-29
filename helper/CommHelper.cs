using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace Utils.helper
{
    public class CommHelper
    {
        /// <summary>
        /// 串口发送方法
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sendFrame"></param>
        public static void Send(SerialPort sp, string sendFrame)
        {
            SerialPort serialPort = sp;
            if (serialPort != null && !serialPort.IsOpen) serialPort.Open();

            try
            {
                serialPort.DiscardInBuffer();
                if ((sendFrame.Length % 2) == 0)
                {
                    int sendLen = sendFrame.Length / 2;
                    byte[] buffer = new byte[sendLen];
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        buffer[i] = Convert.ToByte(sendFrame.Substring(i * 2, 2), 16);
                    }
                    serialPort.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    //MessageBox.Show($"{sendFrame}发送报文长度异常！");
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 串口接收方法
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="switchType"></param>
        /// <returns></returns>
        public static string Receive(SerialPort sp, string switchType = "close")
        {
            Thread.Sleep(120);
            SerialPort serialPort = sp;
            try
            {
                int byteNum = serialPort.BytesToRead;
                byte[] buffer = new byte[byteNum];
                serialPort.Read(buffer, 0, byteNum);

                string receiveFrame = string.Empty;
                for (int i = 0; i < buffer.Length; i++)
                {
                    receiveFrame += Convert.ToString(buffer[i], 16).PadLeft(2, '0');
                }

                return receiveFrame.ToUpper();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (switchType == "close")
                {
                    serialPort.Close();
                }
            }
        }

        /// <summary>
        /// 串口发送和接收方法
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sendFrame"></param>
        /// <returns></returns>
        public static string SendAndReceive(SerialPort sp, string sendFrame)
        {
            Send(sp, sendFrame);
            string receiveStr = Receive(sp);
            return receiveStr;
        }

        /// <summary>
        /// 串口通讯 发送
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="sendFrame"></param>
        public static void PortSend(SerialPort sp, string sendFrame)
        {
            try
            {
                SerialPort serialPort = sp;
                if (serialPort != null && !serialPort.IsOpen)
                    serialPort.Open();
                serialPort.DiscardInBuffer();
                List<byte> buf = new List<byte>();
                for (int i = 0; i < sendFrame.Length; i = i + 2)
                {
                    buf.Add(byte.Parse(sendFrame.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
                }
                Thread.Sleep(100);
                serialPort.Write(buf.ToArray(), 0, buf.Count);
            }
            catch {}
            finally { }
        }

        /// <summary>
        /// 串口通讯 接收
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string PortReceive(SerialPort sp, string type = "close", int SleepTime = 200)
        {
            Thread.Sleep(SleepTime);
            string receiveFrame = string.Empty;
            try
            {
                SerialPort serialPort = sp;
                int n = serialPort.BytesToRead;
                byte[] buff = new byte[n];

                serialPort.Read(buff, 0, n);
                foreach (byte b in buff)
                {
                    receiveFrame = receiveFrame + b.ToString("X2");
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                receiveFrame = e.Message;
            }
            finally
            {
                if (type == "close")
                {
                    sp.Close();
                }
            }
            return receiveFrame.ToUpper();
        }

        /// <summary>
        /// 快速设置SerialPort 对象属性
        /// </summary>
        /// <param name="port"></param>
        /// <param name="buadRate"></param>
        /// <param name="parity"></param>
        /// <returns></returns>
        public static SerialPort SetSerialPortParam(SerialPort sp, string port, int buadRate, string parity)
        {
            //SerialPort sp = new SerialPort();
            sp.PortName = port;
            sp.BaudRate = buadRate;
            Parity parity485 = Parity.Even;
            switch (parity)
            {
                case "奇":
                    parity485 = Parity.Odd; break;
                case "偶":
                    parity485 = Parity.Even; break;
                case "无":
                    parity485 = Parity.None; break;
                default:
                    break;
            }
            sp.Parity = parity485;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.ReadTimeout = 4000;
            sp.RtsEnable = true;
            return sp;
        }

        /// <summary>
        /// 645报文
        /// </summary>
        /// <param name="meterAddr"></param>
        /// <param name="control"></param>
        /// <param name="DI"></param>
        /// <param name="pass"></param>
        /// <param name="oper"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Get645Frame(string meterAddr, string control, string DI, string pass, string oper, string data)
        {
            string headStr = "68";
            string endStr = "16";
            string dataArea = Util.ReverseStr(DI) + pass + Util.ReverseStr(oper) + Util.ReverseStr(data);
            dataArea = Util.Add33(dataArea);
            string dataLen = Util.IntToHex(dataArea.Length / 2, 2);
            string sendFrame = headStr + Util.ReverseStr(meterAddr) + headStr + control + dataLen + dataArea;
            string cs = Util.GetSumCheck(sendFrame);

            sendFrame += cs + endStr;
            return sendFrame.ToUpper();
        }

        /// <summary>
        /// 检查645报文合法性
        /// </summary>
        /// <param name="sendFrame"></param>
        /// <param name="receiveFrame"></param>
        /// <returns></returns>
        public  static Dictionary<string, string> Check645Legality(string sendFrame, string receiveFrame)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (receiveFrame.Trim().Length > 0)
            {
                string headStr = "68";
                string endStr = "16";

                // 去除发送和返回报文的前导符
                while (sendFrame.Substring(0, 2) == "FE")
                {
                    sendFrame = sendFrame.Remove(0, 2);
                }
                while (receiveFrame.Substring(0, 2) == "FE")
                {
                    receiveFrame = receiveFrame.Remove(0, 2);
                }

                // 验证控制码。
                string sendControl = sendFrame.Substring(16, 2); 
                string receiveControl = receiveFrame.Substring(16, 2);

                string newReceiveControl = Util.HexToBin(sendControl, 8);
                newReceiveControl = "1" + newReceiveControl.Substring(1, 7);
                newReceiveControl = Util.BinToInt(newReceiveControl).ToString();
                newReceiveControl = Util.IntToHex(Convert.ToInt32(newReceiveControl), 2);

                // 验证数据长度
                string frameLen = receiveFrame.Substring(18, 2); 
                string newLen = (Util.HexToInt(frameLen) + 9 + 1 + 2).ToString();

                // 验证校验
                string frameCheckSum = receiveFrame.Substring(receiveFrame.Length-4, 2);
                string newCheckSum = Util.GetSumCheck(receiveFrame.Substring(0, receiveFrame.Length-4));

                if (receiveFrame.Substring(0, 2) != headStr)
                {
                    dic.Add("result", "false");
                    dic.Add("message", "帧头异常");
                }
                else if (receiveFrame.Substring(receiveFrame.Length - 2, 2) != endStr)
                {
                    dic.Add("result", "false");
                    dic.Add("message", "帧尾异常");
                }
                else if (receiveControl.ToUpper() != newReceiveControl.ToUpper())
                {
                    dic.Add("result", "false");
                    dic.Add("message", "控制码不是预期的正常控制码");
                }
                else if (Convert.ToInt32(newLen) * 2 != receiveFrame.Length)
                {
                    dic.Add("result", "false");
                    dic.Add("message", "数据长度异常");
                }
                else if (frameCheckSum.ToUpper() != newCheckSum.ToUpper())
                {
                    dic.Add("result", "false");
                    dic.Add("message", "校验异常");
                }
                else
                {
                    dic.Add("result", "true");
                    dic.Add("message", "正常");
                }
            }
            else
            {
                dic.Add("result", "false");
                dic.Add("message", "未收到返回数据");
            }

            return dic;
        }

        /// <summary>
        /// 获取645报文的数据域数据（DI + data）
        /// </summary>
        /// <param name="receiveStr"></param>
        /// <returns></returns>
        public static string Get645DataArea(string receiveStr)
        {
            string dataArea = "";
            if (receiveStr.Trim().Length > 0)
            {
                while (receiveStr.Substring(0, 2) == "FE")
                {
                    receiveStr = receiveStr.Remove(0, 2);
                }

                string dataLen = receiveStr.Substring(18, 2);

                dataArea = receiveStr.Substring(20, Util.HexToInt(dataLen) * 2);
                dataArea = Util.Less33(dataArea);
            }
            return dataArea.ToUpper();
        }
    }
}

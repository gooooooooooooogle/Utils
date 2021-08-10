using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Utils.Enum;

namespace Utils.helper
{
    public class Util
    {
        /// <summary>
        /// 跨线程操作UI
        /// </summary>
        /// <param name="control"></param>
        /// <param name="val"></param>
        /// <param name="type">1：TextBox；2：RIchTextBox；3：ComboBox</param>
        /// <param name="color"></param>
        public static void SetUIVal(object control, string val, ControlType type, Color? color = null)
        {
            Control obj = null;
            switch (type)
            {
                case ControlType.TextBox: // TextBox
                    obj = control as TextBox;
                    break;
                case ControlType.RichTextBox: // RichTextBox
                    obj = control as RichTextBox;
                    break;
                case ControlType.ComboBox: // ComboBox
                    obj = control as ComboBox;
                    break;
            }
            obj.BeginInvoke(new Action<object, string, ControlType, Color>(ChangeUIVal), new object[] { control, val, type, color });
        }

        /// <summary>
        /// 跨线程操作UI的绑定方法
        /// </summary>
        /// <param name="control"></param>
        /// <param name="val"></param>
        /// <param name="type">1：TextBox；2：RIchTextBox；3：ComboBox</param>
        /// <param name="color">Black、Green、Red、Blue</param>
        public static void ChangeUIVal(object control, string val, ControlType type, Color color)
        {
            switch (type)
            {
                case ControlType.TextBox: // TextBox
                    TextBox obj = control as TextBox;
                    obj.Text = val;
                    break;
                case ControlType.RichTextBox: // RichTextBox
                    RichTextBox obj2 = control as RichTextBox;
                    obj2.SelectionColor = color == null ? Color.Black : color;
                    obj2.AppendText(val + "\r");
                    obj2.ScrollToCaret();
                    break;
                case ControlType.ComboBox: // ComboBox
                    ComboBox obj3 = control as ComboBox;
                    obj3.SelectedIndex = Convert.ToInt32(val);
                    break;
            }
        }

        public delegate string GetUIValDelegate(object control, ControlType type);//定义委托
        /// <summary>
        /// 跨线程
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetUIVal(object control, ControlType type)
        {
            Control obj = null;
            string result = string.Empty;
            switch (type)
            {
                case ControlType.TextBox: // TextBox
                    obj = control as TextBox;
                    break;
                case ControlType.RichTextBox: // RichTextBox
                    obj = control as RichTextBox;
                    break;
                case ControlType.ComboBox: // ComboBox
                    obj = control as ComboBox;
                    break;
            }

            if (obj.InvokeRequired)
            {
                GetUIValDelegate d = new GetUIValDelegate(GetUIVal);
                result = obj.Invoke(d, new object[] { control, type }).ToString();  
            }
            else
            {
                result = obj.Text;
            }
            return result;   

        }

        /// <summary>
        /// 十进制转十六进制 自动左边补0  
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Len"></param>  自动补0 后的总长度
        /// <returns></returns>
        public static string IntToHex(int data, int Len)
        {
            return Convert.ToString(data, 16).PadLeft(Len, '0');
        }

        /// <summary>
        /// 十六进制转十进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int HexToInt(string data)
        {
            return Convert.ToInt32(data, 16);
        }

        /// <summary>
        /// 十六进制转二进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HexToBin(string data, int len)
        {
            int change = HexToInt(data);
            return Convert.ToString(change, 2).PadLeft(len, '0');
        }

        /// <summary>
        /// 二进制转十进制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int BinToInt(string data)
        {
            return Convert.ToInt32(data, 2);
        }

        /// <summary>
        /// 获取累加和校验
        /// </summary>
        /// <param name="bs"></param>
        /// <returns></returns>
        public static string GetSumCheck(string str)
        {
            int sum = 0;
            int len = str.Length;
            for (int i = 0; i < len; i += 2)
            {
                sum += HexToInt(str.Substring(i, 2));
            }
            string cs = IntToHex(sum, 2);
            if (cs.Length > 2)
            {
                cs = cs.Substring(cs.Length - 2, 2);
            }
            return cs;
        }

        /// <summary>
        /// 每两位字符串进行倒置
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ReverseStr(string s)
        {
            string ts = "";
            if (s.Length % 2 == 0 && s.Length > 3)
            {
                for (int i = s.Length; i > 1;)
                {
                    i = i - 2;
                    ts = ts + s.Substring(i, 2);
                }
            }
            else
            {
                ts = s;
            }

            return ts;
        }

        /// <summary>
        /// 加33
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Add33(string str)
        {
            string result;
            int i;
            string f_str = "";
            string v_str;
            int xx = Convert.ToInt32("33", 16);
            for (i = 1; i <= (str.Length / 2); i++)
            {
                v_str = str.Substring(2 * i - 1 - 1, 2);
                v_str = string.Format("{0:x2}", Convert.ToInt32(v_str, 16) + xx);
                v_str = v_str.Substring(v_str.Length - 2, 2);
                f_str = f_str + v_str;
            }
            result = f_str;
            return result;
        }

        /// <summary>
        /// 减33
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Less33(string str)
        {
            string result;
            int i;
            string f_str = "";
            string v_str;
            int xx = Convert.ToInt32("33", 16);
            for (i = 1; i <= (str.Length / 2); i++)
            {
                v_str = str.Substring(2 * i - 1 - 1, 2);
                v_str = string.Format("{0:x2}", Convert.ToInt32(v_str, 16) - xx);
                v_str = v_str.Substring(v_str.Length - 2, 2);
                f_str = f_str + v_str;
            }
            result = f_str;
            return result;
        }

        /// <summary>
        /// 十六进制字符串转Ascii码
        /// </summary>
        /// <param name="dataValue"></param>
        /// <returns></returns>
        public static string Str2Ascii(string dataValue)
        {
            byte[] bytetest = Encoding.Default.GetBytes(dataValue);
            string str = "";
            foreach (byte b in bytetest)
            {
                str = str + string.Format("{0:x2}", b);
            }

            return str;
        }

        /// <summary>
        /// Ascii码转十六进制字符串
        /// </summary>
        /// <param name="dataValue"></param>
        /// <returns></returns>
        public static string Ascii2Str(string dataValue)
        {
            try
            {
                double d = dataValue.Length / 2;
                int len = (int)Math.Floor(d);
                byte[] byteArray = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    byteArray[i] = (byte)(Convert.ToInt32(dataValue.Substring(i * 2, 2), 16));
                }

                string temp = System.Text.Encoding.ASCII.GetString(byteArray);
                temp = temp.TrimEnd('\0');
                return temp;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///  AES 加密
        /// </summary>
        /// <param name="str">明文（待加密）</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string AesEncrypt(string str, string key = "1234567890123456")
        {
            try
            {
                if (string.IsNullOrEmpty(str)) return "";
                Byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateEncryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray);
            }
            catch (Exception)
            {
                return "";
            }

        }

        /// <summary>
        ///  AES 解密
        /// </summary>
        /// <param name="str">明文（待解密）</param>
        /// <param name="key">密钥</param>
        /// <returns></returns>
        public static string AesDecrypt(string str, string key = "1234567890123456")
        {
            try
            {
                if (string.IsNullOrEmpty(str)) return "";
                Byte[] toEncryptArray = Convert.FromBase64String(str);

                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateDecryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception)
            {
                return "";
            }

        }
        
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static string MD5(string strs)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.Default.GetBytes(strs);//将要加密的字符串转换为10进制的字节数组
            byte[] encryptdata = md5.ComputeHash(bytes);//将字符串加密后也转换为字符数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为加密字符串
        }
    }
}

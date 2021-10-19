using System.IO;
using System.Net;
using System.Text;

namespace Utils.helper
{
    public class HttpHelper
    {
        public static string Get(string Url, WebProxy proxyObject = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = proxyObject;
            request.KeepAlive = false;
            request.Method = "GET";
            request.ContentType = "application/json; charset=UTF-8";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }

            return retString;
        }

        public static string Post(string Url, string Data, string Referer, WebProxy proxyObject = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Proxy = proxyObject;
            request.Method = "POST";
            request.Referer = Referer;
            byte[] bytes = Encoding.UTF8.GetBytes(Data);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;
            Stream myResponseStream = request.GetRequestStream();
            myResponseStream.Write(bytes, 0, bytes.Length);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();

            myStreamReader.Close();
            myResponseStream.Close();

            if (response != null)
            {
                response.Close();
            }
            if (request != null)
            {
                request.Abort();
            }
            return retString;
        }
    }
}

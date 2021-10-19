using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

namespace Utils.helper
{
    public class XMLHelper
    {
        private XmlDocument _xml;
        public XMLHelper(string xmlFilePath)
        {
            _xml = new XmlDocument();
            _xml.Load(xmlFilePath);
        }

        public string ReadNodeVal(string nodePath) {
            XmlNode xn = _xml.SelectSingleNode(nodePath);
            return xn.InnerText;
        }
    }
}

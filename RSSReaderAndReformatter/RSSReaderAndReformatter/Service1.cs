using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace RSSReaderAndReformatter
{
    public partial class RSSReformatter : ServiceBase
    {
        private static readonly string FOLDER_PATH = @"D:\";
        private static readonly string FOLDER_PATH_LOG = @"D:\M2log.txt";
        private static readonly string FOLDER_PATH_XML = FOLDER_PATH + @"MyNews.xml";
        //private static readonly int f = 1;

        private static readonly string GEO_RSS_URL = "https://www.geo.tv/rss/1/0";
        private static readonly string SUCH_RSS_URL = "http://www.suchtv.pk/latest-news.feed";
        private XmlDocument GeoXmlDoc;
        private XmlDocument TribuneXmlDoc;
        //private XmlWriter xmlWriter;

        //private String[] XMLpaths;
        System.Timers.Timer timeDelay;

        public RSSReformatter()
        {
            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Interval = 30000;                     //30 secs
            try
            {
                GeoXmlDoc = new XmlDocument();
                GeoXmlDoc.Load(GEO_RSS_URL);
                TribuneXmlDoc = new XmlDocument();
                TribuneXmlDoc.Load(SUCH_RSS_URL);
            }
            catch
            {
                LogService("Problem in getting to URL or creating Xmldocument");
            }
            try
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineOnAttributes = true
                };
                //xmlWriter = XmlWriter.Create(FOLDER_PATH_XML, xmlWriterSettings);
            }
            catch
            {
                LogService("Problem creating xml file");
            }

            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(ChainOfWorkerProcesses);
        }

        private void ChainOfWorkerProcesses(object sender, ElapsedEventArgs e)
        {
            ParseXMLGeo();
            ParseXMLSuch();
        }

        //For debugging purposes only. Not for release.
        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            LogService("Service starting! ");
            
            timeDelay.Enabled = true;
        }
        //Gets each individual XML Node
        public static XmlNode GetXMLNode(XmlDocument doc, String nodePath)
        {
            XmlNode node = doc.DocumentElement.SelectSingleNode(nodePath);
            return node;

        }

        public static String GetNodeString(String text)
        {
            text.Replace(" ", "");
            return text;
        }

        //Repetitive code as both have different xml tags
        protected void ParseXMLGeo()
        {
            String title = null;
            String Description = null;
            String Date = null;
            String Channel = null;


            XmlNode node = GetXMLNode(GeoXmlDoc, "/rss/channel/title");
            Channel = GetNodeString(node.InnerText);
            node = GetXMLNode(GeoXmlDoc, "/rss/channel/lastBuildDate");
            Date = GetNodeString(node.InnerText);
            // Parse the Items in the RSS file
            XmlNodeList rssNodes = GeoXmlDoc.SelectNodes("rss/channel/item");

            // Iterate through the items in the RSS file
            foreach (XmlNode rssNode in rssNodes)
            {
                node = rssNode.SelectSingleNode("title");
                title = GetNodeString(node.InnerText);

                node = rssNode.SelectSingleNode("description");
                Description = GetNodeString(node.InnerText);

                DateTime localDate = DateTime.Now;
                Date = localDate.ToString();

                WritingToXML(title, Description, Date, Channel);
            }

        }

        //Repetitive code as both have different xml tags
        protected void ParseXMLSuch()
        {
            

            String title = null;
            String Description = null;
            String Date = null;
            String Channel = null;


            XmlNode node = GetXMLNode(TribuneXmlDoc, "/rss/channel/title");
            Channel = GetNodeString(node.InnerText);
            node = GetXMLNode(TribuneXmlDoc, "/rss/channel/lastBuildDate");
            Date = GetNodeString(node.InnerText);

            // Parse the Items in the RSS file
            XmlNodeList rssNodes = TribuneXmlDoc.SelectNodes("rss/channel/item");

            // Iterate through the items in the RSS file
            foreach (XmlNode rssNode in rssNodes)
            {
                node = rssNode.SelectSingleNode("title");
                title = GetNodeString(node.InnerText);

                node = rssNode.SelectSingleNode("description");
                Description = GetNodeString(node.InnerText);

               
                WritingToXML(title, Description, Date, Channel);
            }
        }

        private void WritingToXML(String title, String description, String date, String channel)
        {
            //xmlWriter.WriteStartDocument();
            /*
            try
            {
                xmlWriter.WriteStartElement("NewsItem");

                xmlWriter.WriteStartElement("Title");
                xmlWriter.WriteString(title);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Description");
                xmlWriter.WriteString(description);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("PublishedDate");
                xmlWriter.WriteString(date);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("NewsChannel");
                xmlWriter.WriteString(channel);
                xmlWriter.WriteEndElement();
                
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            }
            catch
            {
                LogService("Probelem in writing to XML");
            }
            */

            try
            {
                FileStream fs = new FileStream(FOLDER_PATH_XML, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine("\n\n\n\n<NewsItem>\n" + "\t<Title>" + title + "</Title>\n" + "\t<Description>" + description + "</Description>\n"
                    + "\t<PublishedDate>" + date + "</PublishedDate>\n" + "\t<NewsChannel>" + channel + "</NewsChannel>\n"
                    + "</NewsItem>");
                sw.Flush();
                sw.Close();
            }
            catch
            {
                LogService("Problem in printing to XML");
            }

        }


        private static void LogService(string content)
        {
            try
            {


                FileStream fs = new FileStream(FOLDER_PATH_LOG, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(content);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {

            }
        }

        protected override void OnStop()
        {
            LogService("Service Stopping! ");
        }
        
    }
}

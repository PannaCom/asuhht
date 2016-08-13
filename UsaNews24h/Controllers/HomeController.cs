using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using UsaNews24h.Models;
namespace UsaNews24h.Controllers
{
    public class HomeController : Controller
    {
        usanews24hEntities db = new usanews24hEntities();
        public ActionResult Index()
        {
            return View();
        }
        public string[] url = new string[] { "http://www.msnbc.com/feeds/latest", "http://rssfeeds.usatoday.com/usatoday-newstopstories&x=1", "http://rss.cnn.com/rss/edition_us.rss", "http://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml", "http://feeds.abcnews.com/abcnews/usheadlines", "http://feeds.foxnews.com/foxnews/latest?format=xml"};
        
        public string crawl() {
            var States = db.states_list.ToList();
            for (int i = 0; i < url.Length; i++)
            {
                XmlDocument RSSXml = new XmlDocument();
                
                try
                {

                    RSSXml.Load(url[i]);

                }
                catch (Exception ex)
                {

                }

                XmlNodeList RSSNodeList = RSSXml.SelectNodes("rss/channel/item");
                XmlNode RSSDesc = RSSXml.SelectSingleNode("rss/channel/title");

                //StringBuilder sb = new StringBuilder();
                try
                {
                    foreach (XmlNode RSSNode in RSSNodeList)
                    {

                        try
                        {
                            XmlNode RSSSubNode;
                            RSSSubNode = RSSNode.SelectSingleNode("title");
                            string title = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            RSSSubNode = RSSNode.SelectSingleNode("link");
                            string link = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            RSSSubNode = RSSNode.SelectSingleNode("description");
                            string desc = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            RSSSubNode = RSSNode.SelectSingleNode("pubDate");

                            string date = RSSSubNode != null ? RSSSubNode.InnerText : "";
                            // Kiểm tra nếu ngày gửi quá lâu thì không lấy

                            //RSSSubNode = RSSNode.SelectSingleNode("domain");
                            //string domain = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            //RSSSubNode = RSSNode.SelectSingleNode("maindomain");
                            //string maindomain = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            //RSSSubNode = RSSNode.SelectSingleNode("catid");
                            //string catid = RSSSubNode != null ? RSSSubNode.InnerText : "";
                            string all=title+" "+desc;
                            string state1="";
                            string capital1="";
                            string largestcity1 = "";
                            for (int j = 0; j < States.Count; j++) {
                                state1 = "";
                                capital1 = "";
                                largestcity1 = "";
                                if (all.Contains(States[j].state))
                                {
                                    state1= States[j].state;
                                }
                                if (state1!="" && all.Contains(States[j].capital))
                                {
                                    capital1= States[j].capital;
                                }
                                if (state1 != "" && all.Contains(States[j].largestcity))
                                {
                                    largestcity1 = States[j].largestcity;
                                }
                            }
                            if (title != null && !title.Equals(""))
                            {
                                    link = link.Trim();
                                    title = title.Trim();
                             }
                             else continue;
                        }
                        catch (Exception exInFor)
                        {
                            //int abc = 0;
                            //Array.Resize(ref arrItem, Length);
                        }
                    }//for node
                }
                catch (Exception exTryFor)
                {
                    //int abc = 0;
                }
            }
            return "ok";
        }
        public ActionResult About()
        {
           

            return View();
        }
        //Doc Rss
        private void getAllItem(string Url)
        {
            string nowDate = DateTime.Now.ToString();
            //Fetch the subscribed RSS Feed
            XmlDocument RSSXml = new XmlDocument();
            string allLinked = "";
            try
            {
              
                RSSXml.Load(Url);
             
            }
            catch (Exception ex)
            {
                
            }

            XmlNodeList RSSNodeList = RSSXml.SelectNodes("rss/channel/item");
            XmlNode RSSDesc = RSSXml.SelectSingleNode("rss/channel/title");

            //StringBuilder sb = new StringBuilder();
            try
            {
                foreach (XmlNode RSSNode in RSSNodeList)
                {
                  
                    try
                    {
                        XmlNode RSSSubNode;
                        RSSSubNode = RSSNode.SelectSingleNode("title");
                        string title = RSSSubNode != null ? RSSSubNode.InnerText : "";

                        RSSSubNode = RSSNode.SelectSingleNode("link");
                        string link = RSSSubNode != null ? RSSSubNode.InnerText : "";

                        RSSSubNode = RSSNode.SelectSingleNode("description");
                        string desc = RSSSubNode != null ? RSSSubNode.InnerText : "";

                        RSSSubNode = RSSNode.SelectSingleNode("pubDate");
                        
                        string date = RSSSubNode != null ? RSSSubNode.InnerText : "";
                        // Kiểm tra nếu ngày gửi quá lâu thì không lấy

                        //RSSSubNode = RSSNode.SelectSingleNode("domain");
                        //string domain = RSSSubNode != null ? RSSSubNode.InnerText : "";

                        //RSSSubNode = RSSNode.SelectSingleNode("maindomain");
                        //string maindomain = RSSSubNode != null ? RSSSubNode.InnerText : "";

                        //RSSSubNode = RSSNode.SelectSingleNode("catid");
                        //string catid = RSSSubNode != null ? RSSSubNode.InnerText : "";


                        if (title != null && !title.Equals(""))
                        {
                            link = link.Trim();
                            title = title.Trim();
                            
                           
                        }
                        else continue;
                    }
                    catch (Exception exInFor)
                    {
                        //int abc = 0;
                        //Array.Resize(ref arrItem, Length);
                    }
                }//for node
            }
            catch (Exception exTryFor)
            {
                //int abc = 0;
            }
            //Array.Resize(ref arrItem, Length);
        }//void
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
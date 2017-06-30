using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using UsaNews24h.Models;
using PagedList;
using SelectPdf;
using Boilerpipe.Net;
using Boilerpipe.Net.Extractors;
namespace UsaNews24h.Controllers
{
    public class HomeController : Controller
    {
        usanews24hEntities db = new usanews24hEntities();
        public ActionResult Index()
        {
            
            var st = (from q in db.states_list select q).OrderBy(o => o.state).ToList();
            int datetimeid=Config.datetimeidByDay(-7);
            StringBuilder sball = new StringBuilder();
            string content = "";
            for (int i = 0; i < st.Count; i++)
            {
                string state = st[i].state;
                var news=(from q2 in db.news where q2.date_id>=datetimeid && q2.state.Contains(state) select q2).OrderByDescending(o=>o.date_time).Take(5).ToList();
                if (news.Count <= 0) continue;
                content = "";// "<div class=\"box\" style=\"float:left;position:relative;display:block;width:32%;margin-right:2px;\">";
                content += "<div class=\"col-md-12\" style=\"background-color: #ffffff;min-height:50px;margin-bottom:10px;\">";
                content += "<span style=\"background-color:#FD0005;color:white;position:relative;top:0;padding:0px 7px;font-weight:bold;font-size: 13px;padding-right:5px;\"><a href=\"/state/" + state + "-1\" target=_blank style=\"color:white;\">" + state + "</a></span>&nbsp;";
                for (int j = 0; j < news.Count; j++)
                {
                    content += "<i class=\"fa fa-calendar\"></i>&nbsp;<span>" + news[j].date_time + "</span>";
					content +="<h6><a href=\""+news[j].link+"\" target=_blank>"+news[j].name+"</a></h6>";
                }
                content += "</div>";
                sball.Append(content);
            }
            ViewBag.all = sball.ToString();
            return View();
        }
        public ActionResult list(int? page,string state)
        {
           
            //int datetimeid = Config.datetimeidByDay(-7);
            StringBuilder sball = new StringBuilder();
                var news = (from q2 in db.news where q2.state.Contains(state) select q2).OrderByDescending(o => o.date_time).Take(1000).ToList();
                //content = "";// "<div class=\"box\" style=\"float:left;position:relative;display:block;width:32%;margin-right:2px;\">";
                //content += "<div class=\"col-md-12\" style=\"background-color: #ffffff;min-height:150px;margin-bottom:10px;\">";
                //content += "<span style=\"background-color:#FD0005;color:white;position:relative;top:0;padding:0px 7px;font-weight:bold;font-size: 13px;padding-right:5px;\"><a href=\"/list?state=" + state + "\" target=_blank style=\"color:white;\">" + state + "</a></span>&nbsp;<span>";
                //for (int j = 0; j < news.Count; j++)
                //{
                //    content += "<i class=\"fa fa-calendar\"></i> " + news[j].date_time + "</span>";
                //    content += "<h6><a href=\"" + news[j].link + "\" target=_blank>" + news[j].name + "</a></h6>";
                //}
                //content += "</div>";
                //sball.Append(content);
                ViewBag.state = state;
                int pageSize = 50;
                int pageNumber = (page ?? 1);
                ViewBag.page = page;
                return View(news.ToPagedList(pageNumber, pageSize));
        }
        //http://www.50states.com/news/ tham khao them
        public string[] url = new string[] { "https://news.google.com/news?cf=all&hl=en&pz=1&ned=us&topic=n&output=rss", "http://www.msnbc.com/feeds/latest", "http://rssfeeds.usatoday.com/usatoday-newstopstories&x=1", "http://rss.cnn.com/rss/edition_us.rss", "http://rss.nytimes.com/services/xml/rss/nyt/HomePage.xml", "http://feeds.abcnews.com/abcnews/usheadlines", "http://feeds.foxnews.com/foxnews/latest?format=xml" };
        string[] stopword = new string[] { "New York Daily News", "Chicago Tribune", "Los Angeles Times", "Washington Post", "CBS 8 San Diego", "washingtonpost" };
        public string crawl() {
            if (Config.isCrawl) return "Crawling..";
            Config.isCrawl = true;
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
                            if (link.Contains("http://news.google.com"))
                            {
                                link = link.Substring(link.IndexOf("&url=")+5);
                            }
                            RSSSubNode = RSSNode.SelectSingleNode("description");
                            string desc = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            RSSSubNode = RSSNode.SelectSingleNode("pubDate");

                            string date = RSSSubNode != null ? RSSSubNode.InnerText : "";
                            // Kiểm tra nếu ngày gửi quá lâu thì không lấy
                            string image = "";
                            try
                            {
                                if (link.Contains("http://news.google.com"))
                                {
                                    image = getImageSrc(desc);
                                }
                                else
                                {
                                    RSSSubNode = RSSNode.SelectSingleNode("media:thumbnail");
                                    image = RSSSubNode != null ? RSSSubNode.Attributes["url"].Value: "";
                                }
                            }catch(Exception img1){

                            }
                            
                            
                            //try { 
                            //    RSSSubNode = RSSNode.SelectSingleNode("media:thumbnail");
                            //    image = RSSSubNode != null ? RSSSubNode.Attributes["url"].Value: "";
                            //}catch(Exception img1){

                            //}
                            if (image==""){
                                 try {
                                     RSSSubNode = RSSNode.SelectSingleNode("media:content");
                                     image = RSSSubNode != null ? RSSSubNode.Attributes["url"].Value: "";
                                 }
                                 catch (Exception img2)
                                 {

                                 }
                            }
                            //RSSSubNode = RSSNode.SelectSingleNode("maindomain");
                            //string maindomain = RSSSubNode != null ? RSSSubNode.InnerText : "";

                            //RSSSubNode = RSSNode.SelectSingleNode("catid");
                            //string catid = RSSSubNode != null ? RSSSubNode.InnerText : "";
                            string all=title+" "+desc;
                            for (int l = 0; l < stopword.Length; l++)
                            {
                                all = all.Replace(stopword[l],"");
                            }
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
                                if (state1 != "") break;
                            }
                            if (title != null && !title.Equals("") && state1 != "")
                            {
                                    link = link.Trim();
                                    title = title.Trim();
                                    int datetimeid = int.Parse(Config.convertToDateTimeId(date));
                                   
                                    if (datetimeid != 0)
                                    {
                                        DateTime? fdate = Config.toDateTime(date);
                                        fdate = fdate.Value.AddHours(-12);
                                        var any = db.news.Any(o => o.date_id == datetimeid && o.name == title && o.link == link);
                                        if (!any)
                                        {
                                            Uri urldomain= new Uri(link);
                                            string pdf=Config.unicodeToNoMark(title) + ".pdf";
                                            savePdf(link, pdf, urldomain.Host);
                                            string full_content = getAllContent(link);

                                            news n = new news();
                                            n.date_id = datetimeid;
                                            n.date_time = fdate;
                                            n.des = desc;
                                            n.full_content = "";
                                            n.link = link;
                                            n.name = title;
                                            n.full_content = full_content;
                                            n.state = state1;
                                            n.image=image;
                                            n.time = fdate.Value.TimeOfDay;
                                            n.pdf = pdf;
                                            db.news.Add(n);
                                            db.SaveChanges();

                                        }
                                    }
                                    
                             }
                             else continue;
                        }
                        catch (Exception exInFor1)
                        {
                            //int abc = 0;
                            //Array.Resize(ref arrItem, Length);
                        }
                    }//for node
                }
                catch (Exception exTryFor2)
                {
                    //int abc = 0;
                }
            }
            Config.isCrawl = false;
            return "Done";
        }
        public string updatePdf()
        {
            try
            {
                int datetimeid=Config.datetimeidByDay(-3);
                var p = (from q in db.news where q.date_id >= datetimeid && q.pdf==null select q).ToList();
                for (int i = 0; i < p.Count; i++)
                {
                    Uri urldomain = new Uri(p[i].link);
                    string pdf = Config.unicodeToNoMark(p[i].name) + ".pdf";
                    savePdf(p[i].link, pdf, urldomain.Host);
                    db.Database.ExecuteSqlCommand("update news set pdf=N'/Files/"+DateTime.Now.ToString("yyyyMMdd")+"/"+pdf+"' where id="+p[i].id);
                }
                return "1";
            }
            catch
            {
                return "0";
            }
        }
        public string updateContent()
        {
            try
            {
                int datetimeid = Config.datetimeidByDay(-3);
                var p = (from q in db.news where q.date_id >= datetimeid select q).ToList();
                for (int i = 0; i < p.Count; i++)
                {
                    string content = getAllContent(p[i].link);
                    db.Database.ExecuteSqlCommand("update news set full_content=N'" + HttpUtility.HtmlEncode(content) + "' where id=" + p[i].id);
                }
                return "1";
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
        public string getAllContent(string link)
        {
            try
            {
                
                

                String page = String.Empty;
                WebRequest request = WebRequest.Create(link);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);

                page = streamReader.ReadToEnd();
                string text = CommonExtractors.ArticleExtractor.GetText(page);
                text = text.Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
                string allImage = "";
                string pattern = @"<(img)\b[^>]*>";
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
                MatchCollection matches = rgx.Matches(page);

                for (int i = 0, l = matches.Count; i < l; i++)
                {
                     
                    allImage += matches[i].Value;
                }
                
                return text + allImage;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public void savePdf(string url, string name,string domain)
        {
            var originalDirectory = new DirectoryInfo(string.Format("{0}Files", Server.MapPath(@"\")));
            string strDay = DateTime.Now.ToString("yyyyMMdd");
            string pathString = System.IO.Path.Combine(originalDirectory.ToString(), strDay);
            string fullPath = pathString + "\\"+name;
            bool isExists = System.IO.Directory.Exists(pathString);
            if (!isExists)
                System.IO.Directory.CreateDirectory(pathString);
            if (System.IO.File.Exists(fullPath)) return;
            HtmlToPdf converter = new HtmlToPdf();
            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertUrl(url);

            // get conversion result (contains document info from the web page)
            HtmlToPdfResult result = converter.ConversionResult;

            // set the document properties
            doc.DocumentInformation.Title = result.WebPageInformation.Title;
            doc.DocumentInformation.Subject = result.WebPageInformation.Description;
            doc.DocumentInformation.Keywords = result.WebPageInformation.Keywords;

            doc.DocumentInformation.Author = domain;
            doc.DocumentInformation.CreationDate = DateTime.Now;

            // save pdf document
            doc.Save(fullPath);

            // close pdf document
            doc.Close();
        }
        public string getImageSrc(string content)
        {
            string matchString = Regex.Match(content, "<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

            return matchString;
        }
        public ActionResult About()
        {
           

            return View();
        }
        public static string getImageFromLink(string link)
        {
            //string matchString = Regex.Match(content, "<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

            //return matchString;
            //string matchString = Regex.Match(content, "<img.*?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
            string content = getContent(link);
            Regex titRegex;
            Match titm;
            content = content.ToLower();

            titRegex = new Regex("<meta(.*?)property=\"og:image\"(.*?)content=\"(.*?)\"(.*?)/>", RegexOptions.IgnoreCase);
            titm = titRegex.Match(content);
            if (titm.Success)
            {
                content = titm.Value;
                titRegex = new Regex("content=\"(.*?)\"", RegexOptions.IgnoreCase);
                titm = titRegex.Match(content);
                return titm.Value.Replace("content=", "").Replace("\"", "");
            }


            titRegex = new Regex("<meta(.*?)content=\"(.*?).jpg\"(.*?)>", RegexOptions.IgnoreCase);
            titm = titRegex.Match(content);
            if (titm.Success)
            {
                content = titm.Value;
                titRegex = new Regex("content=\"(.*?)\"", RegexOptions.IgnoreCase);
                titm = titRegex.Match(content);
                return titm.Value.Replace("content=", "");
            }
            else
            {
                titRegex = new Regex("<meta(.*?)content=\"(.*?).gif\"(.*?)>", RegexOptions.IgnoreCase);
                titm = titRegex.Match(content);
                if (titm.Success)
                {
                    content = titm.Value;
                    titRegex = new Regex("content=\"(.*?)\"", RegexOptions.IgnoreCase);
                    titm = titRegex.Match(content);
                    return titm.Value.Replace("content=", "");
                }
                else
                {
                    titRegex = new Regex("<meta(.*?)content=\"(.*?).png\"(.*?)>", RegexOptions.IgnoreCase);
                    titm = titRegex.Match(content);
                    if (titm.Success)
                    {
                        content = titm.Value;
                        titRegex = new Regex("content=\"(.*?)\"", RegexOptions.IgnoreCase);
                        titm = titRegex.Match(content);
                        return titm.Value.Replace("content=", "");
                    }
                }
            }
            return "";
        }
        public static String getContent(String url)
        {
            String htmlCode = "";
            Random r = new Random();
            string[] myAngent = {"Chrome /36.0.1944.0",
                       "Chrome /35.0.1916.47",
                       "Firefox/29.0",                                              
                       "Chrome/32.0.1667.0",
                       "Chrome/32.0.1664.3",
                       "Chrome/28.0.1467.0",
                       "Chrome/28.0.1467.0",
                       "Firefox /31.0"};
            try
            {
                bool conti = true;
                byte stry = 0;
                WebResponse myResponse;
                StreamReader sr;
                do
                {
                    try
                    {
                        Random random = new Random();
                        int randomNumber = random.Next(0, myAngent.Length);
                        conti = false;
                        HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                        myRequest.Method = "GET";
                        myRequest.Timeout = 15000;
                        myRequest.UserAgent = myAngent[randomNumber];
                        //myRequest.TransferEncoding = "UTF-8";
                        myResponse = myRequest.GetResponse();
                        sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);//.UTF8
                        htmlCode = sr.ReadToEnd();
                        sr.Close();
                        myResponse.Close();

                    }
                    catch (Exception err1)
                    {
                        conti = true;
                        stry++;
                    }
                } while (conti && stry < 3);
            }
            catch (Exception err)
            {
                return "";
            }

            return htmlCode;
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
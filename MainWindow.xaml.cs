using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace StreamableViewerBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        bool fullplay;
        bool loop;
        bool count;
        int intcount = 0;
        int intbroken = 0;
        int inttotal = 0;
        bool time;
        int cur = 0;
        bool threadmode;
        bool submode = false;
        int intsubview = -1;
        string makelist = "";
        DateTime starttime;
        string startlink = "";
        ArrayList imagelinks = new ArrayList();
        ArrayList vidlinks = new ArrayList();
        ArrayList posts = new ArrayList();
        ArrayList alllinks = new ArrayList();
        ArrayList alllinksource = new ArrayList();
        ArrayList sublinks = new ArrayList();

        StreamWriter output;
        SHDocVw.InternetExplorer ie;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            txtStatus.Text = "Working";
            imagelinks.Clear();
            vidlinks.Clear();
            posts.Clear();
            alllinks.Clear();
            alllinksource.Clear();
            sublinks.Clear();
            LoadConfig();
            int j = 0;
            try
            {
                ie = new SHDocVw.InternetExplorer();
                ie.Visible = true;
                ie.Navigate2("www.streamable.com");
                output = new StreamWriter("output.txt");

                if (threadmode)
                {
                    loadPosts();
                    if (startlink != "")
                    {
                        cur = posts.IndexOf(startlink);
                    }
                    while (intcount < posts.Count)
                    {
                        txtStatus.Text = "Checking " + posts[cur].ToString();
                        if (intcount == 22)
                        {
                            string blagds = "HI";

                        }
                        if (cur > posts.Count - 1)
                        {
                            if (loop)
                            {
                                cur = 0;
                            }
                            else
                            {
                                finishOutput();
                                WriteConfig();
                                txtStatus.Text = "Finished";
                                return;
                            }

                        }
                        checkPost(posts[cur].ToString());
                        cur++;
                        intcount++;
                    }
                }
                else
                {
                    loadLinks();
                    for (j = 0; j < alllinks.Count; j++)
                    {
                        if (j % 100 == 0)
                        {
                            txtStatus.Text = "Checking #" + j;
                        }
                        checklink(j);
                        inttotal++;
                    }
                }
            }
            catch (Exception ex)
            {
                if ((intcount != posts.Count - 1) && threadmode)
                {
                    startlink = posts[cur].ToString();
                }
                else if (!threadmode)
                {
                    startlink = alllinks[j].ToString();
                }
            }
            finishOutput();
            WriteConfig();
            if (makelist != "")
            {
                StreamWriter sw = new StreamWriter("links.txt");
                for(int x = 0; x < alllinks.Count; x++)
                {
                    sw.WriteLine(alllinks[x] + "\t" + alllinksource[x]);
                }
                sw.Close();
            }
            if (intsubview > 0)
            {
                StreamWriter sw = new StreamWriter("subviews.txt");
                for (int x = 0; x < sublinks.Count; x++)
                {
                    sw.WriteLine(sublinks[x] + "\tN/A");
                }
                sw.Close();
            }
            Process[] IEWindows = Process.GetProcessesByName("IEXPLORE");
            for (int p = 0; p < IEWindows.Length; p++)
            {
                try
                {
                    Process curprocc = IEWindows[p];
                    if (!curprocc.HasExited)
                    {
                        curprocc.Kill();
                    }
                }
                catch (Exception ex)
                {

                }
            }
            txtStatus.Text = "Finished";
            MessageBox.Show("Kyr's bot is finished");

        }
        public void checklink(int x)
        {
            string link = alllinks[x].ToString();
            string source = alllinksource[x].ToString();

            bool working = false;
            try
            {
                working = CheckVidImage(link);
            }
            catch (Exception ex)
            {
                ie = null;
                Thread.Sleep(1500);
                Process[] IEWindows = Process.GetProcessesByName("IEXPLORE");
                for (int p = 0; p < IEWindows.Length; p++)
                {
                    try
                    {
                        Process curprocc = IEWindows[p];
                        if (!curprocc.HasExited)
                        {
                            curprocc.Kill();
                        }
                    }
                    catch (Exception killex)
                    {

                    }
                }

                ie = new SHDocVw.InternetExplorer();
                ie.Visible = true;
                ie.Navigate2("www.streamable.com");
                try
                {
                    working = CheckVidImage(link);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }

            if (!working)
            {
                output.WriteLine("LINK: " + link + ", SOURCE: " + source);
            }
        }
        public void finishOutput()
        {
            output.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~");
            if (count)
            {
                output.WriteLine("Total Links checked: " + inttotal);
                output.WriteLine("Total Broken links found: " + intbroken);
            }
            if (time)
            {
                TimeSpan testinglength = DateTime.Now - starttime;
                output.WriteLine("Total Time: " + testinglength.Hours + ":" + testinglength.Minutes + "." + testinglength.Seconds);              
            }
            output.Close();
        }
        public void checkPost(String url)
        {
            output.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~");
            output.WriteLine("Checking " + url);
            WebClient client = new WebClient();
            string webpage = client.DownloadString(url);

            try
            {
                int indexing = webpage.IndexOf("<h2 class");
                if (indexing != -1)
                {
                    webpage = webpage.Substring(indexing);
                }               
                else
                {
                    checkPostNSFW(url);
                    return;
                }
                indexing = webpage.ToLower().IndexOf("what are your thoughts? log in or sign up");
                if (indexing == -1)
                {
                    indexing = webpage.ToLower().IndexOf("commentSort--sortpicker");
                    if (indexing == -1)
                    {
                        indexing = webpage.ToLower().IndexOf("commentsignupbar");
                        if (indexing == -1)
                        {
                            indexing = webpage.ToLower().IndexOf("link-save-button");
                            if (indexing == -1)
                            {
                                indexing = webpage.Length - 10;
                            }
                        }
                    }
                }//
                webpage = webpage.Substring(0, indexing);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //href=\"https://imgur.com/a/0Eiow\">barely able to avoid a direct hit</a>
            while (webpage.IndexOf("href=\"") != -1)
            {
                String link;
                try
                { 
                    webpage = webpage.Substring(webpage.IndexOf("href=\"") - 1);
                    link = webpage.Substring(1, webpage.IndexOf("</a>"));
                    inttotal++;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //"href=\"https://streamable.com/i2m7p\">melt cars<"
                string vidurl = "";

                try
                { //"href=\"https://streamable.com/4kudv\" title=\"Parasites Lost\">Correctly identifies sinus anatomy<"
                    vidurl = link.Substring(link.IndexOf("\"") + 1);
                    vidurl = vidurl.Substring(0, vidurl.IndexOf(">") - 1);
                    if (vidurl.Contains("\" title="))
                    {
                        vidurl = vidurl.Substring(0, vidurl.IndexOf("\" title="));
                    }
                    if (makelist != "" && vidurl.Contains(makelist))
                    {
                        if (!alllinks.Contains(vidurl))
                        {
                            alllinks.Add(vidurl);
                            alllinksource.Add(url.Substring(56));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                bool working = false;
                try
                {
                    working = CheckVidImage(vidurl);
                }
                catch (Exception ex)
                {
                    ie = null;
                    Thread.Sleep(1500);
                    Process[] IEWindows = Process.GetProcessesByName("IEXPLORE");
                    for (int p = 0; p < IEWindows.Length; p++)
                    {
                        try
                        {
                            Process curprocc = IEWindows[p];
                            if (!curprocc.HasExited)
                            {
                                curprocc.Kill();
                            }
                        }
                        catch (Exception killex)
                        {

                        }
                    }

                    ie = new SHDocVw.InternetExplorer();
                    ie.Visible = true;
                    ie.Navigate2("www.streamable.com");
                    try
                    {
                        working = CheckVidImage(vidurl);
                    }
                    catch (Exception exx)
                    {
                        throw exx;
                    }
                }
                
                if (!working)
                {
                    output.WriteLine("LINK: " + vidurl + ", FULL TEXT: " + link);
                }

                try
                {
                    webpage = webpage.Substring(link.Length);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            int blar = 8;
        }
        public void checkPostNSFW(String url)
        {
            output.WriteLine("~~~NSFW Thread (maybe) detected, count and results may be off.  For best results, temporarily turn off NSFW/Spoilers~~~");
            WebClient client = new WebClient();
            string webpage = client.DownloadString(url);
            try
            {
                int indexing = webpage.IndexOf("<title");
                if (indexing != -1)
                {
                    webpage = webpage.Substring(indexing);
                }
                else
                {
                    return;
                }
                indexing = webpage.ToLower().IndexOf("what are your thoughts? log in or sign up");
                if (indexing == -1)
                {
                    indexing = webpage.ToLower().IndexOf("commentSort--sortpicker");
                    if (indexing == -1)
                    {
                        indexing = webpage.ToLower().IndexOf("commentsignupbar");
                        if (indexing == -1)
                        {
                            indexing = webpage.ToLower().IndexOf("link-save-button");
                            if (indexing == -1)
                            {
                                indexing = webpage.Length - 10;
                            }
                        }
                    }
                }//
                webpage = webpage.Substring(0, indexing);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //href=\"https://imgur.com/a/0Eiow\">barely able to avoid a direct hit</a>
            while (webpage.IndexOf("href=\\\"") != -1)
            {//\\n\\n*
                String link;
                try
                {
                    webpage = webpage.Substring(webpage.IndexOf("href=\\\""));
                    link = webpage.Substring(1, webpage.IndexOf("\\u003C/a"));
                    inttotal++;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //"href=\"https://streamable.com/i2m7p\">melt cars<"
                string vidurl = "";

                try
                { //"href=\"https://streamable.com/4kudv\" title=\"Parasites Lost\">Correctly identifies sinus anatomy<"
                    vidurl = link.Substring(link.IndexOf("\"") + 1);
                    vidurl = vidurl.Substring(0, vidurl.IndexOf("\\"));
                    if (vidurl.Contains("\" title="))
                    {
                        vidurl = vidurl.Substring(0, vidurl.IndexOf("\" title="));
                    }
                    if (makelist != "" && vidurl.Contains(makelist))
                    {
                        if (!alllinks.Contains(vidurl))
                        {
                            alllinks.Add(vidurl);
                            alllinksource.Add(url.Substring(56));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                bool working = false;
                try
                {
                    working = CheckVidImage(vidurl);
                }
                catch (Exception ex)
                {
                    ie = null;
                    Thread.Sleep(1500);
                    Process[] IEWindows = Process.GetProcessesByName("IEXPLORE");
                    for (int p = 0; p < IEWindows.Length; p++)
                    {
                        try
                        {
                            Process curprocc = IEWindows[p];
                            if (!curprocc.HasExited)
                            {
                                curprocc.Kill();
                            }
                        }
                        catch (Exception killex)
                        {

                        }
                    }

                    ie = new SHDocVw.InternetExplorer();
                    ie.Visible = true;
                    ie.Navigate2("www.streamable.com");
                    try
                    {
                        working = CheckVidImage(vidurl);
                    }
                    catch (Exception exx)
                    {
                        throw exx;
                    }
                }

                if (!working)
                {
                    output.WriteLine("LINK: " + vidurl + ", FULL TEXT: " + link);
                }

                try
                {
                    webpage = webpage.Substring(link.Length);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            int blar = 8;
        }
        public void LoadConfig()
        {
            StreamReader sr = new StreamReader("config.txt");

            String io = sr.ReadLine();
            if (io.Contains("Thread"))
            {
                threadmode = true;
            }
            else
            {
                threadmode = false;
                if(io.Contains("Subview"))
                {
                    submode = true;
                }
            }
            

            io = sr.ReadLine();
            fullplay = Boolean.Parse(io.Substring(8));

            io = sr.ReadLine();
            loop = Boolean.Parse(io.Substring(5));

            io = sr.ReadLine();
            count = Boolean.Parse(io.Substring(6));

            io = sr.ReadLine();
            time = Boolean.Parse(io.Substring(5));
            if (time)
            {
                starttime = DateTime.Now;
            }

            io = sr.ReadLine();
            if (io.Length > 10)
            {
                makelist = io.Substring(8).Trim();
            }

            io = sr.ReadLine();
            if (io.Length > 4)
            {
                intsubview = int.Parse(io.Substring(io.IndexOf(" ")));
            }

            io = sr.ReadLine();
            if (io.Length > 7)
            {
                startlink = io.Substring(6).Trim();
            }

            sr.Close();
            return;
            
        }
        public void WriteConfig()
        {
            StreamWriter sw = new StreamWriter("config.txt");
            if (threadmode)
            {
                sw.WriteLine("Mode Thread");
            }
            else
            {
                if (submode)
                {
                    sw.WriteLine("Mode Subview");
                }
                else
                {
                    sw.WriteLine("Mode Link");
                }
            }
            sw.WriteLine("Fullplay " + fullplay);
            sw.WriteLine("Loop " + loop);
            sw.WriteLine("Count " + count);
            sw.WriteLine("Time " + time);
            sw.WriteLine("MakeList " + makelist);
            if (intsubview > 0)
            {
                sw.WriteLine("Sub " + intsubview);
            }
            else
            {
                sw.WriteLine("Sub");
            }
            sw.WriteLine("Start " + startlink);
            sw.Close();

        }
        public void loadPosts()
        {
            StreamReader sr = new StreamReader("threads.txt");
            while (!sr.EndOfStream)
            {
                posts.Add(sr.ReadLine());
            }
        }
        public void loadLinks()
        {
            StreamReader sr;
            if (submode)
            {
                sr = new StreamReader("subviews.txt");
            }
            else
            {
                sr = new StreamReader("links.txt");
            }
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                string[] links = line.Split(new char[] { '\t' });
                alllinks.Add(links[0]);
                if (links.Length == 2)
                {
                    alllinksource.Add(links[1]);
                }
                else
                {
                    alllinksource.Add("N/A");
                }
            }
            sr.Close();
        }
        public bool CheckVidImage(String url)
        {
            try
            {
                if (url.Contains("streamable"))
                {
                    return CheckStreamable(url);
                }
                else if(url.Contains("gfycat"))
                {
                    return CheckGfycat(url);
                }
                else if (url.Contains("imgur"))
                {
                    return CheckImgur(url);
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool CheckGfycat(String url)
        {
            try
            {

                ie.Navigate2(url);
                //wb.ReadyState != WebBrowserReadyState.Complete
                while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {

                }
                mshtml.HTMLDocument vidpage = (mshtml.HTMLDocument)ie.Document;

                String title = vidpage.title;

                
                if (title.Contains("Not Found"))
                {
                    intbroken++;
                    return false;
                }
                else
                {//Fullplay not currently supported on gfycat
                    //Thread.Sleep(1000);
                    //if (fullplay)
                    //{
                    //    double dur = vid.getAttribute("duration");
                    //    double curtime = vid.getAttribute("currentTime");
                    //    bool past = false;
                    //    while (curtime != dur && (curtime > .5 || !past))
                    //    {
                    //        if (!past && curtime > .5)
                    //        {
                    //            past = true;
                    //        }
                    //        curtime = vid.getAttribute("currentTime");
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("The remote server returned an error: (404) Not Found.") ||
                    ex.Message.Equals("The remote server returned an error: (500) Internal Server Error."))
                {
                    intbroken++;
                    return false;
                }
                else
                {
                    throw ex;
                }
            }
            return true;
        }
        public bool CheckStreamable(String url)
        {
            try
            {           
                ie.Navigate2(url);
                //wb.ReadyState != WebBrowserReadyState.Complete
                while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {

                }
                mshtml.HTMLDocument vidpage = (mshtml.HTMLDocument)ie.Document;

                mshtml.HTMLInputElement vid = (mshtml.HTMLInputElement)vidpage.all.item("video-player-tag", 0);

                if (vid == null)
                {
                    intbroken++;
                    return false;
                }
                else
                {
                    if (intsubview > 0)
                    {
                        mshtml.IHTMLElement views = vidpage.getElementById("visits");
                        string numviews = views.innerText.ToLower().Replace("views", "").Trim();
                        int intviews = int.Parse(numviews);

                        if (intviews <= intsubview && !sublinks.Contains(url))
                        {
                            sublinks.Add(url);
                        }
                    }
                    Thread.Sleep(1000);
                    if (fullplay)
                    {
                        double dur = vid.getAttribute("duration");
                        double curtime = vid.getAttribute("currentTime");
                        bool past = false;
                        while (curtime != dur && (curtime > .5 || !past))
                        {
                            if (!past && curtime > .5)
                            {
                                past = true;
                            }
                            curtime = vid.getAttribute("currentTime");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("The remote server returned an error: (404) Not Found.") ||
                    ex.Message.Equals("The remote server returned an error: (500) Internal Server Error."))
                {
                    intbroken++;
                    return false;
                }
                else
                {
                    throw ex;
                }
            }
            return true;
        }
        public bool CheckImgur(String url)
        {
            try
            {
                WebClient client = new WebClient();
                String imagepage = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("The remote server returned an error: (404) Not Found."))
                {
                    intbroken++;
                    return false;
                }
                else
                {
                    throw ex;
                }

            }
            return true;
        }
    }
}

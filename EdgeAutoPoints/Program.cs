using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;

namespace EdgeAutoPoints
{
    internal class Program
    {
        //https://stackoverflow.com/questions/1119841/net-console-application-exit-event
        static bool exitSystem = false;
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");

            //do your cleanup here
            KillIt();

            Console.WriteLine("Cleanup complete");

            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion

        public class SyndicationItemWIndex
        {
            public int Index { get; set; }
            public SyndicationItem Item { get; set; }
        }

        private static readonly int minSleepTimerSeconds = 1;
        private static readonly int maxSleepTimerSeconds = 4;
        
        public static void Main()
        {
            // Some biolerplate to react to close window event, CTRL-C, kill, etc
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            AppStart();
            //hold the console so it doesn’t run off the end
            while (!exitSystem)
            {
                Thread.Sleep(500);
            }
        }

        public static void AppStart()
        {
            var items = getSyndicationItems();
            Console.WriteLine("Start earning points on (" + items.Count + " pages) in Edge!");
            //randomize
            var shuffledItems = new List<SyndicationItemWIndex>();
            shuffledItems.AddRange(items.OrderBy(a => Guid.NewGuid()).ToList());
            RunFeedProcess(shuffledItems);
        }
        
        private static List<SyndicationItemWIndex> addIndexToList(List<SyndicationItem> items) 
        {
            return items.Select((item, index) => new SyndicationItemWIndex { Index = index, Item = item }).ToList();
        }

        private static List<SyndicationItemWIndex> getSyndicationItems()
        {
            var TulsaNewsUrl =
                "http://www.newson6.com/category/208401/newson6com-news-rss?clienttype=rss"; //Tulsa Local News
            var Url = "http://www.wsj.com/xml/rss/3_7455.xml"; //The Wall Street Journal: "Technology: What's News"
            var bbcUrl = @"http://feeds.bbci.co.uk/news/rss.xml?edition=uk";
            var bbcUSCanada = @"http://feeds.bbci.co.uk/news/rss.xml?edition=us";
            var bbcRestWorld = @"http://feeds.bbci.co.uk/news/rss.xml?edition=int";

            //get all items
            var items = new List<SyndicationItem>();
            items.AddRange(getSyndicationFeed(TulsaNewsUrl).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcUrl).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcUSCanada).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcRestWorld).Items.ToList());

            return addIndexToList(items);
        }

        private static SyndicationFeed getSyndicationFeed(string url)
        {
            var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            return feed;
        }

        private static void RunFeedProcess(List<SyndicationItemWIndex> items)
        {
            var totalSeconds = items.Count * maxSleepTimerSeconds;
            var lapsedSeconds = 0;
            for (var i = 1; i < items.Count; i++)
            {
                var randSleepTimer = randNumber(maxSleepTimerSeconds, minSleepTimerSeconds);
                lapsedSeconds += randSleepTimer;
                totalSeconds -= randSleepTimer;
                var item = items[i];

                var calcTimeRemaining =
                    "T-" + (totalSeconds - lapsedSeconds).ToString().PadLeft(totalSeconds.ToString().Length);
                var calcRunning = " Running:" + i.ToString().PadLeft(items.Count.ToString().Length) + "/" + items.Count;
                var calcCurrentIndex = " index[" + item.Index.ToString().PadLeft(items.Count.ToString().Length) + "]";
                var calcTitle = " search: '" + item.Item.Title.Text + "'";

                Console.WriteLine(calcTimeRemaining
                               + calcRunning
                               + calcCurrentIndex
                               + calcTitle);
                Thread.Sleep(randSleepTimer * 1000);

                var search = Process.Start("microsoft-edge:https://www.bing.com/search?q=" + item.Item.Title.Text + "");
                search?.WaitForExit();
            }

            KillIt();
        }

        private static int randNumber(int maxInt = 4, int startInt = 0)
        {
            var r = new Random();
            return r.Next(startInt, maxInt); //for ints
        }

        private static void KillIt()
        {
            var allProcs = Process.GetProcessesByName("MicrosoftEdge");
            foreach (var process in allProcs)
            {
                process.Kill();
                
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;

namespace EdgeAutoPoints
{
    internal class Program
    {
        public class SyndicationItemWIndex
        {
            public int Index { get; set; }
            public SyndicationItem Item { get; set; }
        }

        private static readonly int minSleepTimerSeconds = 1;
        private static readonly int maxSleepTimerSeconds = 4;
        
        public static void Main()
        {
            var items = getSyndicationItems();

            var answer = "";
            Console.WriteLine("Start earning points on (" + items.Count + " pages) in Edge? (Yes/No)");
            answer = Console.ReadLine();
            if (answer == "Yes" || answer == "yes" || answer == "Y" || answer == "y")
            {
                //randomize
                var shuffledItems = new List<SyndicationItemWIndex>();
                shuffledItems.AddRange(items.OrderBy(a => Guid.NewGuid()).ToList());

                RunFeedProcess(shuffledItems);
            }

            else
            {
                Console.WriteLine("Why did you even open this...?");
            }

            Console.WriteLine("Have a nice day!");
            Console.ReadKey();
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
            foreach (var process in Process.GetProcessesByName("MicrosoftEdgeCP")) process.Kill();
        }
    }
}
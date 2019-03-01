using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace EdgeAutoPoints
{
    class Program
    {
        private static int sleepTimer = 4000;

        public static void Main()
        {
            var items = getSyndicationItems();

            var answer = "";

            Console.WriteLine("Start earning points on (" + items.Count + " pages) in Edge? (Yes/No)");

            answer = Console.ReadLine();

            if (answer == "Yes" || answer == "yes" || answer == "Y" || answer == "y")
            {
                //randomize
                List<SyndicationItem> shuffledItems = new List<SyndicationItem>();
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

        private static List<SyndicationItem> getSyndicationItems()
        {
            var TulsaNewsUrl = "http://www.newson6.com/category/208401/newson6com-news-rss?clienttype=rss"; //Tulsa Local News
            var Url = "http://www.wsj.com/xml/rss/3_7455.xml"; //The Wall Street Journal: "Technology: What's News"
            var bbcUrl = @"http://feeds.bbci.co.uk/news/rss.xml?edition=uk";
            var bbcUSCanada = @"http://feeds.bbci.co.uk/news/rss.xml?edition=us";
            var bbcRestWorld = @"http://feeds.bbci.co.uk/news/rss.xml?edition=int";
            
            //get all items
            List<SyndicationItem> items = new List<SyndicationItem>();
            items.AddRange(getSyndicationFeed(TulsaNewsUrl).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcUrl).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcUSCanada).Items.ToList());
            items.AddRange(getSyndicationFeed(bbcRestWorld).Items.ToList());
            return items;
        }

        private static SyndicationFeed getSyndicationFeed(string url)
        {
            XmlReader reader = XmlReader.Create(url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            return feed;
        }

        private static void RunFeedProcess(List<SyndicationItem> items)
        {
            for (int i = 1; i < items.Count; i++)
            {
               var item = items[i];

                Console.WriteLine(i + "/" + items.Count + " ---> " + item.Title.Text);
                var totalSeconds = (items.Count * sleepTimer) / 1000;
                var currentSeconds = (i * sleepTimer) / 1000;

                Console.WriteLine((totalSeconds - currentSeconds) + "s || " + ((totalSeconds - currentSeconds) / 60) + "m remaining");
                
                System.Threading.Thread.Sleep(sleepTimer);

                var search = Process.Start("microsoft-edge:https://www.bing.com/search?q=" + item.Title.Text + "");
                search?.WaitForExit();
            }
            KillIt();
        }

        private static void KillIt()
        {
            foreach (var process in Process.GetProcessesByName("MicrosoftEdgeCP"))
            {
                process.Kill();
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.ServiceModel.Syndication;
using System.Xml;

namespace EdgeAutoPoints
{
    class Program
    {
        public static void Main()
        {
            var answer = "";

            Console.WriteLine("Start earning points on Edge? (Yes/No)");

            answer = Console.ReadLine();

            if (answer == "Yes" || answer == "yes" || answer == "Y" || answer == "y")
            {
                var TulsaNewsUrl = "http://www.newson6.com/category/208401/newson6com-news-rss?clienttype=rss"; //Tulsa Local News
                var Url = "http://www.wsj.com/xml/rss/3_7455.xml"; //The Wall Street Journal: "Technology: What's News"
                var bbcUrl = @"http://feeds.bbci.co.uk/news/rss.xml?edition=uk";
                var bbcUSCanada = @"http://feeds.bbci.co.uk/news/rss.xml?edition=us";
                var bbcRestWorld = @"http://feeds.bbci.co.uk/news/rss.xml?edition=int";

                XmlReader reader;
                SyndicationFeed feed;                
                
                //RunFeedProcess(Url);
                RunFeedProcess(TulsaNewsUrl);
                RunFeedProcess(bbcUrl);
                RunFeedProcess(bbcUSCanada);
                RunFeedProcess(bbcRestWorld);

            }
        
            else
            {
                Console.WriteLine("Why did you even open this in that case?");
            }

            Console.WriteLine("Have a nice day!");
            Console.ReadKey();
        }

        private static void RunFeedProcess(string Url)
        {
            XmlReader reader = XmlReader.Create(Url);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            foreach (SyndicationItem item in feed.Items)
            {
                Item.Title = item.Title.Text;

                Console.WriteLine(Item.Title);

                System.Threading.Thread.Sleep(4000);

                var search = Process.Start("microsoft-edge:https://www.bing.com/search?q=" + Item.Title + "");

                search?.WaitForExit();
            }
        }

        public static class Item
        {
            public static string Title { get; set; }
        }
    }
}

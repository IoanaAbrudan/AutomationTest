using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Automation
{
    class AutomationTest
    {
        private static string _deckId;
        private const string ShuffleCards = "https://deckofcardsapi.com/api/deck/new/shuffle/?deck_count=1";
        private string _drawCard = "https://deckofcardsapi.com/api/deck/new/draw/?count=";

        private async Task<string> DrawCardsAsync(int count, string deckId)
        {
            string[] remainingCards = new string[5];

            _drawCard = "https://deckofcardsapi.com/api/deck/" + deckId + "/draw/?count=" + count;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(_drawCard);

            request.Method = WebRequestMethods.Http.Get;

            WebResponse response = await request.GetResponseAsync();

            var responseString = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()).ReadToEnd();
            string[] words = responseString.Split(',');

            foreach (var word in words)
            {
                if (word.Contains("remaining"))
                {
                    var remainingString = word;
                    remainingCards = remainingString.Split(':');
                    remainingCards[1] = remainingCards[1].Substring(1, 2);
                }
            }

            return remainingCards[1];
        }

        private async Task<string> ShuffleCard()
        {
            string[] remainingCards = new string[5];

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(ShuffleCards);

            request.Method = WebRequestMethods.Http.Get;

            WebResponse response = await request.GetResponseAsync();

            var responseString = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()).ReadToEnd();
            string[] words = responseString.Split(',');
            foreach (var word in words)
            {
                string remainingString;
                if (word.Contains("remaining"))
                {
                    remainingString = word;
                    remainingCards = remainingString.Split(':');
                    remainingCards[1] = remainingCards[1].Substring(1, 2);
                }

                if (word.Contains("deck_id"))
                {
                    remainingString = word;
                    remainingCards = remainingString.Split(':');
                    _deckId = remainingCards[1].Substring(2, 12);
                }
            }

            return remainingCards[1];
        }

        [Test]
        public static void ValidResponse_CheckCards()
        {
            AutomationTest test = new AutomationTest();
            Random num = new Random();
            var newCount = 0;

            var initialCount = Int32.Parse(test.ShuffleCard().GetAwaiter().GetResult());
            Console.WriteLine("After the shuffle, number of cards are :" + initialCount);
            var finalCount = initialCount;

            for (var i = 0; i < 5; i++)
            {
                var noOfCards = num.Next(1, 5);
                Console.WriteLine("The Number of Cards Drawn are :" + noOfCards);
                newCount = Int32.Parse(test.DrawCardsAsync(noOfCards, _deckId).GetAwaiter().GetResult());
                finalCount = finalCount - noOfCards;
                Console.WriteLine("The new count and final count after {0} draw is {1} and {2} respectively:", i,
                    newCount, finalCount);
            }

            Console.WriteLine("The Final Count outside loop is :" + finalCount);
            Assert.That(newCount.Equals(finalCount));
        }
    }
}
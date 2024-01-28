using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace Winslow_TTC_BusRoute
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<string> stopIds = new List<string> { "8998", "8997" };

            while (true)
            {
                Console.Clear();
                await DisplayPredictionsForStops(stopIds);
                await Task.Delay(TimeSpan.FromSeconds(20));
            }
        }

        static async Task DisplayPredictionsForStops(List<string> stopIds)
        {
            string agencyTag = "ttc";

            using (HttpClient httpClient = new HttpClient())
            {
                List<string> northBuses = new List<string>();
                List<string> southBuses = new List<string>();

                foreach (string stopId in stopIds)
                {
                    string url = $"https://retro.umoiq.com/service/publicXMLFeed?command=predictions&a={agencyTag}&stopId={stopId}";
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string xmlResponse = await response.Content.ReadAsStringAsync();
                        ParsePredictions(xmlResponse, stopId, northBuses, southBuses);
                    }
                    else
                    {
                        Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
                    }
                }

                Console.WriteLine("BUSES TRAVELING NORTH:\n");
                foreach (string busInfo in northBuses)
                {
                    Console.WriteLine(busInfo);
                    Console.WriteLine();
                }

                Console.WriteLine("BUSES TRAVELING SOUTH:\n");
                foreach (string busInfo in southBuses)
                {
                    Console.WriteLine(busInfo);
                    Console.WriteLine();
                }
            }
        }

        static void ParsePredictions(string xmlResponse, string stopId, List<string> northBuses, List<string> southBuses)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlResponse);

            XmlNodeList predictionsNodes = xmlDoc.SelectNodes("/body/predictions");

            foreach (XmlNode predictionsNode in predictionsNodes)
            {
                XmlNodeList directionNodes = predictionsNode.SelectNodes("direction");

                foreach (XmlNode directionNode in directionNodes)
                {
                    XmlNodeList predictionNodes = directionNode.SelectNodes("prediction");

                    if (predictionNodes.Count > 0)
                    {
                        string routeTitle = predictionsNode.Attributes["routeTitle"].Value;
                        string stopTitle = predictionsNode.Attributes["stopTitle"].Value;
                        string directionTitle = directionNode.Attributes["title"].Value.ToUpper();

                        string busInfo = $"Route: {routeTitle}\nStop: {stopTitle}\nDirection: {directionTitle}";

                        foreach (XmlNode predictionNode in predictionNodes)
                        {
                            string minutes = predictionNode.Attributes["minutes"].Value;
                            string seconds = predictionNode.Attributes["seconds"].Value;
                            bool isDeparture = Convert.ToBoolean(predictionNode.Attributes["isDeparture"].Value);

                            string departureOrArrival = isDeparture ? "Departure" : "Arrival";
                            busInfo += $"\n{departureOrArrival} in {minutes} minutes ({seconds} seconds)";
                        }

                        if (directionTitle.Contains("NORTH"))
                        {
                            northBuses.Add(busInfo);
                        }
                        else if (directionTitle.Contains("SOUTH"))
                        {
                            southBuses.Add(busInfo);
                        }
                    }
                }
            }
        }
    }
}
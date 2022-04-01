using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CS685HW3
{
    class Program
    {
        private const string startUrl = "https://www.engr.uky.edu";

        static void Main(string[] args)
        {
            Helpers helpers = new Helpers();

            DateTime startTime = DateTime.Now;
            //helpers.PostStartTime(startTime);
            Console.WriteLine("Crawl started at " + startTime);

            Dictionary<string, DateTime> visitedUrls = new Dictionary<string, DateTime>();
            Dictionary<string, string> foundFiles = new Dictionary<string, string>();
            Dictionary<string, string> httpErrors = new Dictionary<string, string>();
            Dictionary<string, List<string>> webGraph = new Dictionary<string, List<string>>();
            Dictionary<string,Dictionary<string,int>> invertedIndex = new Dictionary<string, Dictionary<string, int>>();
            List<string> stopwords = File.ReadAllLines("./stopwords.txt").ToList();
            Queue<string> urlFrontier = new Queue<string>();

            urlFrontier.Enqueue(startUrl);

            using (WebClient client = new WebClient())
            {
                while (urlFrontier.Count > 0)
                {
                    bool error = false;
                    string nextURL = urlFrontier.Dequeue();
                    List<string> childLinks = new List<string>();
                    string nextContent = null;

                    try
                    {
                        nextContent = client.DownloadString(nextURL);
                        //Console.WriteLine("PARSE: " + nextURL + "\n");
                    }
                    catch(WebException ex)
                    {
                        var res = ex.Response as HttpWebResponse;

                        error = true;

                        if(res != null)
                        {
                            //Console.WriteLine((int)res.StatusCode + " ERROR AT " + nextURL + "\n");

                            int errorCode = (int)res.StatusCode;

                            if (!httpErrors.ContainsKey(nextURL))
                            {
                                httpErrors.Add(nextURL, errorCode.ToString());
                                //helpers.PostError(nextURL, errorCode.ToString());
                            }
                        }
                        else
                        {
                            //Console.WriteLine(nextURL + " IS THROWING AN UNSPECIFIED ERROR");
                            //Console.WriteLine("UNSPECIFIED ERROR: " + ex.StackTrace);

                            if (!httpErrors.ContainsKey(nextURL))
                            {
                                httpErrors.Add(nextURL, "UNSPECIFIED");
                                //helpers.PostError(nextURL, "UNSPECIFIED");
                            }
                        }
                    }

                    if (!error)
                    {
                        var nextTermCount = helpers.GetTerms(nextContent, stopwords);
                        if(!invertedIndex.ContainsKey(nextURL))
                            invertedIndex.Add(nextURL, nextTermCount);
                        List<string> links = helpers.GetLinks(nextURL, nextContent);

                        if (links.Count != 0)
                        {
                            foreach(var link in links)
                            {
                                string urlType = helpers.CheckURL(link);
                                Uri processedLink = null;

                                if (!visitedUrls.ContainsKey(link))
                                {
                                    switch (urlType)
                                    {
                                        case "link":
                                            processedLink = new Uri(link);

                                            if(!childLinks.Contains(processedLink.AbsoluteUri) && processedLink.AbsoluteUri != nextURL)
                                                childLinks.Add(processedLink.AbsoluteUri);

                                            if (!helpers.CheckForDupedURL(processedLink.AbsoluteUri, urlFrontier) 
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, visitedUrls)
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, nextURL)
                                                )
                                            {
                                                urlFrontier.Enqueue(processedLink.AbsoluteUri);
                                            }
                                            break;
                                        case "resource":
                                            processedLink = new Uri(helpers.GetURLRoot(nextURL) + link);

                                            if(!childLinks.Contains(processedLink.AbsoluteUri) && processedLink.AbsoluteUri != nextURL)
                                                childLinks.Add(processedLink.AbsoluteUri);

                                            if (!helpers.CheckForDupedURL(processedLink.AbsoluteUri, urlFrontier) 
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, visitedUrls)
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, nextURL))
                                            {
                                                urlFrontier.Enqueue(processedLink.AbsoluteUri);
                                            }
                                            break;
                                        case "filtered":
                                            //do nothing
                                            break;
                                        default:
                                            if (helpers.GetURLRoot(link) == "")
                                                processedLink = new Uri(helpers.GetURLRoot(nextURL) + link);
                                            else
                                                processedLink = new Uri(link);

                                            if(helpers.GetURLRoot(processedLink.AbsoluteUri).Contains("engr.uky.edu"))
                                            {
                                                if (!helpers.CheckForDupedURL(processedLink.AbsoluteUri, foundFiles))
                                                {
                                                    foundFiles.Add(processedLink.AbsoluteUri, urlType);

                                                    try
                                                    {
                                                        if(processedLink.AbsoluteUri.ToUpper().Contains("PDF"))
                                                        {
                                                            var pdfBytes = client.DownloadData(processedLink.AbsoluteUri);
                                                            var termCount = helpers.GetPDFTerms(pdfBytes, stopwords);

                                                            if(!invertedIndex.ContainsKey(processedLink.AbsoluteUri))
                                                                invertedIndex.Add(processedLink.AbsoluteUri, termCount);
                                                        }
                                                    }
                                                    catch(Exception ex)
                                                    {
                                                        Console.WriteLine("Error getting content from pdf: " + processedLink.AbsoluteUri);
                                                        Console.WriteLine(ex.StackTrace);
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                                else
                                {
                                    switch(urlType)
                                    {
                                        case "link":
                                            processedLink = new Uri(link);
                                            if(!childLinks.Contains(processedLink.AbsoluteUri) && processedLink.AbsoluteUri != nextURL)
                                                childLinks.Add(processedLink.AbsoluteUri);
                                            break;
                                        case "resource":
                                            processedLink = new Uri(link);
                                            if(!childLinks.Contains(processedLink.AbsoluteUri) && processedLink.AbsoluteUri != nextURL)
                                                childLinks.Add(processedLink.AbsoluteUri);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    Uri processedUrl = new Uri(nextURL);

                    if (!visitedUrls.ContainsKey(processedUrl.AbsoluteUri))
                    {
                        visitedUrls.Add(processedUrl.AbsoluteUri, DateTime.Now);
                        //helpers.PostPage(processedUrl.AbsoluteUri, DateTime.Now);
                    }

                    if(!webGraph.ContainsKey(processedUrl.AbsoluteUri))
                        webGraph.Add(processedUrl.AbsoluteUri, childLinks);
                }

                DateTime finishedTime = DateTime.Now;
                Console.WriteLine("Crawl started at " + startTime);
                Console.WriteLine("Crawl finished at " + finishedTime);
                //helpers.PostEndTime(finishedTime);
                var elapsed = finishedTime.Subtract(startTime);
                Console.WriteLine("Crawl duration: " + elapsed);

                int successCount = visitedUrls.Count - httpErrors.Count;
                Console.WriteLine("Succesfully Crawled: " + successCount + "\n");

                Console.WriteLine("Crawled with http errors: " + httpErrors.Count);
                Console.WriteLine("HTTP Error Counts: ");
                helpers.PrintDictCounts(httpErrors);
                
                Console.WriteLine("\nTotal Crawled: " + visitedUrls.Count + "\n");

                Console.WriteLine("Filetypes encountered: ");
                helpers.PrintDictCounts(foundFiles);

                Console.WriteLine("Subdomains: ");
                List<string> subdomains = new List<string>();

                foreach (var url in visitedUrls.Keys)
                {
                    string nextRoot = helpers.GetURLRoot(url);

                    if (nextRoot != "https://www.engr.uky.edu" && !subdomains.Contains(nextRoot))
                    {
                        Console.WriteLine(nextRoot);
                        subdomains.Add(nextRoot);
                    }
                }

                Console.WriteLine("\n Subdomain Count: " + subdomains.Count);

                string webGraphJson = JsonSerializer.Serialize(webGraph);
                File.WriteAllText("../../../../Outputs/graph.json", webGraphJson);

                string invertedIndexJson = JsonSerializer.Serialize(invertedIndex);
                File.WriteAllText("../../../../Outputs/index.json", invertedIndexJson);
            }
        }
    }
}

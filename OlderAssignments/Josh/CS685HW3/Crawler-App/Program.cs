using System;
using System.Collections.Generic;
using System.Net;

namespace CS685HW3
{
    class Program
    {
        private const string startUrl = "https://www.engr.uky.edu";

        static void Main(string[] args)
        {
            Helpers helpers = new Helpers();

            DateTime startTime = DateTime.Now;
            helpers.PostStartTime(startTime);
            Console.WriteLine("Crawl started at " + startTime);

            Dictionary<string, DateTime> visitedUrls = new Dictionary<string, DateTime>();
            Dictionary<string, string> foundFiles = new Dictionary<string, string>();
            Dictionary<string, string> httpErrors = new Dictionary<string, string>();
            Queue<string> urlFrontier = new Queue<string>();

            urlFrontier.Enqueue(startUrl);

            using (WebClient client = new WebClient())
            {
                while (urlFrontier.Count > 0)
                {
                    bool error = false;
                    string nextURL = urlFrontier.Dequeue();
                    string nextContent = null;

                    try
                    {
                        nextContent = client.DownloadString(nextURL);
                        Console.WriteLine("PARSE: " + nextURL + "\n");
                    }
                    catch(WebException ex)
                    {
                        var res = ex.Response as HttpWebResponse;

                        error = true;

                        if(res != null)
                        {
                            Console.WriteLine((int)res.StatusCode + " ERROR AT " + nextURL + "\n");

                            int errorCode = (int)res.StatusCode;

                            if (!httpErrors.ContainsKey(nextURL))
                            {
                                httpErrors.Add(nextURL, errorCode.ToString());
                                helpers.PostError(nextURL, errorCode.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine(nextURL + " IS THROWING AN UNSPECIFIED ERROR");
                            Console.WriteLine("UNSPECIFIED ERROR: " + ex.StackTrace);

                            if (!httpErrors.ContainsKey(nextURL))
                            {
                                httpErrors.Add(nextURL, "UNSPECIFIED");
                                helpers.PostError(nextURL, "UNSPECIFIED");
                            }
                        }
                    }

                    if (!error)
                    {
                        List<string> links = helpers.GetLinks(nextURL, nextContent);

                        if (links.Count != 0)
                        {
                            foreach (var link in links)
                            {
                                if (!visitedUrls.ContainsKey(link))
                                {
                                    string urlType = helpers.CheckURL(link);
                                    Uri processedLink = null;

                                    switch (urlType)
                                    {
                                        case "link":
                                            processedLink = new Uri(link);

                                            if (!helpers.CheckForDupedURL(processedLink.AbsoluteUri, urlFrontier) 
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, visitedUrls))
                                                urlFrontier.Enqueue(processedLink.AbsoluteUri);
                                            break;
                                        case "resource":
                                            processedLink = new Uri(helpers.GetURLRoot(nextURL) + link);

                                            if (!helpers.CheckForDupedURL(processedLink.AbsoluteUri, urlFrontier) 
                                                && !helpers.CheckForDupedURL(processedLink.AbsoluteUri, visitedUrls))
                                                urlFrontier.Enqueue(processedLink.AbsoluteUri);
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
                                                    helpers.PostFile(processedLink.AbsoluteUri, urlType);
                                                }
                                            }
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
                        helpers.PostPage(processedUrl.AbsoluteUri, DateTime.Now);
                    }
                }

                DateTime finishedTime = DateTime.Now;
                Console.WriteLine("Crawl started at " + startTime);
                Console.WriteLine("Crawl finished at " + finishedTime);
                helpers.PostEndTime(finishedTime);
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
            }
        }
    }
}

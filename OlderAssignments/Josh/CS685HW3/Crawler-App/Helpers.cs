using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using System.Linq;
using UglyToad.PdfPig;

namespace CS685HW3
{    
    class Helpers
    {
        public List<string> GetLinks(string url, string content)
        {
            List<string> discoveredLinks = new List<string>();
            HtmlDocument doc = new HtmlDocument();

            if(content != null)
            {
                doc.LoadHtml(content);

                var hrefNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                var linkNodes = doc.DocumentNode.SelectNodes("//link[@href]");
                var imgNodes = doc.DocumentNode.SelectNodes("//img[@src]");
                var scriptNodes = doc.DocumentNode.SelectNodes("//script[@src]");
                var sourceNodes = doc.DocumentNode.SelectNodes("//source[@src]");

                if(hrefNodes != null)
                    foreach(var node in hrefNodes)
                        discoveredLinks.Add(node.GetAttributeValue("href", null));

                if(linkNodes != null)
                    foreach(var node in linkNodes)
                        discoveredLinks.Add(node.GetAttributeValue("href", null));
                
                if(imgNodes != null)
                    foreach(var node in imgNodes)
                        discoveredLinks.Add(node.GetAttributeValue("src", null));

                if(scriptNodes != null)
                    foreach(var node in scriptNodes)
                        discoveredLinks.Add(node.GetAttributeValue("src", null));

                if(sourceNodes != null)
                    foreach(var node in sourceNodes)
                        discoveredLinks.Add(node.GetAttributeValue("src", null));
            }
            
            return discoveredLinks;
        }

        public Dictionary<string, int> GetTerms(string content, List<string> stopwords)
        {
            HtmlDocument doc = new HtmlDocument();
            Dictionary<string, int> termCounts = new Dictionary<string, int>();

            if(content != null)
            {
                doc.LoadHtml(content);

                var pNodes = doc.DocumentNode.SelectNodes("//p/text()");
                termCounts = ProcessNodes(termCounts, pNodes, stopwords);

                var h1Nodes = doc.DocumentNode.SelectNodes("//h1/text()");
                termCounts = ProcessNodes(termCounts, h1Nodes, stopwords);

                var h2Nodes = doc.DocumentNode.SelectNodes("//h2/text()");
                termCounts = ProcessNodes(termCounts, h2Nodes, stopwords);

                var h3Nodes = doc.DocumentNode.SelectNodes("//h3/text()");
                termCounts = ProcessNodes(termCounts, h3Nodes, stopwords);

                var h4Nodes = doc.DocumentNode.SelectNodes("//h4/text()");
                termCounts = ProcessNodes(termCounts, h4Nodes, stopwords);

                var h5Nodes = doc.DocumentNode.SelectNodes("//h5/text()");
                termCounts = ProcessNodes(termCounts, h5Nodes, stopwords);

                var h6Nodes = doc.DocumentNode.SelectNodes("//h6/text()");
                termCounts = ProcessNodes(termCounts, h6Nodes, stopwords);
            }

            return termCounts;
        }

        public Dictionary<string, int> ProcessNodes(Dictionary<string, int> termCounts, HtmlNodeCollection nodes, List<string> stopwords)
        {
            if(nodes != null)
            {
                foreach(var node in nodes)
                {
                    var terms = node.InnerText.Split();

                    foreach(var term in terms)
                    {
                        if(term.Trim() != "")
                        {
                            var nextTerm = term.Trim();
                            nextTerm = new string(nextTerm.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());

                            if(!stopwords.Contains(term.ToLower()) && nextTerm != "")
                            {
                                if(termCounts.ContainsKey(nextTerm))
                                {
                                    termCounts[nextTerm] += 1;
                                }
                                else
                                {
                                    termCounts.Add(nextTerm, 1);
                                }
                            }
                        }
                    }
                }
            }

            return termCounts;
        }

        public Dictionary<string, int> GetPDFTerms(Byte[] data, List<string> stopwords)
        {
            PdfDocument doc = PdfDocument.Open(data);
            Dictionary<string, int> termCounts = new Dictionary<string, int>();

            var pages = doc.GetPages();

            foreach(var page in pages)
            {
                var terms = page.GetWords();

                foreach(var term in terms)
                {
                    if(term.Text.Trim() != "")
                    {
                        var nextTerm = term.Text.Trim();
                        nextTerm = new string(nextTerm.ToCharArray().Where(c => !char.IsPunctuation(c)).ToArray());

                        if(!stopwords.Contains(term.Text.ToLower()) && nextTerm != "")
                        {
                            if(termCounts.ContainsKey(nextTerm))
                            {
                                termCounts[nextTerm] += 1;
                            }
                            else
                            {
                                termCounts.Add(nextTerm, 1);
                            }
                        }
                    }
                }
            }

            return termCounts;
        }

        public string CheckURL(string url)
        {
            Regex httpCheck = new Regex(@"^https?://");
            Regex endpointCheck = new Regex(@"^/.+");

            if (GetURLRoot(url).Contains("engr.uky.edu"))
            {
                if (httpCheck.IsMatch(url) || url.Contains("www"))
                {
                    return CheckForFile(url, "link");
                }
                else
                {
                    return "filtered";
                }
            }
            else if (endpointCheck.IsMatch(url))
            {
                return CheckForFile(url, "resource");
            }
            else
            {
                return "filtered";
            }
        }

        private string CheckForFile(string url, string fallback)
        {
            Regex fileCheck = new Regex(@"\.\w{2,4}($|\?)");

            try
            {
                Uri uri = new Uri(url);

                if(uri.PathAndQuery.Length > 1)
                {
                    var match = fileCheck.Match(url);
                    if(match != null)
                    {
                        string trimmedMatch = match.Value.Trim('?', '.');
                        if (trimmedMatch != "" && trimmedMatch != "html")
                        {
                            return trimmedMatch;
                        }
                        else
                            return fallback;
                    }
                    else
                        return fallback;
                }
                else
                    return fallback;
            }
            catch
            {
                var match = fileCheck.Match(url);
                if(match != null && match.Value != "")
                    return match.Value.Trim('.', '?');
                else
                    return fallback;
            }
        }

        private bool CheckForResource(string url)
        {
            Uri uri = new Uri(url);

            if (uri.PathAndQuery.Length != 0)
                return true;
            else
                return false;
        }

        public string GetURLRoot(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                bool hasWww = uri.Host.Contains("www");
                string root = null;
                root = uri.Scheme + "://" + uri.Host;
                return root;
            }
            catch
            {
                return "";
            }
        }

        public bool CheckForDupedURL(string url, Dictionary<string, DateTime> visitedList)
        {
            Uri uri = new Uri(url);

            var truncatedHost = uri.Host.Replace("www.", "");
            return ((visitedList.ContainsKey("https://" + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("http://" + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("https://www." + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("http://www." + truncatedHost + uri.PathAndQuery)));
        }

        public bool CheckForDupedURL(string url, Dictionary<string, string> visitedList)
        {
            Uri uri = new Uri(url);

            var truncatedHost = uri.Host.Replace("www.", "");
            return ((visitedList.ContainsKey("https://" + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("http://" + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("https://www." + truncatedHost + uri.PathAndQuery))
                    || (visitedList.ContainsKey("http://www." + truncatedHost + uri.PathAndQuery)));
        }

        public bool CheckForDupedURL(string url, Queue<string> frontierQueue)
        {
            Uri uri = new Uri(url);

            var truncatedHost = uri.Host.Replace("www.", "");
            return ((frontierQueue.Contains("https://" + truncatedHost + uri.PathAndQuery))
                    || (frontierQueue.Contains("http://" + truncatedHost + uri.PathAndQuery))
                    || (frontierQueue.Contains("https://www." + truncatedHost + uri.PathAndQuery))
                    || (frontierQueue.Contains("http://www." + truncatedHost + uri.PathAndQuery)));
        }

        public bool CheckForDupedURL(string url, string target)
        {
            Uri uri = new Uri(url);

            var truncatedHost = uri.Host.Replace("www.", "");
            return (target == ("https://" + truncatedHost + uri.PathAndQuery))
                    || (target == ("http://" + truncatedHost + uri.PathAndQuery))
                    || (target == ("https://www." + truncatedHost + uri.PathAndQuery))
                    || (target == ("http://www." + truncatedHost + uri.PathAndQuery));
        }

        public void PrintDictCounts(Dictionary<string, string> dict)
        {
            Dictionary<string, int> itemCounts = new Dictionary<string, int>();

            foreach(var item in dict.Values)
            {
                if (!itemCounts.ContainsKey(item))
                    itemCounts.Add(item, 1);
                else
                    itemCounts[item] += 1;
            }

            foreach(var count in itemCounts)
                Console.WriteLine(count);
        }

        public void PostStartTime(DateTime time)
        {
            string jsonString = "{ \"time\": \"" + time + "\" }";
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var res = (client.PostAsync("http://localhost:8080/starttime", content));
                var result = res.Result;
            }
        }

        public void PostEndTime(DateTime time)
        {
            string jsonString = "{ \"time\": \"" + time + "\" }";
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var res = (client.PostAsync("http://localhost:8080/endtime", content));
                var result = res.Result;
            }
        }

        public void PostPage(string url, DateTime time)
        {
            string jsonString = "{ \"" + url + "\": \"" + time + "\" }";
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var res = (client.PostAsync("http://localhost:8080/page", content));
                var result = res.Result;
            }
        }

        public void PostError(string url, string error)
        {
            string jsonString = "{ \"" + url + "\": \"" + error + "\" }";
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var res = (client.PostAsync("http://localhost:8080/error", content));
                var result = res.Result;
            }
        }

        public void PostFile(string url, string filetype)
        {
            string jsonString = "{ \"" + url + "\": \"" + filetype + "\" }";
            StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                var res = (client.PostAsync("http://localhost:8080/file", content));
                var result = res.Result;
            }
        }
    }
}
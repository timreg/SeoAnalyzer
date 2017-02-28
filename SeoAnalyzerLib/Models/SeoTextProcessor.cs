using HtmlAgilityPack;
using SeoAnalyzerLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    public class SeoTextProcessor : IProcessor
    {
        private ReportOptions currentOptions;
        private List<string> stopWordsList;
        private string currentText;
        private string currentUrl;
        private List<string> metaTags = new List<string>();
        private bool isInited = false;

        public SeoTextProcessor()
        {
            this.currentOptions = new ReportOptions();
            this.currentText = string.Empty;
            this.currentUrl = string.Empty;
        }

        public bool Init(ReportOptions options)
        {
            stopWordsList = new List<string>() { "and", "or", "a", "the","on", "in", "from", "to" };

            currentOptions = options;
            string sourceString = currentOptions.SourceString;
            //1. Validate passed options
            if (String.IsNullOrWhiteSpace(sourceString))
            {
                isInited = false;
                return false;
            }

            sourceString = sourceString.ToLower().Trim();

            //2. Determine text
            if (isUrl(sourceString))
            {
                this.currentUrl = sourceString;
                string urlContent = getUrlContent(sourceString);

                //Make a single line
                urlContent = urlContent.Replace(Environment.NewLine, "").Replace("\n", " ").Replace("\t", " ");

                //Checking for html markup
                if (String.IsNullOrWhiteSpace(urlContent) || !isHtml(urlContent))
                {
                    throw new Exception("Downloaded url content is empty or is not html markup or has wrong encoding!");
                    isInited = false;
                    return false;
                };

                currentText = urlContent;
            }
            else
            {
                //Source string is text
                currentText = sourceString.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\t", " ");
            }
            isInited = true;
            return isInited;
        }

        private string prepareHtml(string htmlText)
        {
            string retVal = string.Empty;



            return retVal;
        }

        public Report Process()
        {
            //Check data init
            if (!this.isInited) return null;

            Report retVal = new Report();
            retVal.SourceData = this.currentOptions;
            Stopwatch swStat = new Stopwatch();
            List<string> metaKeywords = new List<string>();

            swStat.Start();
            if (isHtml(currentText))
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.OptionFixNestedTags = true;
                htmlDoc.LoadHtml(currentText);
                
                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0 && htmlDoc.ParseErrors.Where(f =>f.Code!=HtmlParseErrorCode.EndTagNotRequired).Count()>0)
                {
                    retVal.Error = true;
                    retVal.ResultDescription = "Error while parsing downloaded html from url! " + Environment.NewLine  + string.Join(Environment.NewLine, htmlDoc.ParseErrors.Select(f => f.Reason  + " at " + f.SourceText).ToArray());
                    return retVal;
                }
                else
                {
                    if (htmlDoc.DocumentNode != null)
                    {
                        currentText = getPureBodyText(htmlDoc.DocumentNode);

                        //Define meta tags
                        if (currentOptions.OptCalcOccurMeta)
                        {

                            var nodeList = htmlDoc.DocumentNode.SelectNodes("//meta");
                            foreach (var node in nodeList)
                            {
                                if (node.GetAttributeValue("name", "").ToLower()=="keywords")
                                {
                                    string words = node.GetAttributeValue("content", "");
                                    metaKeywords = words.Split(',')
                                   .Select(f => f.Trim().ToLower()).ToList();
                                }
                            }
                        }

                        //	calculate number of external links in the text.
                        if (currentOptions.OptCalcOccurExtLinks)
                        {
                            //string localDomain = getDomainFromUrl();
                            retVal.NumberExtLinks = getNumberExtLinks(htmlDoc.DocumentNode);
                        }
                    }
                }
            }

            //Leave only alphanumeric characters in text
            Regex rxAlNum = new Regex("[^a-zA-Z0-9 -]");
            currentText = rxAlNum.Replace(currentText, " ");

            //Replacing multiple spaces by single
            currentText = fixSpaces(currentText);

            

            //Filtering stop words           
            if (currentOptions.OptFilterStopWords && stopWordsList.Count > 0)
            {
                currentText = filterStopWords(stopWordsList, currentText);
            }

            //Calculating meta tags in text
            if (currentOptions.OptCalcOccurMeta && metaKeywords.Count() > 0)
            {
                foreach (string keyWord in metaKeywords)
                {
                    Regex rxIncl = new Regex($"(\\s{keyWord}\\s|{keyWord}\\s|\\s{keyWord})", RegexOptions.IgnoreCase);
                    retVal.NumberOccurWordsMeta.Add(keyWord, rxIncl.Matches(currentText).Count);
                }
            }

            //	calculate number of occurrences on the page of each word
            if (currentOptions.OptCalcOccurAll)
            {
                retVal.NumberOccurWordsAll = getNumberWordsAll(currentText);
            }



            swStat.Stop();
            retVal.StatsTime = swStat.Elapsed;

            return retVal;
        }

        internal int getNumberExtLinks(HtmlNode documentNode)
        {
            int retVal = documentNode.SelectNodes("//a").Count();
            return retVal;
        }


        /// <summary>
        /// Replaces two and more spaces by single
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        internal string fixSpaces(string sourceText)
        {
            Regex rxDoubleSpaces = new Regex("\\s{2,}");
            return rxDoubleSpaces.Replace(sourceText, " ").Trim();
        }


        /// <summary>
        /// Removing stop words from text
        /// </summary>
        /// <param name="stopWords"></param>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        internal string filterStopWords(List<string> stopWords, string sourceText)
        {
            string retVal = sourceText;
            foreach (string word in stopWords)
            {
                Regex rxStopWords = new Regex($"(\\s{word}\\s|{word}\\s|\\s{word})", RegexOptions.IgnoreCase);
                retVal = rxStopWords.Replace(retVal, " ");

            }
            retVal = fixSpaces(retVal);
            return retVal;

        }







        #region Helpers

        private Dictionary<string, int> getNumberExtLinks(string currentText)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, int> getNumberWordsMeta(string currentText)
        {
            throw new NotImplementedException();
        }

        private Dictionary<string, int> getNumberWordsAll(string sourceText)
        {
            Dictionary<string, int> retVal = new Dictionary<string, int>();
            string[] wordsAll = sourceText.Split(' ');
            foreach (var word in wordsAll)
            {
                if (retVal.ContainsKey(word))
                {
                    retVal[word]++;
                }
                else
                {
                    retVal.Add(word, 1);
                }
            }
            return retVal;
        }

        private List<string> getMetaTags(string currentText)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Getting body text and removing all unnesessary nodes and symbols
        /// </summary>
        /// <param name="documentNode">Document node</param>
        /// <returns></returns>
        internal string getPureBodyText(HtmlNode documentNode)
        {
            string retVal = string.Empty;
            HtmlDocument sourceDoc = documentNode.OwnerDocument;

            //Removing scripts, styles nodes and comments
            documentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.NodeType == HtmlNodeType.Comment || n.NodeType == HtmlNodeType.Comment)
                .ToList()
                .ForEach(n => n.Remove());

            Regex rxHtmlEscapeSymbols = new Regex("&\\#*[a-zA-Z0-9]+;");
            retVal = rxHtmlEscapeSymbols.Replace(documentNode.InnerText, " ");
            retVal = fixSpaces(retVal);

            //Encoding to utf8 if needed?
            //Encoding encUtf8 = new UTF8Encoding();
            //byte[] encodedBytes = Encoding.Convert(sourceDoc.Encoding, encUtf8, encUtf8.GetBytes(documentNode.InnerText));
            //retVal = encUtf8.GetString(encodedBytes);

            return retVal;
        }

        /// <summary>
        /// Check input string for url
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        internal static bool isUrl(string sourceString)
        {
            //variants: url without root domain, user and pass in url etc...
            Regex rxUrl = new Regex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{0,10})([\/\S \.-]*)*\/?$", RegexOptions.IgnoreCase & RegexOptions.Singleline); //simple
            return rxUrl.IsMatch(sourceString);
        }

        internal static bool isHtml(string sourceString)
        {
            Regex rxHtml = new Regex(@".*<html.*\/html>", RegexOptions.IgnoreCase & RegexOptions.Singleline); //simple
            return rxHtml.IsMatch(sourceString);
        }

        internal static string getDomainFromUrl(string url)
        {
            Regex rxUrl = new Regex(@"^((http[s]?):\/\/).+$"); //simple
            return (rxUrl.Match(url)).Value;
        }

        internal static string getUrlContent(string url)
        {
            string retVal = string.Empty;

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                retVal = client.DownloadString(url);
            }

            return retVal;
        }

        #endregion


    }
}

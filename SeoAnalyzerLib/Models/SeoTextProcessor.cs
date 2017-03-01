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
    /// <summary>
    /// Main text analyzing class. Performs analyzing by passed options.
    /// </summary>
    public class SeoTextProcessor : IProcessor
    {
        private ReportOptions currentOptions;
        private List<string> stopWordsList;
        private string currentText;
        private string currentUrl;
        private bool isInited = false;

        #region Ctor
        public SeoTextProcessor()
        {
            this.currentOptions = new ReportOptions();
            this.currentText = string.Empty;
            this.currentUrl = string.Empty;
        } 
        #endregion

        #region Interface methods
        /// <summary>
        /// Init processor by passed options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Init(ReportOptions options)
        {
            stopWordsList = new List<string>() { "and", "or", "a", "the", "on", "in", "from", "to", "of", "is", "are", "there", "it", "by", "that", "but", "this" };

            currentOptions = options;
            string sourceString = currentOptions.SourceString;
            
            //1. Validate some passed options
            if (String.IsNullOrWhiteSpace(sourceString))
            {
                isInited = false;
                return false;
            }

            sourceString = sourceString.Trim();

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
                };


                currentText = urlContent.ToLower().Trim();
            }
            else
            {
                //Source string is text
                currentText = sourceString.ToLower().Trim().Replace(Environment.NewLine, " ").Replace("\n", "").Replace("\t", " ");
            }
            isInited = true;
            return isInited;
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

                if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0 && htmlDoc.ParseErrors.Where(f => f.Code != HtmlParseErrorCode.EndTagNotRequired).Count() > 0)
                {
                    retVal.Error = true;
                    retVal.ResultDescription = "Error while parsing downloaded html from url! " + Environment.NewLine + string.Join(Environment.NewLine, htmlDoc.ParseErrors.Select(f => f.Reason + " at " + f.SourceText).ToArray());
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
                                //For variant when only meta keywords used
                                //if (node.GetAttributeValue("name", "").ToLower() == "keywords")
                                //{
                                string words = node.GetAttributeValue("content", "");
                                //metaKeywords = words.Split(',').Select(f => f.Trim().ToLower()).ToList();
                                metaKeywords.AddRange(
                                    words.Replace(",", "")
                                    .Replace(".", "")
                                    .Split(' ')
                                    .Where(f => f.Length > 2 && f.Length < 15 && !f.StartsWith("http"))
                                    .Select(f => f.Trim()
                                    .ToLower())
                                    .ToList());
                                //}
                            }
                            metaKeywords = metaKeywords.Distinct().ToList();

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

            //Leave only alphanumeric ENGLISH ONLY (task requirement) characters in text
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
                    Regex rxIncl = new Regex(keyWord, RegexOptions.IgnoreCase);
                    if (!retVal.NumberOccurWordsMeta.ContainsKey(keyWord))
                    {
                        retVal.NumberOccurWordsMeta.Add(keyWord, rxIncl.Matches(currentText).Count);
                    }
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
        #endregion

        #region Helpers

        /// <summary>
        /// Returns count of links found in text with some filtrations
        /// </summary>
        /// <param name="documentNode"></param>
        /// <returns></returns>
        internal int getNumberExtLinks(HtmlNode documentNode)
        {
            int retVal = documentNode.SelectNodes("//a").Where(f => f.Attributes["href"] != null ? f.Attributes["href"].Value.StartsWith("http") : false).Count();
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
            string regexPattern = $"\\b(?:{string.Join("|", stopWords)})\\b\\s*";
            Regex rxStopWords = new Regex(regexPattern, RegexOptions.IgnoreCase);
            retVal = rxStopWords.Replace(retVal, "");
            retVal = fixSpaces(retVal);
            return retVal;

        }

        /// <summary>
        /// Calculate each word count in text
        /// </summary>
        /// <param name="sourceText"></param>
        /// <returns></returns>
        private Dictionary<string, int> getNumberWordsAll(string sourceText)
        {
            Dictionary<string, int> retVal = new Dictionary<string, int>();
            Regex rxNumeric = new Regex("^\\d*(\\.|\\,)*\\d*$");
            string[] wordsAll = sourceText.Split(' ');
            foreach (var word in wordsAll)
            {
                if (rxNumeric.IsMatch(word) || word.Length <= 2) continue;
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
        internal bool isUrl(string sourceString)
        {
            //variants: url without root domain, user and pass in url etc...
            Regex rxUrl = new Regex(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{0,10})([\/\S \.-]*)*\/?$", RegexOptions.IgnoreCase & RegexOptions.Singleline); //simple
            return rxUrl.IsMatch(sourceString);
        }

        /// <summary>
        /// Check string for html markup
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        internal bool isHtml(string sourceString)
        {
            Regex rxHtml = new Regex(@".*<html.*\/html>", RegexOptions.IgnoreCase & RegexOptions.Singleline); //simple
            return rxHtml.IsMatch(sourceString);
        }

        /// <summary>
        /// Getging document content by url
        /// </summary>
        /// <param name="url">URL to document</param>
        /// <returns></returns>
        internal string getUrlContent(string url)
        {
            string retVal = string.Empty;

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;

                //TODO: non-standart encoding problem not solved!
                retVal = client.DownloadString(url);
            }

            return retVal;
        }

        #endregion


    }
}

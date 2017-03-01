using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeoAnalyzerLib.Models;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace SeoAnalyzerTest
{
    /// <summary>
    /// Some simple tests
    /// </summary>
    [TestClass]
    public class TestProcessor
    {
        private ReportOptions Options;

        [TestInitialize]
        public void Setup()
        {
            Options = new ReportOptions()
            {
                OptFilterStopWords = true,
                OptCalcOccurAll = true,
                OptCalcOccurMeta = true,
                OptCalcOccurExtLinks = true
            };
        }

        [TestMethod]
        public void TestInit()
        {
            Options.SourceString = "asdf sdaf sdf sdf dsf sdfsf";
            SeoTextProcessor prc = new SeoTextProcessor();
            prc.Init(this.Options);

            Report report = prc.Process();

            Assert.IsNotNull(report);
        }


        [TestMethod]
        public void TestDoProcessText()
        {
            Options.SourceString = "asdf sdaf sdf sdf dsf sdfsf";
            SeoTextProcessor prc = new SeoTextProcessor();
            prc.Init(this.Options);

            Report report = prc.Process();

            Assert.IsNotNull(report);
        }
        [TestMethod]
        public void TestDoProcessUrl()
        {
            Options.SourceString = "https://www.w3schools.com/xml/xpath_syntax.asp";
            SeoTextProcessor prc = new SeoTextProcessor();
            prc.Init(this.Options);

            Report report = prc.Process();

            Assert.IsNotNull(report);
        }

        [TestMethod]
        public void TestIsUrl()
        {
            Assert.IsTrue((new SeoTextProcessor()).isUrl("http://example.com"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("https://example.com"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("http://example.com:80/pages/page"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("https://example.com:80/pages/page?id=123"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("https://example.com:80/pages/page?id=123&view=1"));

            Assert.IsTrue((new SeoTextProcessor()).isUrl("example.com"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("example.com/someurl"));
            Assert.IsTrue((new SeoTextProcessor()).isUrl("example.com?id=123&a=b"));


            Assert.IsFalse((new SeoTextProcessor()).isUrl("text text"));
            Assert.IsFalse((new SeoTextProcessor()).isUrl("text http://localhost"));
            Assert.IsFalse((new SeoTextProcessor()).isUrl("http://localhost text"));
            Assert.IsFalse((new SeoTextProcessor()).isUrl(""));
            Assert.IsFalse((new SeoTextProcessor()).isUrl("    "));
        }

        [TestMethod]
        public void TestIsHtml()
        {
            Assert.IsTrue((new SeoTextProcessor()).isHtml("<!DOCTYPE html><html itemscope itemtype = \"http://schema.org/QAPage\" ><body>some text</body></html>"));
            Assert.IsTrue((new SeoTextProcessor()).isHtml("<html></html>"));
            Assert.IsTrue((new SeoTextProcessor()).isHtml("<html><body></body></html>"));

            Assert.IsFalse((new SeoTextProcessor()).isHtml("text text"));
            Assert.IsFalse((new SeoTextProcessor()).isHtml("html text"));
            Assert.IsFalse((new SeoTextProcessor()).isHtml(""));
            Assert.IsFalse((new SeoTextProcessor()).isHtml("    "));
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void TestGetUrlContentFails()
        {
            string retVal = string.Empty;
            retVal = (new SeoTextProcessor()).getUrlContent("sometext");
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void TestGetUrlContentFails2()
        {
            string retVal = string.Empty;
            retVal = (new SeoTextProcessor()).getUrlContent("http://not-existing-domain.com/page");
        }

        [TestMethod]
        public void TestGetUrlContentSuccess()
        {
            string retVal = string.Empty;
            Assert.IsTrue((new SeoTextProcessor()).getUrlContent("http://example.com").Length > 0);
            Assert.IsTrue((new SeoTextProcessor()).getUrlContent("https://example.com").Length > 0);
        }

        [TestMethod]
        public void TestGetPureBodyText()
        {
            string text = (new SeoTextProcessor()).getPureBodyText(this.getDocNodeFromUrl("https://ru.aliexpress.com/item/100pcs-M5-M5x10x0-8-Brass-Flat-Gasket-Copper-Plain-Washer/32708447448.html?aff_platform=aaf&cpt=1488153438533&sk=VnYZvQVf&aff_trace_key=05d640bf3fb047588aacd9f1e315b827-1488153438533-06632-VnYZvQVf"));
            Assert.IsTrue(text.Length > 0);
        }
        [TestMethod]
        public void TestGetNumberExtLinks()
        {
            int count1 = (new SeoTextProcessor()).getNumberExtLinks(this.getDocNodeFromUrl("https://ru.aliexpress.com/item/100pcs-M5-M5x10x0-8-Brass-Flat-Gasket-Copper-Plain-Washer/32708447448.html?aff_platform=aaf&cpt=1488153438533&sk=VnYZvQVf&aff_trace_key=05d640bf3fb047588aacd9f1e315b827-1488153438533-06632-VnYZvQVf"));
            Assert.IsTrue(count1 > 0);

            int count2 = (new SeoTextProcessor()).getNumberExtLinks(this.getDocNodeFromUrl("https://example.com"));
            Assert.AreEqual(1, count2);
        }

        [TestMethod]
        public void TestFilterStopWords()
        {
            List<string> stopWords = new List<string> { "a", "or", "not", "at", "in", "this", "an" };
            Assert.AreEqual("new text sample test integer interest", (new SeoTextProcessor()).filterStopWords(stopWords, "A new text sample in this test integer interest"));
            Assert.AreEqual("original internet", (new SeoTextProcessor()).filterStopWords(stopWords, "or original in an internet"));

        }

        [TestMethod]
        public void TestFixSpaces()
        {
            Assert.AreEqual("normal spaces", (new SeoTextProcessor()).fixSpaces("normal spaces"));
            Assert.AreEqual("before and after", (new SeoTextProcessor()).fixSpaces("   before and after   "));
            Assert.AreEqual("more spaces", (new SeoTextProcessor()).fixSpaces("more    spaces"));
        }

        private HtmlNode getDocNodeFromUrl(string url)
        {
            string urlContent = string.Empty;
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                urlContent = wc.DownloadString(url);
            }

            urlContent = urlContent
                .Replace(Environment.NewLine, " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                ;

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.LoadHtml(urlContent);
            return htmlDoc.DocumentNode.SelectSingleNode("//body");

        }
    }
}

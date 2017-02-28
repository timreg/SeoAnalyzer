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
            Assert.IsTrue(SeoTextProcessor.isUrl("http://example.com"));
            Assert.IsTrue(SeoTextProcessor.isUrl("https://example.com"));
            Assert.IsTrue(SeoTextProcessor.isUrl("http://example.com:80/pages/page"));
            Assert.IsTrue(SeoTextProcessor.isUrl("https://example.com:80/pages/page?id=123"));
            Assert.IsTrue(SeoTextProcessor.isUrl("https://example.com:80/pages/page?id=123&view=1"));

            Assert.IsTrue(SeoTextProcessor.isUrl("example.com"));
            Assert.IsTrue(SeoTextProcessor.isUrl("example.com/someurl"));
            Assert.IsTrue(SeoTextProcessor.isUrl("example.com?id=123&a=b"));


            Assert.IsFalse(SeoTextProcessor.isUrl("text text"));
            Assert.IsFalse(SeoTextProcessor.isUrl("text http://localhost"));
            Assert.IsFalse(SeoTextProcessor.isUrl("http://localhost text"));
            Assert.IsFalse(SeoTextProcessor.isUrl(""));
            Assert.IsFalse(SeoTextProcessor.isUrl("    "));
        }

        [TestMethod]
        public void TestIsHtml()
        {
            Assert.IsTrue(SeoTextProcessor.isHtml("<!DOCTYPE html><html itemscope itemtype = \"http://schema.org/QAPage\" ><body>some text</body></html>"));
            Assert.IsTrue(SeoTextProcessor.isHtml("<html></html>"));
            Assert.IsTrue(SeoTextProcessor.isHtml("<html><body></body></html>"));

            Assert.IsFalse(SeoTextProcessor.isHtml("text text"));
            Assert.IsFalse(SeoTextProcessor.isHtml("html text"));
            Assert.IsFalse(SeoTextProcessor.isHtml(""));
            Assert.IsFalse(SeoTextProcessor.isHtml("    "));
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void TestGetUrlContentFails()
        {
            string retVal = string.Empty;
            retVal = SeoTextProcessor.getUrlContent("sometext");
        }

        [TestMethod]
        [ExpectedException(typeof(WebException))]
        public void TestGetUrlContentFails2()
        {
            string retVal = string.Empty;
            retVal = SeoTextProcessor.getUrlContent("http://not-existing-domain.com/page");
        }

        [TestMethod]
        public void TestGetUrlContentSuccess()
        {
            string retVal = string.Empty;
            Assert.IsTrue(SeoTextProcessor.getUrlContent("http://example.com").Length > 0);
            Assert.IsTrue(SeoTextProcessor.getUrlContent("https://example.com").Length > 0);
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
            List<string> stopWords = new List<string> {"a", "or","not","at","in","this" };
            Assert.AreEqual("new text sample test",(new SeoTextProcessor()).filterStopWords(stopWords, "A new text sample in this test"));
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeoAnalyzerLib.Models;
using System.Collections.Generic;

namespace SeoAnalyzerTest
{
    [TestClass]
    public class TestProcessor
    {
        private Setup Options;
        [TestInitialize]
        public void Setup()
        {
            Options = new Setup()
            {
                StopWords = new List<string>() { "and", "or", "a", "the" },
                OptFilterStopWords = true,
                OptCalcOccurAll = true,
                OptCalcOccurMeta = true,
                OptCalcOccurExtLinks = true
            };
        }


        [TestMethod]
        public void TestDoProcess()
        {
            Options.SourceString = "asdf sdaf sdf sdf dsf sdfsf";
            Processor prc = new Processor();
            prc.Init(this.Options);

            Report report = prc.DoProcess();

            Assert.IsNotNull(report);
        }
    }
}

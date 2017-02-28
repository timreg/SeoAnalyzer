﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    public class Report
    {
        public Dictionary<string, int> NumberOccurWordsAll;
        public Dictionary<string, int> NumberOccurWordsMeta;
        public int NumberExtLinks;
        public TimeSpan StatsTime;
        public ReportOptions SourceData;
        public bool Error;
        public string ResultDescription;

        public Report()
        {
            SourceData = new ReportOptions();
            Error = false;
            ResultDescription = string.Empty;
            NumberOccurWordsAll = new Dictionary<string, int>();
            NumberOccurWordsMeta = new Dictionary<string, int>();
            NumberExtLinks = 0;
        }
    }
}

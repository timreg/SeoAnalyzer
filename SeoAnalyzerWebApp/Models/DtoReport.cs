using SeoAnalyzerWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerWebApp.Models
{

    public class DtoReport
    {
        public Dictionary<string, int> NumberOccurWordsAll;
        public Dictionary<string, int> NumberOccurWordsMeta;
        public int NumberExtLinks;
        public TimeSpan StatsTime;
        public DtoReportOptions SourceData;
        public bool Error;
        public string ResultDescription;

        public DtoReport()
        {
            SourceData = new DtoReportOptions();
            Error = false;
            ResultDescription = string.Empty;
            NumberOccurWordsAll = new Dictionary<string, int>();
            NumberOccurWordsMeta = new Dictionary<string, int>();
            NumberExtLinks = 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    public class ReportOptions
    {
        public string SourceString { get; set; }
        public bool OptFilterStopWords { get; set; }
        public bool OptCalcOccurAll { get; set; }
        public bool OptCalcOccurMeta { get; set; }
        public bool OptCalcOccurExtLinks { get; set; }
    }
}

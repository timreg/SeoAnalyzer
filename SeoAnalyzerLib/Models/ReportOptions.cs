using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    /// <summary>
    /// Contains defined options for text analyzing
    /// </summary>
    public class ReportOptions
    {
        public string SourceString { get; set; }
        public bool OptFilterStopWords { get; set; }
        public bool OptCalcOccurAll { get; set; }
        public bool OptCalcOccurMeta { get; set; }
        public bool OptCalcOccurExtLinks { get; set; }

        public ReportOptions()
        {
            this.OptCalcOccurAll = false;
            this.OptCalcOccurExtLinks = false;
            this.OptCalcOccurMeta = false;
            this.OptFilterStopWords = false;
        }
    }
}

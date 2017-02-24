using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    public class Setup
    {
        public string SourceString;
        public List<string> StopWords;
        public bool OptFilterStopWords;
        public bool OptCalcOccurAll;
        public bool OptCalcOccurMeta;
        public bool OptCalcOccurExtLinks;
    }
}

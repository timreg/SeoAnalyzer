using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerWebApp.Models
{
    /// <summary>
    /// Data transfer object for options form
    /// </summary>
    public class DtoReportOptions
    {
        [Required(ErrorMessage ="Empty string is not allowed!")]
        public string SourceString { get; set; }
        public List<string> StopWordsList { get; set; }
        public bool OptFilterStopWords { get; set; }
        public bool OptCalcOccurAll { get; set; }
        public bool OptCalcOccurMeta { get; set; }
        public bool OptCalcOccurExtLinks { get; set; }

        
    }
}

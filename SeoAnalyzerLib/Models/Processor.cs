using SeoAnalyzerLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Models
{
    public class Processor : IProcessor
    {
        private Setup options;
        public Report DoProcess()
        {
            return new Report();
        }

        public bool Init(Setup options)
        {
            bool retVal = false;
            this.options = options;


            return retVal;
        }
    }
}

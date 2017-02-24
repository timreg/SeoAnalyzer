using SeoAnalyzerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeoAnalyzerLib.Interfaces
{
    public interface IProcessor
    {
        bool Init(Setup options);
        Report DoProcess();

    }
}

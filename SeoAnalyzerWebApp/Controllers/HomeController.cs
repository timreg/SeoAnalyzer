using SeoAnalyzerLib.Models;
using SeoAnalyzerWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SeoAnalyzerWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetReport(DtoReportOptions dtoOptions)
        {
            SeoTextProcessor processor = new SeoTextProcessor();
            Report report = new Report();
            ReportOptions options = new ReportOptions()
            {
                SourceString = dtoOptions.SourceString
                ,
                OptCalcOccurAll = dtoOptions.OptCalcOccurAll
                ,
                OptCalcOccurExtLinks = dtoOptions.OptCalcOccurExtLinks
                ,
                OptCalcOccurMeta = dtoOptions.OptCalcOccurMeta
                ,
                OptFilterStopWords = dtoOptions.OptFilterStopWords
            };
            try
            {
                bool initResult = processor.Init(options);
                if (!initResult)
                {
                    ViewBag.Message = "Error while initing input data! Check parameters and try again.";
                    return PartialView("Error");
                }

                report = processor.Process();
                if (report.Error)
                {
                    ViewBag.Message = report.ResultDescription;
                    return PartialView("Error");
                }
                return PartialView("Report", report);
            }
            catch (Exception ex)
            {
                
                return PartialView("Error",ex);
            }
        }

    }
}
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

        /// <summary>
        /// Returns AJAX data as report
        /// </summary>
        /// <param name="dtoOptions"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetReport(DtoReportOptions dtoOptions)
        {
            if (string.IsNullOrWhiteSpace(dtoOptions.SourceString)
                || (dtoOptions.OptCalcOccurAll | dtoOptions.OptCalcOccurExtLinks | dtoOptions.OptCalcOccurMeta | dtoOptions.OptFilterStopWords) == false)
            {
                ViewBag.Message = "Some parameters is not set!";
                return PartialView("_ErrorPartial");
            }

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
                    return PartialView("_ErrorPartial");
                }

                report = processor.Process();
                if (report.Error)
                {
                    ViewBag.Message = report.ResultDescription;
                    return PartialView("_ErrorPartial");
                }

                if (Session["SeoReport"] == null)
                {
                    Session.Add("SeoReport", report);
                }
                else
                {
                    Session["SeoReport"] = report;
                }
                return PartialView("_ReportPartial", report);
            }
            catch (Exception ex)
            {
                return PartialView("_ErrorPartial", ex);
            }
        }

        /// <summary>
        /// Return AJAX data report for sorting report
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SortResult()
        {
            Report report = new Report();
            if (Session["SeoReport"] != null)
            {
                report = (Report)Session["SeoReport"];
                string target = Request["target"].ToLower();
                string field = Request["field"].ToLower();
                string dir = Request["direction"].ToLower();

                switch (target)
                {
                    case "all":
                        report.NumberOccurWordsAll = sortDictionary(report.NumberOccurWordsAll, field, dir);
                        break;
                    case "meta":
                        report.NumberOccurWordsMeta = sortDictionary(report.NumberOccurWordsMeta, field, dir);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                ViewBag.Message = "Report not found in session cache. Session may be expired. Reenter data and analyze again.";
                return PartialView("_ErrorPartial");
            }
            return PartialView("_ReportPartial", report);
        }


        #region Private methods
        private static Dictionary<string, int> sortDictionary(Dictionary<string, int> source, string field, string direction)
        {
            IEnumerable<KeyValuePair<string, int>> retVal;
            switch (field)
            {
                case "word":
                    retVal = (direction == "asc") ? source.OrderBy(f => f.Key) : source.OrderByDescending(f => f.Key);
                    break;
                case "count":
                    retVal = (direction == "asc") ? source.OrderBy(f => f.Value) : source.OrderByDescending(f => f.Value);
                    break;
                default:
                    retVal = null;
                    break;
            }
            return retVal.ToDictionary(k => k.Key, v => v.Value);
        }
    }
    #endregion
}
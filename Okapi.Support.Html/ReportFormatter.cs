using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using Okapi.Common;
using Okapi.Extensions;
using Okapi.Report;
using Okapi.Utils;

namespace Okapi.Support.Html
{
    public class ReportFormatter : IReportFormatter
    {
        private readonly string mainReportFileName;
        private readonly string reportDirectory;
        private Session session = null;
        private const string okapiSessionIdAttribute = "okapiSessionId";

        public ReportFormatter()
        {
            reportDirectory = Session.Instance.ReportDirectory;
            mainReportFileName = $"{reportDirectory}{Path.DirectorySeparatorChar}OkapiReport_{Util.SessionStartDateTime.GetTimestamp()}.html";
            session = Session.Instance;
        }

        private void CreateReport(HtmlTextWriter writer, TestCase testCase)
        {
            writer.AddAttribute(okapiSessionIdAttribute, $"{session.Id}");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "row100");
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            List<string> testCaseDataItems = new List<string>
            {
                testCase.Method.Name,
                testCase.Result.ToString(),
                testCase.DurationInSeconds.ToString(),
                testCase.StartDateTime.ToString(),
                testCase.EndDateTime.ToString()
            };

            for (int i = 0; i < testCaseDataItems.Count; i++)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, $"column100 column{i + 1}");
                writer.AddAttribute("data-column", $"column{i + 1}");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.Write(testCaseDataItems[i]);
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        public void Run(TestCase testCase)
        {
            string reportTemplateName = "index.html";
            string existingReportData = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            string reportTemplateContent = Util.GetEmbeddedResource(assembly, reportTemplateName);
            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                CreateReport(writer, testCase);
            }

            string newReportData = stringWriter.ToString();

            if (session.ReportingInProgress)
            {
                string currentReportContent = Util.ReadFile(mainReportFileName);
                existingReportData = GetExistingData(currentReportContent);
            }
            else
            {
                session.ReportingInProgress = true;
                WriteEmbeddedResourceFilesToReportFolder(assembly, reportTemplateName);
            }

            string reportContent = reportTemplateContent.Replace("{testcases}", $"{existingReportData}{newReportData}");
            Util.WriteToFile(mainReportFileName, true, reportContent);

            //if (testCase.AllAdditionalData.HasAny())
            //{
            //    writer.Write(writer.NewLine);
            //    writer.RenderBeginTag(HtmlTextWriterTag.Span);
            //    writer.Write($"ADDITIONAL DATA: {testCase.AllAdditionalData.ConvertToString()}");
            //    writer.RenderEndTag();
            //}

            //if (testCase.FailAdditionalData.HasAny())
            //{
            //    NewLineAndTab(reportStringBuilder);
            //    reportStringBuilder.Append($"FAIL ADDITIONAL DATA: {testCase.FailAdditionalData.ConvertToString()}");
            //}

            //if (testCase.TestObjectInfo != null)
            //{
            //    NewLineAndTab(reportStringBuilder);
            //    reportStringBuilder.Append($"TEST OBJECT INFO: {testCase.TestObjectInfo}");
            //}

            //if (testCase.Exception != null)
            //{
            //    NewLineAndTab(reportStringBuilder);
            //    reportStringBuilder.Append($"EXCEPTION: {testCase.Exception}");
            //}

            //IList<TestStep> testSteps = testCase.TestSteps;

            //if (testSteps.HasAny())
            //{
            //    testSteps.ToList().ForEach(x =>
            //    {
            //        NewLineAndTab(reportStringBuilder);
            //        NewLineAndTab(reportStringBuilder);

            //        reportStringBuilder.Append($"STEP: {x.Method.Name}");

            //        NewLineAndTab(reportStringBuilder);
            //        reportStringBuilder.Append($"RESULT: {x.Result}");

            //        if (x.DurationInSeconds > 0)
            //        {
            //            NewLineAndTab(reportStringBuilder);
            //            reportStringBuilder.Append($"DURATION: {x.DurationInSeconds} seconds");
            //        }

            //        NewLineAndTab(reportStringBuilder);
            //        reportStringBuilder.Append($"START TIME: {x.StartDateTime}");

            //        NewLineAndTab(reportStringBuilder);
            //        reportStringBuilder.Append($"END TIME: {x.EndDateTime}");

            //        if (x.AllAdditionalData.HasAny())
            //        {
            //            NewLineAndTab(reportStringBuilder);
            //            reportStringBuilder.Append($"ADDITIONAL DATA: {x.AllAdditionalData.ConvertToString()}");
            //        }

            //        if (x.FailAdditionalData.HasAny())
            //        {
            //            NewLineAndTab(reportStringBuilder);
            //            reportStringBuilder.Append($"FAIL ADDITIONAL DATA: {x.FailAdditionalData.ConvertToString()}");
            //        }

            //        if (x.TestObjectInfo != null)
            //        {
            //            NewLineAndTab(reportStringBuilder);
            //            reportStringBuilder.Append($"TEST OBJECT INFO: {x.TestObjectInfo}");
            //        }

            //        if (x.Exception != null)
            //        {
            //            NewLineAndTab(reportStringBuilder);
            //            reportStringBuilder.Append($"EXCEPTION: {x.Exception}");
            //        }
            //    });
            //}

            //NewLineAndTab(reportStringBuilder);
            //NewLineAndTab(reportStringBuilder);

            //if (testCase.Result.Equals(TestResult.PASS))
            //{
            //    logger.Information(reportStringBuilder.ToString());
            //}
            //else
            //{
            //    logger.Error(reportStringBuilder.ToString());
            //}
        }

        private void WriteEmbeddedResourceFilesToReportFolder(Assembly assembly, string excludeName = null)
        {
            IList<string> fullResourceNames = Util.GetAllResourceFullFileNames(assembly);

            if (fullResourceNames.HasAny())
            {
                fullResourceNames.ToList().ForEach(x =>
                {
                    string[] nameParts = x.Split('.');
                    int len = nameParts.Length;
                    string fileName = $"{nameParts[len - 2]}.{nameParts[len - 1]}";

                    if (!string.IsNullOrWhiteSpace(x))
                    {
                        if (excludeName == null || (excludeName != null && !fileName.Equals(excludeName)))
                        {
                            WriteEmbeddedResourceFileToReportFolder(assembly, x);
                        }
                    }
                });
            }
        }

        private void WriteEmbeddedResourceFileToReportFolder(Assembly assembly, string fullResourceName)
        {
            string[] nameParts = fullResourceName.Split('.');
            int len = nameParts.Length;
            string fileName = $"{nameParts[len - 2]}.{nameParts[len - 1]}";
            string folder = reportDirectory;

            for (int i = 4; i < len - 2; i++)
            {
                folder = $"{folder}{Path.DirectorySeparatorChar}{nameParts[i]}";
            }

            Util.WriteEmbeddedResourceFileToFolder(assembly, fullResourceName, folder, fileName);
        }

        private string GetExistingData(string currentReportContent)
        {
            var dataFirstIndex = currentReportContent.IndexOf($"<tr {okapiSessionIdAttribute}=\"{session.Id}\"");
            string currentData = currentReportContent.Substring(dataFirstIndex).Trim();
            var dataLastIndex = currentData.LastIndexOf("</tr>");
            return currentData.Substring(0, dataLastIndex + 5).Trim();
        }
    }
}
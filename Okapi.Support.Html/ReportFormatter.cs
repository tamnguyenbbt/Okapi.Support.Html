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
        private readonly string reportPath;
        private readonly string detailedReportPath;
        private readonly string reportDirectory;
        private Session session = null;
        private const string okapiSessionIdAttribute = "okapiSessionId";

        public ReportFormatter()
        {
            reportDirectory = Session.Instance.ReportDirectory;
            detailedReportPath = $"{reportDirectory}{Path.DirectorySeparatorChar}OkapiReport_{Util.SessionStartDateTime.GetTimestamp()}";
            reportPath = $"{detailedReportPath}.html";
            session = Session.Instance;
        }

        public void Run(TestCase testCase)
        {
            string reportTemplateName = "report.html";
            string detailedReportTemplateName = "test-case-detailed-report.html";
            string existingReportData = null;

            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> allResourceFullFileNames = assembly.GetAllResourceFullFileNames().ToList();

            string reportTemplateFullName = assembly.GetResourceFullFileName(reportTemplateName, "html", "table-highlight-vertical");
            string reportTemplateContent = assembly.GetResourceContent(reportTemplateFullName);

            string detailedReportTemplateFullName = assembly.GetResourceFullFileName(detailedReportTemplateName, "html", "table-fixed-column");
            string detailedReportTemplateContent = assembly.GetResourceContent(detailedReportTemplateFullName);

            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                BuildReportData(writer, testCase);
            }

            string newReportData = stringWriter.ToString();

            if (session.ReportingInProgress)
            {
                string currentReportContent = Util.ReadFile(reportPath);
                existingReportData = GetExistingReportData(currentReportContent);
            }
            else
            {
                session.ReportingInProgress = true;

                if (allResourceFullFileNames.HasAny())
                {
                    var copyingResourceFullFileNames = allResourceFullFileNames.Where(x => !x.Equals(reportTemplateFullName));

                    if (copyingResourceFullFileNames.HasAny())
                    {
                        copyingResourceFullFileNames.ToList().ForEach(x =>
                        {
                            CopyEmbeddedResourceFileToReportFolder(assembly, x);
                        });
                    }
                }
            }

            string reportContent = reportTemplateContent.Replace("{testcases}", $"{existingReportData}{newReportData}");
            Util.WriteToFile(reportPath, true, reportContent);

            //TO DO: 
            Util.WriteToFile($"{detailedReportPath}{Path.DirectorySeparatorChar}{testCase.Method.Name}.html", true, detailedReportTemplateContent);
        }

        private void CopyEmbeddedResourceFileToReportFolder(Assembly assembly, string resourceFullFileName)
        {
            if (!string.IsNullOrWhiteSpace(resourceFullFileName) && assembly != null)
            {
                string assemblyName = assembly.GetName().Name;
                string path = resourceFullFileName.Substring(assemblyName.Length + 1);
                string[] parts = path?.Split('.');
                string destinationDirectory = reportDirectory;
                string destinationFileName = resourceFullFileName;

                if (parts.HasAny())
                {
                    destinationFileName = parts.Length > 1 ? $"{parts[parts.Length - 2]}.{parts[parts.Length - 1]}" : parts[parts.Length - 1];
                    int directoryPartCount = parts.Length > 2 ? parts.Length - 2 : 0;

                    if (directoryPartCount > 0)
                    {
                        for (int i = 0; i < directoryPartCount; i++)
                        {
                            destinationDirectory = $"{destinationDirectory}{Path.DirectorySeparatorChar}{parts[i]}";
                        }
                    }
                }

                assembly.CopyResourceFileToFolder(resourceFullFileName, destinationDirectory, destinationFileName);
            }
        }

        private void BuildReportData(HtmlTextWriter writer, TestCase testCase)
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

        private void BuildDetailedReportData(HtmlTextWriter writer, TestCase testCase)
        {
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

        private string GetExistingReportData(string currentReportContent)
        {
            var dataFirstIndex = currentReportContent.IndexOf($"<tr {okapiSessionIdAttribute}=\"{session.Id}\"");
            string currentData = currentReportContent.Substring(dataFirstIndex).Trim();
            var dataLastIndex = currentData.LastIndexOf("</tr>");
            return currentData.Substring(0, dataLastIndex + 5).Trim();
        }
    }
}

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
        private readonly string detailedReportDirectory;
        private readonly string reportDirectory;
        private Session session = null;
        private const string okapiSessionIdAttribute = "okapiSessionId";
        private const string summaryReportTemplateName = "report.html";
        private const string detailedReportTemplateName = "test-case-detailed-report.html";
        private const string verticalTableFolderName = "table-highlight-vertical";
        private const string htmlFolderName = "html";
        private readonly bool firstTimeReporting = true;

        public ReportFormatter()
        {
            session = Session.Instance;
            reportDirectory = session.ReportDirectory;
            detailedReportDirectory = $"{reportDirectory}{Path.DirectorySeparatorChar}OkapiReport_{session.StartDateTime.GetTimestamp()}";
            reportPath = $"{detailedReportDirectory}.html";
            firstTimeReporting = !session.ReportingInProgress;
        }

        public void Run(TestCase testCase)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string mainReportTemplateFullName = WriteMainReport(assembly, testCase);
            WriteTestCaseDetailedReport(assembly, testCase);
            CopyEmbeddedHtmlResourcesToReportFolder(assembly, mainReportTemplateFullName);
            session.ReportingInProgress = true;
        }

        private string WriteMainReport(Assembly assembly, TestCase testCase)
        {
            string templateFullName = assembly.GetResourceFullFileName(summaryReportTemplateName, htmlFolderName, verticalTableFolderName);
            string templateContent = assembly.GetResourceContent(templateFullName);
            string existingReportData = null;
            List<string> allResourceFullFileNames = assembly.GetAllResourceFullFileNames().ToList();
            Dictionary<string, string> testCaseDataItems = GetTestCaseDataItems(testCase);
            string newReportData = BuildMainReportBody(testCaseDataItems);

            if (!firstTimeReporting)
            {
                string currentReportContent = Util.ReadFile(reportPath);
                existingReportData = GetExistingReportBody(currentReportContent);
            }

            string reportContent = templateContent.Replace("{testCases}", $"{existingReportData}{newReportData}");
            Util.WriteToFile(reportPath, true, reportContent);
            return templateFullName;
        }

        private void WriteTestCaseDetailedReport(Assembly assembly, TestCase testCase)
        {
            Dictionary<string, string> testCaseDataItems = GetTestCaseDataItems(testCase);
            string templateFullName = assembly.GetResourceFullFileName(detailedReportTemplateName, htmlFolderName, verticalTableFolderName);
            string templateContent = assembly.GetResourceContent(templateFullName);
            string testCaseDetails = BuildTestArtifactDetailedReportBody(testCaseDataItems, "table100 ver5 m-b-110", "ver5");
            string stepDetails = BuildTestSteps(testCase);

            string reportContent = templateContent.Replace("{testCaseDetails}", testCaseDetails);
            reportContent = reportContent.Replace("{testCaseName}", testCase.Method.Name);
            reportContent = reportContent.Replace("{testStepDetails}", stepDetails);
            Util.WriteToFile($"{detailedReportDirectory}{Path.DirectorySeparatorChar}{testCase.Method.Name}.html", true, reportContent);
        }

        private string BuildTestSteps(TestCase testCase)
        {
            string stepDetails = null;

            if (testCase.TestSteps.HasAny())
            {
                testCase.TestSteps.ToList().ForEach(x =>
                {
                    Dictionary<string, string> testStepDataItems = GetTestStepDataItems(x);
                    stepDetails = $"{stepDetails}{BuildTestArtifactDetailedReportBody(testStepDataItems, "table100 ver2 m-b-10", x.Result.Equals(TestResult.PASS) ? "ver1" : "ver4")}";
                });
            }

            return stepDetails;
        }

        private void CopyEmbeddedHtmlResourcesToReportFolder(Assembly assembly, string mainReportTemplateFullName)
        {
            List<string> allResourceFullFileNames = assembly.GetAllResourceFullFileNames().ToList();

            if (firstTimeReporting && allResourceFullFileNames.HasAny())
            {
                var copyingResourceFullFileNames = allResourceFullFileNames.Where(x => !x.Equals(mainReportTemplateFullName));

                if (copyingResourceFullFileNames.HasAny())
                {
                    copyingResourceFullFileNames.ToList().ForEach(x =>
                    {
                        CopyEmbeddedHtmlResourceToReportFolder(assembly, x);
                    });
                }
            }
        }

        private void CopyEmbeddedHtmlResourceToReportFolder(Assembly assembly, string resourceFullFileName)
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

        private string BuildMainReportBody(Dictionary<string, string> testCaseDataItems)
        {
            string testCaseName = testCaseDataItems["Test Case Name"];
            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(okapiSessionIdAttribute, $"{session.Id}");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "row100");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                List<string> testCaseDataItemValues = testCaseDataItems.Values.ToList();

                for (int i = 0; i < 5; i++)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, $"column100 column{i + 1}");
                    writer.AddAttribute("data-column", $"column{i + 1}");


                    if (i == 0)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.AddAttribute("href", $"{detailedReportDirectory}/{testCaseName}.html");
                        writer.RenderBeginTag(HtmlTextWriterTag.A);
                        writer.Write(testCaseDataItemValues[i]);
                        writer.RenderEndTag();
                    }
                    else if (i == 1)
                    {

                        switch (testCaseDataItemValues[i].ToLower())
                        {
                            case "pass":
                                writer.AddAttribute(HtmlTextWriterAttribute.Style, "color: green; ");
                                break;
                            case "fail":
                                writer.AddAttribute(HtmlTextWriterAttribute.Style, "color: red; ");
                                break;
                            default:
                                break;
                        }

                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    }
                    else
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.Write(testCaseDataItemValues[i]);
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();
            }

            return stringWriter.ToString();
        }

        private string BuildTestArtifactDetailedReportBody(Dictionary<string, string> testArtifactDataItems, string tableClass, string tableVersion)
        {
            StringWriter stringWriter = new StringWriter();
            List<string> keys = testArtifactDataItems.Keys.ToList();
            List<string> values = testArtifactDataItems.Values.ToList();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, tableClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.AddAttribute("data-vertable", tableVersion);
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

                for (int i = 0; i < keys.Count; i++)
                {
                    if (!string.IsNullOrWhiteSpace(values[i]))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "row100");
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "cell100 column1");
                        writer.AddAttribute("data-column", "column1");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);

                        writer.RenderBeginTag(HtmlTextWriterTag.B);
                        writer.AddAttribute("padding-left", "16em");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                        writer.Write(keys[i]);
                        writer.RenderEndTag();
                        writer.RenderEndTag();

                        writer.RenderEndTag();

                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "cell100 column2");
                        writer.AddAttribute("data-column", "column2");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.Write(values[i]);
                        writer.RenderEndTag();

                        writer.RenderEndTag();
                    }
                }

                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            return stringWriter.ToString();
        }

        private Dictionary<string, string> GetTestCaseDataItems(TestCase testCase)
        {
            return new Dictionary<string, string>
            {
                { "Test Case Name", testCase.Method.Name },
                { "Result", testCase.Result.ToString() },
                { "Duration (seconds)", testCase.DurationInSeconds == -1 ? null : testCase.DurationInSeconds.ToString() },
                { "Start At", testCase.StartDateTime.ToString()},
                { "End At", testCase.EndDateTime.ToString().Contains("1/1/0001") ? null : testCase.EndDateTime.ToString()},
                { "Test Object Info", testCase.TestObjectInfo },
                { "Additional Info", testCase.AllAdditionalData.ConvertToString()?.Replace("\"", "") },
                { "Fail Additional Info", testCase.FailAdditionalData.ConvertToString()?.Replace("\"", "") },
                { "Exception", testCase.Exception?.ToString() }
            };
        }

        private Dictionary<string, string> GetTestStepDataItems(TestStep testStep)
        {
            return new Dictionary<string, string>
            {
                { "Step Name", testStep.Method.Name },
                { "Result", testStep.Result.ToString() },
                { "Duration (seconds)", testStep.DurationInSeconds == -1 ? null : testStep.DurationInSeconds.ToString() },
                { "Start At", testStep.StartDateTime.ToString()},
                { "End At", testStep.EndDateTime.ToString().Contains("1/1/0001") ? null : testStep.EndDateTime.ToString()},
                { "Test Object Info", testStep.TestObjectInfo },
                { "Additional Info", testStep.AllAdditionalData.ConvertToString()?.Replace("\"", "") },
                { "Fail Additional Info", testStep.FailAdditionalData.ConvertToString()?.Replace("\"", "") },
                { "Exception", testStep.Exception?.ToString() },
                { "Parent Steps", testStep.ParentSteps.ConvertToString()?.Replace("\"", "") }
            };
        }

        private string GetExistingReportBody(string currentReportContent)
        {
            int dataFirstIndex = currentReportContent.IndexOf($"<tr {okapiSessionIdAttribute}=\"{session.Id}\"");
            string currentData = currentReportContent.Substring(dataFirstIndex).Trim();
            int dataLastIndex = currentData.LastIndexOf("</tr>");
            return currentData.Substring(0, dataLastIndex + 5).Trim();
        }
    }
}
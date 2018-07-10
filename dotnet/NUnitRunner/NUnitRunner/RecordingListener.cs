using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using NUnit.Engine;

namespace NUnitRunner
{
    public class RecordingListener : ITestEventListener, IDisposable
    {
        public string ReportFile { get; set; }
        public StreamWriter Writer;

        public RecordingListener(string reportFile)
        {
            ReportFile = reportFile;
            Writer = new StreamWriter(reportFile)
            {
                AutoFlush = true
            };
        }

        public void CloseFile()
        {
            Writer.Close();
        }

        public void WriteReport(ReportItem item)
        {
            var sample = new Dictionary<object, object>
            {
                { "start_time", item.StartTime },
                { "duration", item.Duration },
                { "test_case", item.TestCase },
                { "test_suite", item.TestSuite },
                { "status", item.Status },
                { "error_msg", item.ErrorMessage },
                { "error_trace", item.ErrorTrace },
                { "extras", item.Extras }
            };
            string json = JsonConvert.SerializeObject(sample);
            Writer.WriteLine(json);
        }

        public void OnTestEvent(string report)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(report);
                XmlNode node = xmlDoc.FirstChild;
                if (node.Name == "test-case")
                {
                    Console.WriteLine(report);
                    /*
                     test-case
                        id=0-1001
                        name=TestCase
                        fullname=SeleniumSuite.Test.TestCase
                        methodname=TestCase
                        classname=SeleniumSuite.Test
                        runstate=Runnable
                        seed=2077806795
                        result=Passed
                        start-time=2017-07-28 14:02:47Z
                        end-time=2017-07-28 14:02:50Z
                        duration=2.483757
                        asserts=1
                        parentId=0-1000
                    */
                    ReportItem item = new ReportItem();
                    DateTime start = DateTime.Parse(node.Attributes?["start-time"].Value);
                    item.StartTime = (start.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                    item.Duration = Double.Parse(node.Attributes?["duration"].Value,
                        NumberStyles.AllowDecimalPoint,
                        NumberFormatInfo.InvariantInfo);
                    item.TestCase = node.Attributes?["methodname"].Value;
                    item.TestSuite = node.Attributes?["classname"].Value;
                    item.ErrorMessage = "";
                    item.ErrorTrace = "";
                    if (node.Attributes?["result"].Value == "Passed")
                    {
                        item.Status = "PASSED";
                    }
                    else if (node.Attributes?["result"].Value == "Failed")
                    {
                        item.Status = "FAILED";
                        XmlNode failureNode = node.SelectSingleNode("failure");
                        if (failureNode != null)
                        {
                            string message = failureNode.SelectSingleNode("message")?.InnerText;
                            string trace = failureNode.SelectSingleNode("stack-trace")?.InnerText;
                            item.ErrorMessage = message.Trim();
                            item.ErrorTrace = trace.Trim();
                        }
                    }
                    else if (node.Attributes?["result"].Value == "Skipped")
                    {
                        item.Status = "SKIPPED";
                        XmlNode reasonNode = node.SelectSingleNode("reason");
                        if (reasonNode != null)
                            item.ErrorMessage = reasonNode.SelectSingleNode("message")?.InnerText.Trim();
                    }
                    else
                        Console.WriteLine(report);

                    WriteReport(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"EXCEPTION: {e}");
            }
        }

        public void Dispose()
        {
            CloseFile();
        }
    }
}
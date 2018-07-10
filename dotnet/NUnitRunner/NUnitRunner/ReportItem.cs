using System.Collections.Generic;

namespace NUnitRunner
{
    public class ReportItem
    {
        public long StartTime;
        public double Duration;
        public string TestCase;
        public string TestSuite;
        public string Status;
        public string ErrorMessage;
        public string ErrorTrace;
        public Dictionary<object, object> Extras;
    }
}
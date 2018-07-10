using System;
using System.Reflection;
using Mono.Options;
using NUnit.Engine;


namespace NUnitRunner
{
    public class NUnitRunner
    {     
        public static void ShowHelp()
        {
            Console.WriteLine("NUnit runner for Taurus");
			Console.WriteLine("Usage:");
            Console.WriteLine("\t <executable> ARGS");
            Console.WriteLine("\t --iterations N - number of iterations over test suite to make");
			Console.WriteLine("\t --duration T - duration limit of test suite execution");
            Console.WriteLine("\t --report-file REPORT_FILE - filename of report file");
            Console.WriteLine("\t --target TARGET_ASSEMBLY - assembly which will be used to load tests from");
            Console.WriteLine("\t --help - show this message and exit");
            Environment.Exit(0);
		}

        public static RunnerOptions ParseOptions(string[] args)
        {
            var opts = new RunnerOptions();
            
			var optionSet = new OptionSet {
				{ "i|iterations=", "number of iterations over test suite to make.", (int n) => opts.iterations = n },
                { "d|duration=", "duration of test suite execution.", (int d) => opts.durationLimit = d },
				{ "r|report-file=", "Name of report file", r => opts.reportFile = r },
                { "t|target=", "Test suite", t => opts.targetAssembly = t },
			    { "f|filter=", "Test Case Filter", t => opts.testCaseFilter = string.IsNullOrEmpty(t) ? TestFilter.Empty : new TestFilter(t) },
                { "h|help", "show this message and exit", h => opts.shouldShowHelp = h != null },
			};

            optionSet.Parse(args);

			if (opts.shouldShowHelp)
				ShowHelp();

            if (opts.targetAssembly == null)
            {
                throw new Exception("Target test suite wasn't provided. Is your file actually NUnit test DLL?");
            }

            var path = AppDomain.CurrentDomain.BaseDirectory + opts.targetAssembly.Replace("/", "\\");
            try
            {      
                opts.targetAssembly = Assembly.LoadFile(path).Location;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to load Assembly:{path}", e);

            }
     
            

            if (opts.iterations == 0) opts.iterations = opts.durationLimit > 0 ? int.MaxValue : 1;

            Console.WriteLine($"Iterations: {opts.iterations}");
            Console.WriteLine($"Hold for: {opts.durationLimit}");
            Console.WriteLine($"Report file: {opts.reportFile}");
            Console.WriteLine($"Target: {opts.targetAssembly}");
            if (!string.IsNullOrEmpty(opts.testCaseFilter?.Text))
            {
                Console.WriteLine($"Test Case Filter: {opts.testCaseFilter?.Text}");
            }

            return opts;
		}

		public static void Main(string[] args)
        {

            RunnerOptions opts = null;
			try
			{
                opts = ParseOptions(args);
			}
			catch (OptionException e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Try running with '--help' for more information.");
                Environment.Exit(1);
			}

            var listener = new RecordingListener(opts.reportFile);

			var engine = TestEngineActivator.CreateInstance();
            var package = new TestPackage(opts.targetAssembly);

            var runner = engine.GetRunner(package);
            
            var testCount = runner.CountTestCases(opts.testCaseFilter);
            if (testCount < 1)
                throw new ArgumentException("Nothing to run, no tests were loaded");

            try
            {
                var startTime = DateTime.Now;
                for (var i = 0; i < opts.iterations; i++)
                {
                    runner.Run(listener, opts.testCaseFilter);
                    var offset = DateTime.Now - startTime;
                    if (opts.durationLimit > 0 && offset.TotalSeconds > opts.durationLimit)
                        break;
                }
            }
            finally
            {
                listener.CloseFile();
            }
            Environment.Exit(0);
		}
    }
}

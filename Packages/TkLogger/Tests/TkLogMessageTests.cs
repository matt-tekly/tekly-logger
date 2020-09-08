using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace Tekly.Logging.Tests
{
    public class TkLogMessageTests
    {
        [Test]
        public void TestPrint()
        {
            var logParams = TkLogParams.Create("a", 1, "b", 2, "c", "3");
            var logMessage = new TkLogMessage(TkLogLevel.Info, "Test", "Test", "Test {a} {b} {c}", StackTraceUtility.ExtractStackTrace(), logParams.Params);
            
            var sb = new StringBuilder();
            logMessage.Print(sb);
            Assert.That(sb.ToString(), Is.EqualTo("Test 1 2 3"));
        }
    }
}


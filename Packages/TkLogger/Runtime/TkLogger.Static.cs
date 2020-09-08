using System;
using System.Collections.Generic;
using Tekly.Utility.LifeCycles;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace Tekly.Logging
{
    public partial class TkLogger
    {
        public static bool EnableStackTrace = true;
        public static TkLogLevel GlobalMinLogLevel = TkLogLevel.Info;

        public static bool EnableUnityLogger = TkLoggerConstants.UNITY_LOG_ENABLED_DEFAULT;

        public static readonly List<ITkLogDestination> Destinations = new List<ITkLogDestination>();

        public static readonly Dictionary<string, string> CommonFields = new Dictionary<string, string>();

        private static UnityLogDestination s_unityLogDestination;

        private static readonly Dictionary<Type, TkLogger> s_loggers = new Dictionary<Type, TkLogger>();
        private static readonly object s_lock = new object();
        
        public static TkLogger Get<T>(TkLogLevel logLevel = TkLogLevel.Info)
        {
            return Get(typeof(T), logLevel);
        }

        public static TkLogger Get(Type type, TkLogLevel logLevel = TkLogLevel.Info)
        {
            lock (s_lock) {
                if (!s_loggers.TryGetValue(type, out var logger)) {
                    logger = new TkLogger(type, logLevel);
                    s_loggers[type] = logger;
                }

                return logger;
            }
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            s_unityLogDestination = new UnityLogDestination();
            Application.logMessageReceivedThreaded += HandleLog;
            LifeCycle.Instance.Update += Update;
        }
        
        
        private static void Update()
        {
            if (EnableUnityLogger) {
                s_unityLogDestination.Update();
            }

            foreach (var destination in Destinations) {
                destination.Update();
            }
        }

        private static void HandleLog(string message, string stacktrace, LogType type)
        {
            if (!EnableUnityLogger) {
                return;
            }

            if (message[message.Length - 1] == '\u2004') {
                return;
            }

            var level = UnityLogDestination.TypeToLevel(type);
            var loggerName = TkLoggerConstants.UNITY_LOG_NAME;
            stacktrace = stacktrace.Replace("\\", "/");
            LogToDestinations(new TkLogMessage(level, loggerName, loggerName, message, stacktrace), false);
        }

        public static void SetValue(string id, object value)
        {
            CommonFields[id] = value.ToString();
        }

        public static void ClearValue(string id)
        {
            CommonFields.Remove(id);
        }

        private static string GetStackTrace(TkLogLevel logLevel)
        {
            if (!EnableStackTrace) {
                return null;
            }

            var stackTrace = StackTraceUtility.ExtractStackTrace();

            // Find the index where the stack trace frames related logging end.
            int count = 0;
            int index;

            for (index = 0; index < stackTrace.Length; index++) {
                if (stackTrace[index] == '\n') {
                    count++;
                    if (count == 3) {
                        break;
                    }
                }
            }

            // Now trim out the frames related to logging so we get a clean stack trace
            return stackTrace.Substring(index + 1).Replace("\\", "/");
        }

        private static void LogToDestinations(TkLogMessage message, bool logToUnity = true)
        {
            if (EnableUnityLogger && logToUnity) {
                s_unityLogDestination.LogMessage(message);
            }

            foreach (var destination in Destinations) {
                destination.LogMessage(message);
            }
        }

        private static void LogToDestinations(TkLogMessage message, Object context, bool logToUnity = true)
        {
            if (EnableUnityLogger && logToUnity) {
                s_unityLogDestination.LogMessage(message, context);
            }

            foreach (var destination in Destinations) {
                destination.LogMessage(message, context);
            }
        }
    }
}
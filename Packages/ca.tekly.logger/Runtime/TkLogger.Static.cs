using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tekly.LifeCycles;
using UnityEngine;
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
        private static readonly ThreadLocal<StringBuilder> s_stringBuilders = new ThreadLocal<StringBuilder>(() => new StringBuilder(512));
        
        public static TkLogger Get<T>()
        {
            return Get(typeof(T));
        }

        public static TkLogger Get(Type type)
        {
            lock (s_lock) {
                if (!s_loggers.TryGetValue(type, out var logger)) {
                    logger = new TkLogger(type, GlobalMinLogLevel);
                    s_loggers[type] = logger;
                }

                return logger;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            s_unityLogDestination = new UnityLogDestination();
            Application.logMessageReceivedThreaded += HandleUnityLog;
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

        private static void HandleUnityLog(string message, string stacktrace, LogType type)
        {
            if (!EnableUnityLogger) {
                return;
            }

            if (message[message.Length - 1] == TkLoggerConstants.UNITY_LOG_MARKER) {
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

            var sb = s_stringBuilders.Value;
            sb.Clear();
            
            StackTraceUtilities.ExtractStackTrace(sb, 4);
            
            sb.Replace("\\", "/");
            
            return sb.ToString();
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
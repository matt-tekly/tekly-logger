using System;
using Object = UnityEngine.Object;

namespace Tekly.Logging
{
    public partial class TkLogger
    {
        public readonly Type Type;
        public readonly string Name;
        public readonly string FullName;
        public readonly TkLogLevel MinLogLevel;

        public TkLogger(Type type, TkLogLevel minLogLevel)
        {
            Type = type;
            Name = Type.Name;
            FullName = Type.FullName;
            MinLogLevel = minLogLevel;
        }

#if TKLOG_DISABLE_TRACE
        [Conditional("TK_UNDEFINED")]
#endif
        public void Trace(string message)
        {
            LogMessage(TkLogLevel.Trace, message);
        }

#if TKLOG_DISABLE_TRACE
        [Conditional("TK_UNDEFINED")]
#endif
        public void Trace(string message, params object[] logParams)
        {
            LogMessage(TkLogLevel.Trace, message, logParams);
        }

#if TKLOG_DISABLE_TRACE
        [Conditional("TK_UNDEFINED")]
#endif
        public void TraceContext(string message, Object context)
        {
            LogMessage(TkLogLevel.Trace, message, context);
        }

#if TKLOG_DISABLE_TRACE
        [Conditional("TK_UNDEFINED")]
#endif
        public void TraceContext(string message, Object context, params object[] logParams)
        {
            LogMessage(TkLogLevel.Trace, message, context, logParams);
        }

#if TKLOG_DISABLE_INFO
        [Conditional("TK_UNDEFINED")]
#endif
        public void Info(string message)
        {
            LogMessage(TkLogLevel.Info, message);
        }

#if TKLOG_DISABLE_INFO
        [Conditional("TK_UNDEFINED")]
#endif
        public void Info(string message, params object[] logParams)
        {
            LogMessage(TkLogLevel.Info, message, logParams);
        }

#if TKLOG_DISABLE_INFO
        [Conditional("TK_UNDEFINED")]
#endif
        public void InfoContext(string message, Object context)
        {
            LogMessage(TkLogLevel.Info, message, context);
        }

#if TKLOG_DISABLE_INFO
        [Conditional("TK_UNDEFINED")]
#endif
        public void InfoContext(string message, Object context, params object[] logParams)
        {
            LogMessage(TkLogLevel.Info, message, context, logParams);
        }

#if TKLOG_DISABLE_WARNING
        [Conditional("TK_UNDEFINED")]
#endif
        public void Warning(string message)
        {
            LogMessage(TkLogLevel.Warning, message);
        }

#if TKLOG_DISABLE_WARNING
        [Conditional("TK_UNDEFINED")]
#endif
        public void Warning(string message, params object[] logParams)
        {
            LogMessage(TkLogLevel.Warning, message, logParams);
        }

#if TKLOG_DISABLE_WARNING
        [Conditional("TK_UNDEFINED")]
#endif
        public void WarningContext(string message, Object context)
        {
            LogMessage(TkLogLevel.Warning, message, context);
        }

#if TKLOG_DISABLE_WARNING
        [Conditional("TK_UNDEFINED")]
#endif
        public void WarningContext(string message, Object context, params object[] logParams)
        {
            LogMessage(TkLogLevel.Warning, message, context, logParams);
        }

#if TKLOG_DISABLE_ERROR
        [Conditional("TK_UNDEFINED")]
#endif
        public void Error(string message)
        {
            LogMessage(TkLogLevel.Error, message);
        }

#if TKLOG_DISABLE_ERROR
        [Conditional("TK_UNDEFINED")]
#endif
        public void Error(string message, params object[] logParams)
        {
            LogMessage(TkLogLevel.Error, message, logParams);
        }

#if TKLOG_DISABLE_ERROR
        [Conditional("TK_UNDEFINED")]
#endif
        public void ErrorContext(string message, Object context)
        {
            LogMessage(TkLogLevel.Error, message, context);
        }

#if TKLOG_DISABLE_ERROR
        [Conditional("TK_UNDEFINED")]st
#endif
        public void ErrorContext(string message, Object context, params object[] logParams)
        {
            LogMessage(TkLogLevel.Error, message, context, logParams);
        }

        public void LogMessage(TkLogLevel level, string message)
        {
            if (level < GlobalMinLogLevel || level < MinLogLevel) {
                return;
            }

            LogToDestinations(new TkLogMessage(level, Name, FullName, message, GetStackTrace(level)));
        }

        public void LogMessage(TkLogLevel level, string message, params object[] logParams)
        {
            if (level < GlobalMinLogLevel || level < MinLogLevel) {
                return;
            }

            LogToDestinations(new TkLogMessage(level, Name, FullName, message, GetStackTrace(level), logParams));
        }

        public void LogMessage(TkLogLevel level, string message, Object context)
        {
            if (level < GlobalMinLogLevel || level < MinLogLevel) {
                return;
            }

            LogToDestinations(new TkLogMessage(level, Name, FullName, message, GetStackTrace(level)), context);
        }

        public void LogMessage(TkLogLevel level, string message, Object context, params object[] logParams)
        {
            if (level < GlobalMinLogLevel || level < MinLogLevel) {
                return;
            }

            LogToDestinations(new TkLogMessage(level, Name, FullName, message, GetStackTrace(level), logParams), context);
        }
    }
}
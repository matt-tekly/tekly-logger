using System;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Tekly.Logging
{
    public class UnityLogDestination : ITkLogDestination
    {
        private int m_currentFrame;
        
        private readonly ThreadLocal<StringBuilder> m_stringBuilders = new ThreadLocal<StringBuilder>(() => new StringBuilder(512));
        private readonly ThreadLocal<object[]> m_objects = new ThreadLocal<object[]>(() => new object[4]);
        private readonly MethodInfo m_logMethod;
        
        public UnityLogDestination()
        {
            var debugLogHandler = typeof(Texture2D).Assembly.GetType("UnityEngine.DebugLogHandler");
            m_logMethod = debugLogHandler.GetMethod("Internal_Log", BindingFlags.Static | BindingFlags.NonPublic);
            
            Assert.IsNotNull(m_logMethod, "UnityLogDestination failed to find method UnityEngine.DebugLogHandler.Internal_Log");
        }
        
        public void LogMessage(TkLogMessage message)
        {
            LogMessage(message, null);
        }

        public void LogMessage(TkLogMessage message, Object context)
        {
            var sb = m_stringBuilders.Value;
            sb.Clear();
            
            sb.AppendFormat("[{0}] [{1}] ", m_currentFrame, message.LoggerName);
            message.Print(sb);
            sb.AppendFormat("\n\n{0}\u2004", message.StackTrace);
            
            var logType = LevelToType(message.Level);

            var objectArray = m_objects.Value;
            objectArray[0] = logType;
            objectArray[1] = LogOption.NoStacktrace;
            objectArray[2] = sb.ToString();
            objectArray[3] = context;
            
            m_logMethod.Invoke(null, objectArray);
        }
        
        public void Update()
        {
            m_currentFrame = Time.frameCount;
        }
        
        public static LogType LevelToType(TkLogLevel level)
        {
            switch (level) {
                case TkLogLevel.Trace:
                    return LogType.Log;
                case TkLogLevel.Info:
                    return LogType.Log;
                case TkLogLevel.Warning:
                    return LogType.Warning;
                case TkLogLevel.Error:
                    return LogType.Error;
                case TkLogLevel.Exception:
                    return LogType.Exception;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static TkLogLevel TypeToLevel(LogType logType)
        {
            switch (logType) {
                case LogType.Error:
                    return TkLogLevel.Error;
                case LogType.Assert:
                    return TkLogLevel.Error;
                case LogType.Warning:
                    return TkLogLevel.Warning;
                case LogType.Log:
                    return TkLogLevel.Info;
                case LogType.Exception:
                    return TkLogLevel.Exception;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }
    }
}
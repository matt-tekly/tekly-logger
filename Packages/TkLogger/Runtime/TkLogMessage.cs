using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Tekly.Logging
{
    [Serializable]
    public struct TkLogMessage
    {
        public TkLogLevel Level;
        public string LoggerName;
        public string LoggerFullName;
        public string Message;
        public List<TkLogParam> Params;
        public string Timestamp;
        public string StackTrace;

        public TkLogMessage(TkLogLevel level, string loggerName, string loggerFullName, string message, string stackTrace)
        {
            Level = level;
            LoggerName = loggerName;
            LoggerFullName = loggerFullName;
            Message = message;
            Params = null;
            Timestamp = DateTime.UtcNow.ToString(TkLoggerConstants.TIME_FORMAT);
            StackTrace = stackTrace;
            
            if (TkLogger.CommonFields.Count > 0) {
                Params = new List<TkLogParam>();
                CopyCommonFields();
            }
        }
        
        public TkLogMessage(TkLogLevel level, string loggerName, string loggerFullName, string message, string stackTrace, List<TkLogParam> logParams)
        {
            Level = level;
            LoggerName = loggerName;
            LoggerFullName = loggerFullName;
            Message = message;
            Params = logParams;
            Timestamp = DateTime.UtcNow.ToString(TkLoggerConstants.TIME_FORMAT);
            StackTrace = stackTrace;
            
            CopyCommonFields();
        }
        
        public TkLogMessage(TkLogLevel level, string loggerName, string loggerFullName, string message, string stackTrace, params object[] logParams)
        {
            Level = level;
            LoggerName = loggerName;
            LoggerFullName = loggerFullName;
            Message = message;
            Params = TkLogParams.Create(logParams).Params;
            Timestamp = DateTime.UtcNow.ToString(TkLoggerConstants.TIME_FORMAT);
            StackTrace = stackTrace;
            
            CopyCommonFields();
        }

        public void Print(StringBuilder sb)
        {
            sb.Append(Message);
            
            if (Params == null || !Message.Contains("{")) {
                return;
            }
            
            foreach (var param in Params) {
                sb.Replace($"{{{param.Id}}}", param.Value);
            }
        }

        /// <summary>
        /// Copying the common fields when the message is created so that we get the fields as they were when the
        /// log message was created.
        /// </summary>
        private void CopyCommonFields()
        {
            foreach (var commonField in TkLogger.CommonFields) {
                Params.Add(new TkLogParam(commonField.Key, commonField.Value));
            }
        }

        public void ToJson(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append($"\"Level\":\"{Level}\"");
            sb.Append($",\"Logger\":\"{LoggerFullName}\"");
            sb.Append($",\"Message\":\"{Message}\"");
            sb.Append($",\"Timestamp\":\"{Timestamp}\"");
            
            if (!string.IsNullOrEmpty(StackTrace)) {
                sb.Append($",\"StackTrace\":\"{StackTrace}\"");    
            }
            
            if (Params != null) {
                foreach (var tkLogParam in Params) {
                    sb.Append($",\"{tkLogParam.Id}\":\"{tkLogParam.Value}\"");
                }
            }
            
            sb.Append("}");
        }
    }

    public struct TkLogParams
    {
        public List<TkLogParam> Params;

        public TkLogParams Add(string id, object value)
        {
            Params.Add(new TkLogParam(id, value.ToString()));
            return this;
        }

        public static TkLogParams Create()
        {
            var tkLogParams = new TkLogParams {Params = new List<TkLogParam>()};
            return tkLogParams;
        }

        public static TkLogParams Create(params object[] logParams)
        {
            var tkLogParams = new TkLogParams {Params = new List<TkLogParam>()};
            for (var index = 0; index < logParams.Length; index += 2) {
                tkLogParams.Add(logParams[index].ToString(), logParams[index + 1]);
            }

            return tkLogParams;
        }
    }

    [Serializable]
    public struct TkLogParam
    {
        public string Id;
        public string Value;

        public TkLogParam(string id, string value)
        {
            Id = id;
            Value = value;
        }
    }
}
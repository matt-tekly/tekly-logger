﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tekly.Logging
{
    public static class StackTraceUtilities
    {
        private static string s_projectFolder;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            var field = typeof(StackTraceUtility).GetField("projectFolder", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "field != null");

            s_projectFolder = field.GetValue(null) as string;
            Assert.IsNotNull(s_projectFolder, "projectFolder != null");
        }

        public static void ExtractStackTrace(StringBuilder sb, int skipFrames = 1)
        {
            ExtractFormattedStackTrace(new StackTrace(skipFrames, true), sb);
        }

        public static void ExtractStringFromExceptionInternal(Exception exception, out string message, out string stackTrace)
        {
            if (exception == null) {
                throw new ArgumentException("ExtractStringFromExceptionInternal called with null exception");
            }

            var exceptionStackTrace = exception.StackTrace;

            // StackTrace might not be available
            StringBuilder sb = new StringBuilder(exceptionStackTrace?.Length * 2 ?? 512);
            message = "";
            string traceString = "";
            
            while (exception != null) {
                if (traceString.Length == 0) {
                    traceString = exception.StackTrace;
                } else {
                    traceString = exception.StackTrace + "\n" + traceString;
                }

                string thisMessage = exception.GetType().Name;
                string exceptionMessage = "";
                
                if (exception.Message != null) {
                    exceptionMessage = exception.Message;
                }
                
                if (!string.IsNullOrWhiteSpace(exceptionMessage)) {
                    thisMessage += ": ";
                    thisMessage += exceptionMessage;
                }

                message = thisMessage;
                if (exception.InnerException != null) {
                    traceString = "Rethrow as " + thisMessage + "\n" + traceString;
                }

                exception = exception.InnerException;
            }

            sb.Append(traceString + "\n");
            sb.Replace("\\", "/");
            sb.Replace(s_projectFolder, "");

            StackTrace trace = new StackTrace(2, true);
            ExtractFormattedStackTrace(trace, sb);

            sb.Replace("\\", "/");
            stackTrace = sb.ToString();
        }

        public static void ExtractFormattedStackTrace(StackTrace stackTrace, StringBuilder sb)
        {
            sb = sb ?? new StringBuilder(255);
            int iIndex;

            // need to skip over "n" frames which represent the
            // System.Diagnostics package frames
            for (iIndex = 0; iIndex < stackTrace.FrameCount; iIndex++) {
                StackFrame frame = stackTrace.GetFrame(iIndex);

                MethodBase mb = frame.GetMethod();
                if (mb == null)
                    continue;

                Type classType = mb.DeclaringType;
                if (classType == null)
                    continue;

                var classTypeName = classType.Name;
                var classTypeNamespace = classType.Namespace;

                // Add namespace.classname:MethodName
                if (!string.IsNullOrEmpty(classTypeNamespace)) {
                    sb.Append(classTypeNamespace);
                    sb.Append(".");
                }

                sb.Append(classTypeName);
                sb.Append(":");
                sb.Append(mb.Name);
                sb.Append("(");

                // Add parameters
                int j = 0;
                ParameterInfo[] pi = mb.GetParameters();
                bool fFirstParam = true;
                while (j < pi.Length) {
                    if (fFirstParam == false)
                        sb.Append(", ");
                    else
                        fFirstParam = false;

                    sb.Append(pi[j].ParameterType.Name);
                    j++;
                }

                sb.Append(")");

                // Add path name and line number - unless it is a Debug.Log call, then we are only interested
                // in the calling frame.
                string path = frame.GetFileName();
                if (path != null) {
                    bool shouldStripLineNumbers =
                            (classTypeName == "Debug" && classTypeNamespace == "UnityEngine") ||
                            (classTypeName == "Logger" && classTypeNamespace == "UnityEngine") ||
                            (classTypeName == "DebugLogHandler" && classTypeNamespace == "UnityEngine") ||
                            (classTypeName == "Assert" && classTypeNamespace == "UnityEngine.Assertions") ||
                            (mb.Name == "print" && classTypeName == "MonoBehaviour" && classTypeNamespace == "UnityEngine")
                        ;

                    if (!shouldStripLineNumbers) {
                        sb.Append(" (at ");

                        if (!string.IsNullOrEmpty(s_projectFolder)) {
                            if (StartsWithPath(path, s_projectFolder)) {
                                path = path.Substring(s_projectFolder.Length, path.Length - s_projectFolder.Length);
                            }
                        }

                        sb.Append(path);
                        sb.Append(":");
                        sb.Append(frame.GetFileLineNumber().ToString());
                        sb.Append(")");
                    }
                }

                sb.Append("\n");
            }

            sb.Replace("\\", "/");
        }

        /// <summary>
        /// Determines if strA starts with strB but ignores differences in slashes
        /// </summary>
        private static unsafe bool StartsWithPath(string strA, string strB)
        {
            if (strA == null || strB == null) {
                return false;
            }

            var length = strB.Length;

            if (strA.Length < length) {
                return false;
            }

            fixed (char* startA = strA)
            fixed (char* startB = strB) {
                char* charA = startA;
                char* charB = startB;

                for (int i = 0; i < length; i++) {
                    if (*charA != *charB) {
                        if ((*charA == '\\' && *charB == '/') || (*charA == '/' && *charB == '\\')) {
                            continue;
                        }

                        return false;
                    }

                    ++charA;
                    ++charB;
                }
            }

            return true;
        }
    }
}
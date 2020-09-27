﻿using System;
using Tekly.Logging;
using UnityEngine;

public class TestLogs : MonoBehaviour
{
    private TkLogger m_logger = TkLogger.Get<TestLogs>();
    
    public void OnEnable()
    {
        TkLogger.SetValue("g_user", "Joe");

        m_logger.Info("Start"); // [TestLogs] Start
        m_logger.Info("Start {g_user}"); // [TestLogs] Start Joe
        m_logger.Info("Start {g_user} {id}", ("id", 5)); // [TestLogs] Start 5
        m_logger.Info("Start {date}", ("date", DateTime.Now)); // [TestLogs] Start 9/7/2020 10:15:22 AM

        TestException();
    }

    private void TestException()
    {
        try {
            throw new Exception("[Exception Message]");
        } catch (Exception ex) {
            m_logger.Exception(ex, "[Log Message]");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{

    public Dictionary<string, string> logColumns;
    public Dictionary<string, string> resetColumns;
    public bool logging;

    private StreamWriter writer = null;

    private float logStartTime, logUnpauseTime;

    void OnEnable()
    {
        logColumns = new Dictionary<string, string>();
        resetColumns = new Dictionary<string, string>();
        logColumns.Add("time", Time.time.ToString());
        logColumns.Add("timeNoPauses", Time.time.ToString());

        //Application.logMessageReceivedThreaded += HandleLog;
    }


    public void AddEntry(string k, string v = "", bool resetEachFrame = false)
    {

        logColumns.Add(k, v + "\t");
        //print(k + " added!");
        if (resetEachFrame)
            resetColumns.Add(k, v);

    }

    public void UpdateEntry(string k, string v = "")
    {
        if (logColumns.ContainsKey(k))
            logColumns[k] = v + "\t";
        else
            Debug.LogWarning("Attempted update of missing log-key: " + k);

    }


    public void StartLogging(string id)
    {

        print("started logging");
        if (logging)
        {
            Debug.LogWarning("Logging was on when StartLogging was called. No new log was started.");
            return;
        }
        
        logging = true;
        

        //string logPath = Application.dataPath + "/Logs/";
        //Directory.CreateDirectory(logPath);

        DateTime now = DateTime.Now;
        string fileName = string.Format("{0}-{1}-{2:00}-{3:00}-{4:00}-{5:00}", id, now.Year, now.Month, now.Day, now.Hour, now.Minute);

        //string path = logPath + fileName + ".tsv";
        string path = Path.Combine(Application.persistentDataPath, fileName + ".tsv");
        writer = new StreamWriter(path);

        Debug.Log("Log file started at: " + path);
        LogLabels();

        logStartTime = Time.time;
        logUnpauseTime = Time.time;

        

    }

    public void PauseLogging()
    {
        if (!logging)
        {
            Debug.LogWarning("Logging was already paused when PauseLogging was called.");
            return;
        }

        logging = false;

    }

    public void ResumeLogging()
    {
        if (logging)
        {
            Debug.LogWarning("Logging was already running when ResumeLogging was called.");
            return;
        }

        logUnpauseTime = Time.time;
        logging = true;

    }

    private void LogLabels()
    {
        if (writer == null)
            return;

        string line = "";
        foreach (string k in logColumns.Keys)
        {
            line += k + "\t";
        }
        writer.WriteLine(line);

    }

    public void Log()
    {

        if (!logging || writer == null)
            return;

        string line = "";
        foreach (string v in logColumns.Values)
        {
            line += v;
        }
        writer.WriteLine(line);
        //writer.Flush();

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void FixedUpdate()
    {

        if (logging){
            UpdateEntry("time", (Time.time).ToString("F4"));
            UpdateEntry("timeNoPauses", (Time.time - logUnpauseTime).ToString("F4"));

            Log();
            foreach (var item in resetColumns)
            {
                UpdateEntry(item.Key, item.Value);
            }

        }

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            writer.WriteLine(logString);
            writer.WriteLine(stackTrace);
            writer.Flush();

        }
    }

    void OnApplicationQuit()
    {
        if (writer != null && writer.BaseStream != null)
        {
            writer.Flush();
            writer.Close();
        }
    }

    void OnDestroy()
    {
        if (writer != null && writer.BaseStream != null)
        {
            writer.Flush();
            writer.Close();
        }
    }

    void OnApplicationPause()
    {
        if (writer != null)
        {
            writer.Flush();
        }
    }
}

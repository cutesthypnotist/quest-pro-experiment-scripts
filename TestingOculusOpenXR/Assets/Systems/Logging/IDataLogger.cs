using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.IO;

/// <summary>
/// Allows for the basic logging of some data at a consistent fixedupdate rate.
/// </summary>
public abstract class DataLogger : MonoBehaviour
{
    [Header("File Info - logPath/activity/user-time-dataType.csv")]
    public string logPath = "logs";
    public string activity;
    public string username;
    public string dataType;

    [Header("Write Info")]
    [SerializeField]
    protected float writeInterval;
    private float writeTimer = 0;

    [SerializeField]
    [Tooltip("If true, calls LogLine every FixedUpdate. If false, call it yourself.")]
    protected bool writeConstant = true;

    [SerializeField]
    protected bool isRecording;
    protected float logStartTime;
    protected List<string> lines;
    protected string filePath;

    public float delayStart = 0f;
    private bool delayed = false;

    // The data to be logged.
    protected Dictionary<string, string> Data;
    // Start is called before the first frame update
    protected void Start()
    {
        if (delayStart > 0f) delayed = true;
        lines = new List<string>();

        // Initialize the data dictionary.
        Data = new Dictionary<string, string> {
            {"subject_id", "NA"},
            {"sync_id", "NA"},
            {"timestamp", "NA"},
            {"sys_ticks", "NA"},  
            {"unity_log_time", "NA"}
        };
    }

    // Update is called once per frame
    protected void Update()
    {
        if (isRecording)
        {
            writeTimer += Time.deltaTime;
            if (writeTimer > writeInterval)
            {
                WriteLines();
                writeTimer = 0;
            }
        }
    }

    // Log data every .02 seconds;
    private void FixedUpdate()
    {
        // TODO: Make a logging rate
        if (writeConstant && isRecording && !delayed)
        {
            LogLine();
        }
    }

    /// <summary>
    /// Start logging with the activity name as already set
    /// </summary>
    public void StartLogging()
    {
        isRecording = true;
        logStartTime = Time.time;
        // Make sure the directory exists
        Directory.CreateDirectory(logPath + @"\" + activity + @"\");

        // Write and clear the file
        string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() +
            " " + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString();
        filePath = logPath + @"\" + activity + @"\" + username + " " + time + " " +
            dataType + ".csv";

        if (delayed)
        {
            StartCoroutine(delayWriteHeader());
        }
        else
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, false))
            {
                WriteHeader(file);
            }
        }

    }


    private IEnumerator delayWriteHeader()
    {
        yield return new WaitForSeconds(delayStart);
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, false))
        {
            WriteHeader(file);
        }
        delayed = false;
    }

    public void StopLogging()
    {
        isRecording = false;
        WriteLines();
        writeTimer = 0;
    }

    public void SetActivity(string a)
    {
        activity = a;
    }

    public bool GetIsRecording()
    {
        return isRecording;
    }

    // ----- Data Functions -----

    /// <summary>
    /// Add a column. If the column already exists, the value is updated.
    /// </summary>
    public bool SetOrAddData(string key, string value)
    {
        if (Data.ContainsKey(key))
        {
            Data[key] = value;
            return true;
        }
        else
        {
            Data.Add(key, value);
            return false;
        }
    }

    /// <summary>
    /// Sets common data for all logs.
    /// </summary>
    protected void SetCommonData()
    {
        Data["subject_id"] = SubjectDataManager.Instance.subject_id;
        Data["sync_id"] = SubjectDataManager.Instance.sync_id.ToString();
        Data["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Data["sys_ticks"] = DateTime.Now.Ticks.ToString();
        Data["unity_log_time"] = (Time.time - logStartTime).ToString();
    }

    // ----- Writing Functions -----
    protected virtual void WriteLines()
    {
        // Make sure there are lines in the first place
        if (lines.Count == 0) return;

        // Write all lines to file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
        {
            foreach (string line in lines)
                file.WriteLine(line);
            lines.Clear();
            file.Close();
        }
    }

    /// <summary>
    /// Add one line of data. Override this method and call base.LogLine() in the override for added data. 
    /// </summary>
    protected virtual void LogLine()
    {
        SetCommonData();
        string line = "";
        for (int i = 0; i < Data.Count; i++)
        {
            line += Data.ElementAt(i).Value;
            if (i < Data.Count - 1)
                line += ",";
        }
        lines.Add(line);
    }

    /// <summary>
    /// Adds the header. Overridden for gaze data.
    /// </summary>
    protected virtual void WriteHeader(System.IO.StreamWriter file)
    {
        for (int i = 0; i < Data.Count; i++)
        {
            file.Write(Data.ElementAt(i).Key);
            if (i < Data.Count - 1)
                file.Write(",");
        }
        file.Write(System.Environment.NewLine);
    }
}

using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public static class GazeLog {

    public static List<string> lines = new List<string>();
        
    public static Dictionary<string, string> GazeData = new Dictionary<string, string>
    {
        {"subject_id", ""},
        {"sync_id", ""},
        {"timestamp", ""},
        {"dt_ticks", ""},
        {"no_user", ""},
        {"gaze_position", ""},
        {"gaze_direction", ""}
    };


    public static void  AddGazeLine() {

        GazeData["subject_id"] = SubjectDataManager.Instance.subject_id;
        GazeData["sync_id"] = SubjectDataManager.Instance.sync_id.ToString();
        GazeData["dt_ticks"] = DateTime.Now.Ticks.ToString();
        GazeData["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        GazeData["gaze_position"] = DataUtilities.EncodeVector3CSV(OculusGazeDataHub.Instance.GazeData.GazeOrigin);
        GazeData["gaze_direction"] = DataUtilities.EncodeVector3CSV(OculusGazeDataHub.Instance.GazeData.GazeDirection);
        var line = "";
        for (int i = 0; i < GazeData.Count; i++)
        {
            line += GazeData.ElementAt(i).Value;
            if (i < GazeData.Count - 1)
                line += ",";
        }
        lines.Add(line);
        line = "";
    }
}


public class GazeLogger : DataLogger {

    private static GazeLogger _instance;
    public static GazeLogger Instance { get { return _instance; } }

    public static List<Dictionary<string,string>> GazeDataLines = new List<Dictionary<string,string>>();
    public static Dictionary<string, string> GazeData = new Dictionary<string, string>
        {
            {"subject_id", ""},
            {"sync_id", ""},
            {"timestamp", ""},
            {"dt_ticks", ""},
            {"no_user", ""},
            {"gaze_position", ""},
            {"gaze_direction", ""}
        };
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }
    protected override void WriteLines()
    {
        // Make sure there are lines in the first place
        if (GazeLog.lines.Count == 0) return;

        // Write all lines to file
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
        {
            for (int i = 0; i < GazeLog.lines.Count; i++)
                file.WriteLine(GazeLog.lines[i]);
            GazeLog.lines.Clear();
            file.Close();
        }
    }

    protected override void WriteHeader(System.IO.StreamWriter file) {
        print("Writing the gaze header!!!");
        for (int i = 0; i < GazeData.Count; i++)
        {
            file.Write(GazeData.ElementAt(i).Key);
            if (i < GazeData.Count - 1)
                file.Write(",");
        }
        file.Write(System.Environment.NewLine); 
    }

}
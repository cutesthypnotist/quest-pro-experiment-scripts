using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.IO;

// A hub for all the data we want to collect.
public class SubjectDataManager : MonoBehaviour {
    private static SubjectDataManager _instance;
    public static SubjectDataManager Instance { get { return _instance; } }
    
    
    [Header("Assign these in the inspector")]
    public GazeLogger gazeLogger;
    public RatingLogger ratingLogger;
    public QuizLogger quizLogger;
    
    private List<DataLogger> DataLoggers = new List<DataLogger>();

    
    public string subject_id;
    public int sync_id = 0;

    void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    void Start()
    {
        DataLoggers.Add(quizLogger);
        DataLoggers.Add(gazeLogger);
        DataLoggers.Add(ratingLogger);
            // string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() +
            // " " + DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Minute.ToString();

        
    }

    public void StartLogging() {
        foreach (DataLogger logger in DataLoggers)
        {
            logger.StartLogging();
        }
    }

    public void EndLogging() {
        foreach (DataLogger logger in DataLoggers)
        {
            logger.StopLogging();
        }
    }

    void FixedUpdate() {
        sync_id++;
    }


}
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class QuizLogger : DataLogger, IReceiveQuizUpdates {

    protected void Start() {
        base.Start();
        dataType = "quiz";
    }

    protected override void WriteHeader(System.IO.StreamWriter file)
    {
        SetOrAddData("question", "NA");
        SetOrAddData("answer", "NA");
        SetOrAddData("correct", "NA");
        base.WriteHeader(file);
    }    

    public void ReceiveQuizAnswer(string question, string answer, bool correct) {
        
        SetOrAddData("question", question);
        SetOrAddData("answer", answer);
        SetOrAddData("correct", correct.ToString());
        base.SetCommonData();
        string line = "";
        for (int i = 0; i < Data.Count; i++)
        {
            line += Data.ElementAt(i).Value;
            if (i < Data.Count - 1)
                line += ",";
        }
        lines.Add(line);

    }
    public void ReceiveQuizFinished()
    {
        throw new NotImplementedException();
    }
}

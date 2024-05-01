using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class RatingLogger : DataLogger, IReceiveRatingUpdates {

    protected void Start() {
        base.Start();
        dataType = "Rating";
    }

    public void UpdateRating(string ratingName, int ratingValue) {
        SetOrAddData("Question", ratingName);
        SetOrAddData("Rating", ratingValue.ToString());
        Debug.Log("[RatingLogger] " + ratingName + ": " + ratingValue);
        LogLine();
        WriteLines();
    }

    protected override void WriteHeader(StreamWriter file) {
        SetOrAddData("Question", "NA");
        SetOrAddData("Rating", "NA");
        base.WriteHeader(file);
    }

    //Call when ratings are done.
    public void FinishedRating()
    {
        base.LogLine();
    }    

}

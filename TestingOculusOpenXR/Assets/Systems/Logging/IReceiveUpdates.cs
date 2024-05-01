using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic interfaces for something that wants to receive udpates
/// </summary>
public interface IReceiveQuizUpdates
{
    void ReceiveQuizAnswer(string question, string answer, bool correct);
    void ReceiveQuizFinished();
}
public interface IReceiveRatingUpdates
{
    void UpdateRating(string ratingName, int ratingValue);
}

public interface IReceiveRankingData
{
    void SetRanking(string effect);
}

public interface IReceiveRankingUpdates
{
    void UpdateRanking(string rankingName, string effect);
}
public interface IReceiveObjectUpdates
{
    void UpdateObject(string name, float headAngle, float eyeAngle);
}
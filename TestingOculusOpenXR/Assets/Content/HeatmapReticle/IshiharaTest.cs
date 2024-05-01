using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class IshiharaQuestion {
    public Texture questionTexture;
    public int correctAnswer;
    public int selectedAnswer;
}


public class IshiharaTest : MonoBehaviour
{
    public List<IshiharaQuestion> questions = new List<IshiharaQuestion>();
    public GameObject endPage;
    public GameObject testUI;
    public Image questionImage;
    public GameObject screenUI;
    private int currentQuestionIndex = -1;
    public int score = 0;
    public int totalQuestions = 0;

    void Start()
    {
        
    }

    public void NextQuestion()
    {
        if (questions.Count == 0)
        {
            EndTest();
            return;
        }

        int index = Random.Range(0, questions.Count);
        currentQuestionIndex = index;
        DisplayQuestion(questions[index]);
    }

    private void DisplayQuestion(IshiharaQuestion question)
    {
        testUI.SetActive(true);
        Renderer renderer = screenUI.GetComponent<Renderer>(); // Make sure screenUI has a Renderer component.
        if (renderer != null)
        {
            renderer.material.mainTexture = question.questionTexture;
        }
        else
        {
            Debug.LogError("Renderer component not found on screenUI GameObject.");
        }
    }

    public void SelectAnswer(int answerIndex)
    {
        if (currentQuestionIndex != -1)
        {
            questions[currentQuestionIndex].selectedAnswer = answerIndex;
            questions.RemoveAt(currentQuestionIndex);
            NextQuestion();
        }
    }

    private void EndTest()
    {
        foreach (var question in questions)
        {
            if (question.selectedAnswer == question.correctAnswer)
            {
                score++;
            }
        }
        totalQuestions = questions.Count;


        // Handle the end of the test, e.g., calculate score, show results, etc.
        screenUI.SetActive(false);
        testUI.SetActive(false);
        endPage.SetActive(true);
        Debug.Log("Test completed.");

    }
}

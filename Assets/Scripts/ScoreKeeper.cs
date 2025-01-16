using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ScoreKeeper : MonoBehaviour
{
    int correctAnswers = 0; 
    int totalAnswered = 0;
    float percent;
    string scoreDisplay = "Score";

    // Start is called before the first frame update
    void Start()
    {

    }

    public void incrementScore(bool correct) {
        totalAnswered++;
        if (correct) {
            correctAnswers++;
        }
        percent = (float) correctAnswers / totalAnswered * 100;
        Debug.Log(string.Format("Score total:{0}, correct:{1}, new value:{2}", totalAnswered, correctAnswers, percent));
        scoreDisplay = string.Format("Score: {0:F2} %", percent);
    }

    public string getDisplay() {
        return scoreDisplay;
    }

    public float getPercent() {
        return percent;
    }
    public int getTotal() {
        return totalAnswered;
    }
    public int getCorrect() {
        return correctAnswers;
    }

    public void reset() {
        correctAnswers = totalAnswered = 0;
        percent = 0;
        scoreDisplay = "Score";
    }

    // Update is called once per frame
    void Update()
    {
    }
}

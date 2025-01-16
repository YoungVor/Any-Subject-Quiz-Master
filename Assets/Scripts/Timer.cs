using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    float timerValue;
    float timerTotal;
    bool clockRunning = true;
    [SerializeField] float questionTime = 30f;
    [SerializeField] float waitForNextTime = 10f;
    public float timeLeft; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        updateTimer();
    }

    private void updateTimer() {
        if (!clockRunning) {
            return;
        }

        timerValue -= Time.deltaTime;
        if (timerValue <= 0) {
            timerValue = 0;
            pauseTimer();
        }

        timeLeft = timerValue / timerTotal;

        //Debug.Log(string.Format("time left:{0}",timerValue));
    }

    public void resetTimer(bool waitingForNextQuestion) {
        timerValue = timerTotal = timeLeft = waitingForNextQuestion ? waitForNextTime : questionTime;
        clockRunning = true;
        Debug.Log("start timer: " + timerTotal);
    }

    public void pauseTimer() {
        clockRunning = false;
        Debug.Log("stop timer");
    }


    public void setQuestionTime(int val) {
        questionTime = val;
    }
    public void setDelay(int val) {
        waitForNextTime = val;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class MenuScript : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Dropdown quizDropdown;
    [SerializeField] TextMeshProUGUI labelQuestionCount;
    [SerializeField] TextMeshProUGUI labelDelay;
    [SerializeField] TextMeshProUGUI labelTimer;

    [SerializeField] Slider sliderQuestionCount;
    [SerializeField] Slider sliderDelay;
    [SerializeField] Slider sliderTimer;
    [SerializeField] int maxTimer = 121;
    [SerializeField] int maxDelay = 31;
    [SerializeField] int startTimer = 30;
    [SerializeField] int startDelay = 5;
    [SerializeField] int startQuestionCount = 40;
    bool readyPlayerFlag;
    Quiz quiz;
    String quizDataPath = "Assets/Quiz Data/";



    // Start is called before the first frame update
    void Start()
    {
        readyPlayerFlag = false;
        sliderDelay.maxValue = maxDelay;
        sliderDelay.value = startDelay;
        sliderTimer.value = startTimer;
        sliderTimer.maxValue = maxTimer;
        sliderQuestionCount.value = startQuestionCount;
        quiz = FindObjectOfType<Quiz>(includeInactive:true);
        populateDropdown();
        quizDropdown.enabled = true;

    }



    public void populateDropdown() {
        // iterate through sheets in asset directory
        Debug.Log("populateDropdown");
        DirectoryInfo dir = new DirectoryInfo(quizDataPath);
       FileInfo[] info = dir.GetFiles("*.csv");
       List<String> fileList = new List<String>();
       Debug.Log(String.Format("files found: {0}.  Dir exists:{1}", info.Length, dir.Exists));
       foreach (FileInfo f in info) {
        fileList.Add(f.Name);
       }
       quizDropdown.AddOptions(fileList);
    }

    public void selectQuiz() {
        Debug.Log("selectQuiz: " + quizDropdown.options[quizDropdown.value].text);

        quiz.readQuizData(quizDataPath + quizDropdown.options[quizDropdown.value].text);
    }

    public void startPressed() {
        //if () {

        //}
        Debug.Log("start pressed");
        if (quizDropdown.value != 0) {
            readyPlayerFlag = true;
        }
    }

    // Call readQuizData() to load the question when pulldown changes

    public bool readyPlayer() {
        return readyPlayerFlag;
    }

    public void updateLabelQuestionCount() {
        labelQuestionCount.text = string.Format("Question Count: {0}", sliderQuestionCount.value);
    }
    public void updateLabelDelay() {
        int sval = (int) sliderDelay.value;
        if (sval == maxDelay) {
            labelDelay.text = string.Format("Next Question Delay: Wait for Player");
            quiz.setQuestionDelay(int.MaxValue);
        } else {
            labelDelay.text = string.Format("Next Question Delay: {0}s", sval);
            quiz.setQuestionDelay(sval);
        }
    }
    public void updateLabelTimer() {
        int sval = (int) sliderTimer.value;
        int mval = 0;
        if (sval == maxDelay) {
            labelTimer.text = string.Format("Time Limit: Unlimited");
            quiz.setTimeout(int.MaxValue);
        } else {
            string label = "Time Limit:";
            quiz.setTimeout(sval); // before we reset sval to leftover seconds
            if (sval >= 60) {
                mval = sval / 60;
                sval = sval % 60;
                label += " " + mval.ToString() + "m";
            }
            if (sval > 0) {
                label += " " + sval.ToString() + "s";
            }
            labelTimer.text = label;
        }  

    }


}

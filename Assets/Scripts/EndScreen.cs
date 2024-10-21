using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] GameObject resetButton;
    [SerializeField] GameObject quitButton;
        // UI sprites
    [SerializeField] Sprite defaultButtonSprite;
    [SerializeField] Sprite pressedButtonSprite;
    ScoreKeeper scoreKeeper;

    bool kickOffEndscreen = false;
    bool playerRestarting = false;

    // Start is called before the first frame update
    void Awake()
    {
        scoreKeeper = FindObjectOfType<ScoreKeeper>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (kickOffEndscreen) {
            Debug.Log("EndScreen start coroutine");
            StartCoroutine(displayScore());
            kickOffEndscreen = false;
        }
    }

    private IEnumerator displayScore() {
        float fastType = 0.5f;
        float mediumType = 1f;
        float slowType = 2f;
        Debug.Log("start DisplayScore");
        string text = "";
        finalScoreText.text = text;
        yield return new WaitForSecondsRealtime(mediumType);
        text += "Quiz Ended!";
        finalScoreText.text = text; 
        Debug.Log(string.Format(" DisplayScore updated text to '{0}'",text));
        yield return new WaitForSecondsRealtime(slowType);
        text = "";
        finalScoreText.text = text; 
        yield return new WaitForSecondsRealtime(fastType);

        text += "Final";
        finalScoreText.text = text; 
        Debug.Log(string.Format(" DisplayScore updated text to '{0}'",text));
        yield return new WaitForSecondsRealtime(fastType);
        text += " Score:";
        finalScoreText.text = text;

        yield return new WaitForSecondsRealtime(mediumType);
        text += string.Format("     {0}", scoreKeeper.getCorrect());
        finalScoreText.text = text; 
        yield return new WaitForSecondsRealtime(fastType);
        text += " / ";
        finalScoreText.text = text;
        yield return new WaitForSecondsRealtime(fastType);
        text += string.Format("{0}", scoreKeeper.getTotal());
        finalScoreText.text = text; 
        yield return new WaitForSecondsRealtime(fastType);
        text += "\nAccuracy:";
        finalScoreText.text = text; 
        yield return new WaitForSecondsRealtime(fastType);
        text += string.Format("     {0}", scoreKeeper.getPercent());
        finalScoreText.text = text; 
        yield return new WaitForSecondsRealtime(fastType);
        text += "%";
        finalScoreText.text = text;

        yield return new WaitForSecondsRealtime(mediumType);
        text += "\nGrade:";
        finalScoreText.text = text;
        yield return new WaitForSecondsRealtime(slowType);
        text += "    " + giveGrade(scoreKeeper.getPercent());
        finalScoreText.text = text;
        yield return new WaitForSecondsRealtime(slowType);
        setButtonsActive();
        // todo: add stats like average time, longest time, total time
    }

    private string giveGrade(float percent) {
        if (percent == 100) {
            return "Perfect Score!";
        } else if (percent >= 90) {
            return "Well Done!";
        } else if (percent >= 70) {
            return "Not Bad.";
        } else if (percent >= 50) {
            return "Try Harder Next Time.";
        } else {
            return "You should be ashamed.";
        }
    }
    private void setButtonsActive() {
        Button rButton = resetButton.GetComponent<Button>();
        Image rButtonImg = resetButton.GetComponent<Image>();
        Button qButton = quitButton.GetComponent<Button>();
        Image qButtonImg = quitButton.GetComponent<Image>();
        rButtonImg.color = Color.white;
        rButton.interactable = true;
        qButtonImg.color = Color.white;
        qButton.interactable = true;
    }

    public void startEndScreen() {
        Debug.Log("EndScreen kickoff");
        resetEndScreen();
        kickOffEndscreen = true;
    }

    public void onButtonPressed(GameObject button) {
        Button rButton = resetButton.GetComponent<Button>();
        Button qButton = quitButton.GetComponent<Button>();
        Image buttonImg = button.GetComponent<Image>();
        rButton.interactable = true;
        qButton.interactable = true;
        buttonImg.sprite = pressedButtonSprite;
    }

    public void resetEndScreen() {
        kickOffEndscreen = false;
        playerRestarting = false;
        finalScoreText.text = "";
        Button rButton = resetButton.GetComponent<Button>();
        Image rButtonImg = resetButton.GetComponent<Image>();
        Button qButton = quitButton.GetComponent<Button>();
        Image qButtonImg = quitButton.GetComponent<Image>();
        rButtonImg.color = Color.clear;
        rButton.interactable = false;
        qButtonImg.color = Color.clear;
        qButton.interactable = false;
    }


}

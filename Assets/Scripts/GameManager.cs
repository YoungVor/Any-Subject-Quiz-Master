using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Quiz quiz;
    EndScreen endScreen;
    float transitionTimer = 0;
    bool endingGame = false;
    bool restartingGame = false;
    void Awake()
    {
        quiz = FindObjectOfType<Quiz>();
        endScreen = FindObjectOfType<EndScreen>();
    }

    void Start() {
        quiz.gameObject.SetActive(true);
        endScreen.gameObject.SetActive(false);
    }

    void Update()
    {
        // todo: add menu canvas
        if (quiz.gameObject.activeInHierarchy && quiz.getQuizState() == Quiz.QuizState.End) {
            quiz.quitQuiz();
            quiz.gameObject.SetActive(false);
            endScreen.gameObject.SetActive(true);
            endScreen.startEndScreen();
        }

        if (endScreen.gameObject.activeInHierarchy && transitionTimer > 0) {
            updateState();
        }
    }
    private void updateState() {
        transitionTimer -= Time.deltaTime;
        if (transitionTimer <= 0) {
            if (restartingGame) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            } else if (endingGame) {
                Application.Quit();
            }
        }
    }

    public void buttonPressed(GameObject button) {
        endScreen.onButtonPressed(button);
        transitionTimer = 2;
        restartingGame = true;
    }
    
    public void quitPressed() {
        Application.Quit();
    }
}
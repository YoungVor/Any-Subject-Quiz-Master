using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;


public class Quiz : MonoBehaviour
{
    [Header("User Parameters")]
    [SerializeField] TextAsset quizData;
    [SerializeField] int questionCount = 10;

    [Header("Question")]
    [SerializeField] TextMeshProUGUI questionUIText;
    QuestionSO currentQuestion;
    List<QuestionSO> questions;

    [Header("Buttons and Grades")]
    [SerializeField] GameObject commitButton;
    [SerializeField] GameObject[] answerButtons;
    [SerializeField] GameObject[] gradeObjects;
    [SerializeField] TextMeshProUGUI[] orderTexts;
    // UI sprites
    [SerializeField] Sprite defaultAnswerSprite;
    [SerializeField] Sprite selectedAnswerSprite;
    [SerializeField] Sprite correctGradeSprite;
    [SerializeField] Sprite wrongGradeSprite;

    [Header("Timer, Progress, ScoreKeeper")]
    [SerializeField] Timer timer;
    [SerializeField] Image timerImage;
    [SerializeField] ScoreKeeper scoreKeeper;
    [SerializeField] TextMeshProUGUI scoreKeeperText;
    [SerializeField] Slider progressBar;
    int[] answersSelected;
    private bool waitingForNextQuestion = false;
    private bool gradingQuestion = false;
    int orderCount = 0;

    private QuizState quizState;
    private bool commitPressed;
    int orderFontSize = 120;
    List<String> questionTable;

    public enum QuizState {
        Menu,
        SettingUpQuiz,
        PlayerAnswering,
        GradingAnswers,
        WaitingForNextQuestion,
        End
    }

    // Start is called before the first frame update
    void Awake()
    {
        timer = FindObjectOfType<Timer>();
        scoreKeeper = FindObjectOfType<ScoreKeeper>();

        questions = new List<QuestionSO> ();
        answersSelected = new int[5];
    }
    
    void Start() {
        // TODO
        //quizState = QuizState.Menu;
        quizState = QuizState.SettingUpQuiz;
    }

    void Update() {
        if (quizState == QuizState.PlayerAnswering) {
            timerImage.fillAmount = timer.timeLeft;
        }
        switch(quizState) {
            case QuizState.Menu:
                Assert.IsNull("Haven't implemented menu yet");
                break;
            case QuizState.SettingUpQuiz:
                scoreKeeper.reset();
                parseQuizData();
                setupQuestion();
                quizState = QuizState.PlayerAnswering;
                break;
            case QuizState.PlayerAnswering:
                if (timer.timeLeft <= 0 || commitPressed) { 
                    commitPressed = false;
                    quizState = QuizState.GradingAnswers;
                    commitAnswers();
                }  
                break;
            case QuizState.GradingAnswers:
                break;
            case QuizState.WaitingForNextQuestion:
                if (timer.timeLeft <= 0 || commitPressed) { 
                    commitPressed = false;
                    if (questions.Count > 0) {
                        setupQuestion();
                        quizState = QuizState.PlayerAnswering;
                    } else {
                        quizState = QuizState.End;
                    }
                }  
                break;
            case QuizState.End:
                Debug.Log("state: End Screen");
                break;
            default:
                Assert.IsTrue(false);
                return;
        }

    }

    public QuizState getQuizState() {
        return quizState;
    }

    public void quitQuiz() {
        quizState = QuizState.Menu;
    }
    public void resetQuiz() {
        quizState = QuizState.SettingUpQuiz;
        // todo: set new quiz data/questions 
        // todo: player settable fields like question count and timer
    }

    // entering state machine, player is answering
    private void setupQuestion() {
        Debug.Assert(quizState == QuizState.SettingUpQuiz || quizState == QuizState.WaitingForNextQuestion);
        setButtonState(false);
        // choose a random question
        int qId = UnityEngine.Random.Range(0, questions.Count);
        currentQuestion = questions[qId];
        questions.RemoveAt(qId);
        // todo: remove dups.
        // set up questionUI
        resetGradeMarkers();
        resetOrderMarkers();
        resetState();
        resetButtons();
        timer.resetTimer(false);
        timerImage.fillAmount = 1;
        quizState = QuizState.PlayerAnswering;
    }

    private void commitAnswers() {
        Debug.Log("commitAnswers");
        setButtonState(false);

        Image buttonImage = commitButton.GetComponent<Image>();
        buttonImage.sprite = selectedAnswerSprite;
        Debug.Log("call check answers coroutine");
        StartCoroutine(gradeAnswers());
    }

    public void readQuizData(string path) {

        // TODO: make this an async call and add state that blocks game start on its completion
        questionTable = new List<String>();
        Debug.Log("readQuizData:" + path);
        using (StreamReader sr = new StreamReader(path))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    questionTable.Insert(questionTable.Count, line);
                }
            }
        Debug.Log(string.Format("Finished parsing file {0}.  Question count: {1}", path, questionTable.Count));

    }

    public void setQuestionCount(int val) {
        questionCount = val;
    }
    public void setTimeout(int val) {
        timer.setQuestionTime(val);
    }
    public void setQuestionDelay(int val) {
        timer.setDelay(val);
    }

    private void parseQuizData() {
        //questionTable = quizData.text.Split("\n", StringSplitOptions.None);
        
        Debug.Assert(questionTable.Count > 0);
        string[] heading = questionTable[0].Split(",", StringSplitOptions.None);
        //Debug.Log("Parsed heading: " + String.Join(",",heading));
        
        // we will parse and load X random questions
        //List<bool> parsedQuestions  = new List<bool>(questionLines.Length){false};
        bool[] parsedQuestions = new bool[questionTable.Count];
        for (int i = 1; i < questionTable.Count; i++) { parsedQuestions[i] = false; };
        parsedQuestions[0] = true; // remove header

        for (int i = 0; i < questionCount; i++) {
            //Debug.Log(String.Format("call parse and add question: cq:{0}, ql:{1}", parsedQuestions.Length, questionLines.Length));
            bool noMoreQuestions = parseAndAddRandomQuestion(ref parsedQuestions);
            string dbgList = "";
            for (int j = 0; j < parsedQuestions.Length; j++) { dbgList += string.Format("{0}:{1},", j.ToString(), parsedQuestions[j].ToString()); }
            Debug.Log("bool array:" + dbgList);
            if (noMoreQuestions) { break; }
        }
        progressBar.maxValue = questions.Count;
        progressBar.value = 0;
        Debug.Log(string.Format("progress max:{0}, val:{1}", progressBar.maxValue, progressBar.value));

        Debug.Log("Finished parsing question data.  Question count:" + questions.Count);
    }

    // grab random X choices in array
    // return true if empty (fewer available questions than questionCount)
    private bool parseAndAddRandomQuestion(ref bool[] chosenQuestions) {
        int startIndex;
        int chosenIndex;
        
        Debug.Log(String.Format("parse and add question: cq:{0}, ql:{1}", chosenQuestions.Length, questionCount));
        while (true) {
            startIndex = chosenIndex = UnityEngine.Random.Range(1,questionTable.Count);
            while (chosenQuestions[chosenIndex] == true) {
                chosenIndex++;
                if (chosenIndex >= questionTable.Count) { chosenIndex = 1; } // skip first one }
                if (chosenIndex == startIndex) { return true; }
            } 

            Debug.Log("parsing question index:" + chosenIndex);
            chosenQuestions[chosenIndex] = true;
            string [] questionText = questionTable[chosenIndex].Split(",", StringSplitOptions.None);
            QuestionSO question = ScriptableObject.CreateInstance<QuestionSO>();

            try {
                question.parseQuestionText(questionText);
            } catch (Exception e) {
                Debug.Log(string.Format("Parsing Failure, threw exception:{0}", e.ToString()));
                continue;
            }
            if (question.error != QuestionErrorType.None) {
                Debug.Log("Warning: Failed to parse question error:" + question.error);
                continue; // let it get garbage collected
            }
            Debug.Log("Adding question to list: " + string.Join(",", questionText));
            questions.Add(question);
            break;
        }
        return false;
    }

    public void OnAnswerSelectionPressed(int index) {
        if (quizState != QuizState.PlayerAnswering) {
            return;
        }
        Debug.Log(string.Format("pressing button:{0} orderCount:{1} prevState:{2}", index, orderCount, answersSelected[index]));
        GameObject button = answerButtons[index];
        TextMeshProUGUI commitText = commitButton.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = button.GetComponent<Image>();
        if (currentQuestion.GetQuestionType() == QuestionTypeValue.ChooseOne) {
            if (orderCount > 0) { // only one can ever be selected
                resetButtons();
                resetState();
                resetOrderMarkers();
            }
        } else if (currentQuestion.GetQuestionType() == QuestionTypeValue.Order) {     
            if (answersSelected[index] >= 0) {
                // check if already selected
                resetButtons();
                resetState();
                resetOrderMarkers();
                return;
            }
        } else if (currentQuestion.GetQuestionType() == QuestionTypeValue.AllThatApply) {
            if (answersSelected[index] >= 0) {
                answersSelected[index] = -1;
                buttonImage.sprite = defaultAnswerSprite;
                orderCount--;
                if (orderCount == 0) {
                    commitText.text = "";
                }
                return;
            }
        }
        buttonImage.sprite = selectedAnswerSprite;
        commitText.text = "Final Answer?";
        if (currentQuestion.GetQuestionType() == QuestionTypeValue.Order) {
            answersSelected[index] = orderCount;
            orderTexts[index].text = (orderCount + 1).ToString();
        } else {
            answersSelected[index] = 1;
        }
        orderCount++; 
    }

    public void OnCommitButtonPressed() {
        Debug.Log(string.Format("Pressed Commit.  State:{0} orderCount:{1}", quizState, orderCount));
        if (quizState != QuizState.PlayerAnswering && quizState != QuizState.WaitingForNextQuestion) {
            return;
        }
        if (quizState == QuizState.PlayerAnswering)
        {
            if ( orderCount < 0) { // TODO: check count depending on type of question
                return;
            }
        }

        if (quizState == QuizState.WaitingForNextQuestion || quizState == QuizState.PlayerAnswering) {
            commitPressed = true;
        }
    }

    private IEnumerator gradeAnswers() {
        Debug.Log("start check answers wait");
        yield return new WaitForSecondsRealtime(1.5f);
        Debug.Log("start check answers");
        
        int[] correctAnswers = currentQuestion.GetCorrectAnswerValueOrders();
        bool correct = true;
        for (int i = 0; i < answersSelected.Length; i++) {
            if (answersSelected[i] > -1) {
                Image gradeImage = gradeObjects[i].GetComponent<Image>();
                Image buttonImage = answerButtons[i].GetComponent<Image>();
                gradeImage.color = Color.white;
                if (correctAnswers[i] != answersSelected[i]) { 
                    correct = false;
                    gradeImage.sprite = wrongGradeSprite;
                    buttonImage.color = Color.red;
                    if (currentQuestion.GetQuestionType() == QuestionTypeValue.Order) { orderTexts[i].color = Color.red; }
                } else {
                    gradeImage.sprite = correctGradeSprite;
                    buttonImage.color = Color.green;
                    if (currentQuestion.GetQuestionType() == QuestionTypeValue.Order) { orderTexts[i].color = Color.green; }
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(0.5f);
        for (int i = 0; i < correctAnswers.Length; i++) {
            // Show correct values
            if (correctAnswers[i] >= 0) {
                // button was never selected
                if (answersSelected[i] < 0) { 
                    Image buttonImage = answerButtons[i].GetComponent<Image>();
                    buttonImage.color = Color.yellow;
                    correct = false;
                }
                // wrong order
                if (currentQuestion.GetQuestionType() == QuestionTypeValue.Order &&
                    answersSelected[i] != correctAnswers[i]) { 
                    orderTexts[i].text = (correctAnswers[i] + 1).ToString();
                    orderTexts[i].color = Color.yellow;
                    orderTexts[i].fontSize = orderFontSize + 10;
                    correct = false;
                }
            }
            yield return new WaitForSecondsRealtime(0.1f);
        }
        yield return new WaitForSecondsRealtime(correct ? 0.2f : 1); // wait longer if wrong

        //update Commit button
        setCommitButton(false, questions.Count == 0 ? "See final score" : "Next Question");
        // update progress bar
        progressBar.value = progressBar.maxValue-questions.Count;
        Debug.Log(string.Format("progress max:{0}, val:{1}", progressBar.maxValue, progressBar.value));
        // update score and timer
        scoreKeeper.incrementScore(correct);
        scoreKeeperText.text = scoreKeeper.getDisplay();
        timer.resetTimer(true);
        // update quiz state
        quizState = QuizState.WaitingForNextQuestion;
    }

    private void resetButtons() {
        questionUIText.text = currentQuestion.GetQuestion();
        string[] answers = currentQuestion.GetPossibleAnswers();
        for (int i = 0; i < answerButtons.Length; i++) {
            TextMeshProUGUI buttonText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            Button button = answerButtons[i].GetComponent<Button>();
            Image buttonImg = answerButtons[i].GetComponent<Image>();
            if (i < answers.Length) {
                buttonImg.color = Color.white;
                buttonImg.sprite = defaultAnswerSprite;
                button.interactable = true;
                buttonText.text = answers[i];
            } else {
                buttonImg.color = Color.clear;
                button.interactable = false;
                buttonText.text = "";
            }
            
        }
        // set commit button
        setCommitButton(false, "");
    }

    private void setCommitButton(bool pressed, string text) {
        TextMeshProUGUI commitText = commitButton.GetComponentInChildren<TextMeshProUGUI>();
        Image commitImage = commitButton.GetComponent<Image>();
        Button cbutton = commitButton.GetComponent<Button>();
        commitText.text = text;
        if (pressed) {
            cbutton.interactable = false;
            commitImage.sprite = selectedAnswerSprite;
        } else {
            cbutton.interactable = true;
            commitImage.sprite = defaultAnswerSprite;
        }
    }

    private void resetOrderMarkers() {
        for (int i = 0; i < orderTexts.Length; i++) {
            TextMeshProUGUI orderMarker = orderTexts[i];
            orderMarker.text = "";
            orderMarker.fontSize = orderFontSize;
        }
    }

    private void resetState() {
        for (int i = 0; i < answersSelected.Length; i++) {
            answersSelected[i] = -1;
        }
        orderCount = 0;
    }

    private void resetGradeMarkers() {
        for (int i = 0; i < gradeObjects.Length; i++) {
            GameObject gradeMarker = gradeObjects[i];
            Image gradeImage = gradeMarker.GetComponent<Image>();
            gradeImage.sprite = null;
            gradeImage.color = Color.clear;
        }
    }

    private void setButtonState(bool enabled) {
        for (int i = 0; i < answerButtons.Length; i++) {
            Button b = answerButtons[i].GetComponent<Button>();
            b.interactable = enabled;
        }
        Button cb = commitButton.GetComponent<Button>();
        cb.interactable = enabled;
    }

}

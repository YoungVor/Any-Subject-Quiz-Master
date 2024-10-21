using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum QuestionTypeValue {
    BadType = 0,
    ChooseOne,
    AllThatApply,
    Order
}

public enum QuestionErrorType {
    None = 0,
    BadType,
    MalformedNoBreak,
    NoCorrectAnswers,
    AnswerOutOfBounds,
    BadCorrectAnswerCount,
    MalformedCorrectAnswerIndex,

}

public class QuestionType {
    public QuestionTypeValue type;

    public static QuestionTypeValue getQuestionType(string typeIn) {
        //Debug.Log("createEnum:" + typeIn);
        switch (typeIn) {
            case "ChooseOne" : 
                return QuestionTypeValue.ChooseOne;
            case "AllThatApply" : 
                return QuestionTypeValue.AllThatApply;
            case "Order" : 
                return QuestionTypeValue.Order;
        }
        return QuestionTypeValue.BadType;
    }
};

[CreateAssetMenu(menuName = "Quiz Question", fileName = "New Question")]
public class QuestionSO : ScriptableObject
{
    [TextArea(2,6)] 
    string question = "Enter new question text here";
    List<(string,int)> answerTuples;
    //int[] correctAnswers;
    // [SerializeField] int correctAnswerId;

    public QuestionTypeValue type;
    public QuestionErrorType error = QuestionErrorType.None;


    public QuestionSO() {
        answerTuples = new List<(string,int)>();
        //correctAnswers = new int[5] {-1, -1, -1, -1, -1};
    }

    public QuestionTypeValue GetQuestionType() {
        return type;
    }

    public string GetQuestion() {
        return question;
    }

    public string[] GetPossibleAnswers() {
        string[] possibleAnswers = new string[answerTuples.Count];
        for (int i = 0; i < answerTuples.Count; i++) {
            possibleAnswers[i] = answerTuples[i].Item1;
        }
        return possibleAnswers;
    }

    public int[] GetCorrectAnswerValueOrders() {
        int[] correctAnswerValueOrders = new int[answerTuples.Count];
        for (int i = 0; i < answerTuples.Count; i++) {
            correctAnswerValueOrders[i] = answerTuples[i].Item2;
        }
        return correctAnswerValueOrders;
    }

    private void shuffleAnswers() {
        List<(string, int)> newAnswerTuples = new List<(string, int)>();
        while (answerTuples.Count > 0) { 
            int randIndex = UnityEngine.Random.Range(0, answerTuples.Count);
            newAnswerTuples.Add(answerTuples[randIndex]);
            answerTuples.RemoveAt(randIndex); 
        }
        answerTuples = newAnswerTuples;
    }

    // returns int array of size (5) (total possible slots in the game), either 1 (if order doesnt matter) or order index for each correct value
    //public int[] GetCorrectAnswerIndexes() {
    //    return correctAnswers;
    //}

    public void parseQuestionText(string[] questionText) {
        int id = 0;
        //Debug.Log(string.Format("Parsing question line length:{0}, string:'{1}'", questionText.Length, string.Join(",", questionText)));
        question = questionText[id++];
        type = QuestionType.getQuestionType(questionText[id++]);
        if (CheckState(type != 0, QuestionErrorType.BadType) != 0) { return; };
        ParseAnswers(ref id, ref questionText);
        ParseCorrectAnswers(ref id, ref questionText);
        shuffleAnswers();
    }

    private void ParseAnswers(ref int id, ref string[] questionText) {
        while (questionText[id] != "_BREAK ANSWERS_") {
            //Debug.Log(string.Format("parsing possible answer:{0}.  id:{1}", questionText[id], id));
            string nextAnswer = questionText[id++];
            if (!string.IsNullOrWhiteSpace(nextAnswer)) { 
                (string answer, int orderValue) answerTuple = (nextAnswer, -1);
                answerTuples.Add(answerTuple); 
            }
            if (CheckState(id < questionText.Length, QuestionErrorType.MalformedNoBreak) != 0) { return; };
        }
        id++;
    }

    private QuestionErrorType CheckState(bool check, QuestionErrorType ecode) {
        if (!check) { error = ecode;}
        return error;
    }

    private void ParseCorrectAnswers(ref int id, ref string[] questionText) {
        //Debug.Log("parse question begin");
        if (CheckState(id < questionText.Length, QuestionErrorType.BadCorrectAnswerCount) != 0) { return; };

        int answerCount = 0;
        while (id < questionText.Length && !string.IsNullOrWhiteSpace(questionText[id])) {
            //Debug.Log(string.Format("parsing CORRECT answer:{0}.  id:{1}, question:'{2}'", questionText[id], id, string.Join(",", questionText)));
            int answerIndex;
            try {
                answerIndex = int.Parse(questionText[id]);
            } catch (Exception e) {
                Debug.Log(string.Format("failed to parse correct answer index:'{0}', equal?:{1}, exception:{2}", questionText[id], questionText[id]==null, e.ToString()));
                error = QuestionErrorType.MalformedCorrectAnswerIndex;
                return;
            }
            id++;
            if (CheckState((answerIndex < answerTuples.Count && answerIndex >= 0), QuestionErrorType.AnswerOutOfBounds) != 0) { return; };
            answerTuples[answerIndex] = (answerTuples[answerIndex].Item1, (type == QuestionTypeValue.Order) ? answerCount : 1);
            answerCount++;
        }
        //Debug.Log("correct answer list: " + correctAnswers.ToString());
        switch(type) {
            case QuestionTypeValue.ChooseOne:
                if (CheckState((answerCount == 1), QuestionErrorType.BadCorrectAnswerCount) != 0) { return; };
                break;
            case QuestionTypeValue.AllThatApply:
                if (CheckState((answerCount <= answerTuples.Count && answerCount >= 1), QuestionErrorType.BadCorrectAnswerCount) != 0) { return; };
                break;
            case QuestionTypeValue.Order:
                if (CheckState((answerCount == answerTuples.Count), QuestionErrorType.BadCorrectAnswerCount) != 0) { return; };
                break;
            default:
                error = QuestionErrorType.BadType;
                return;
        }
        //Debug.Log("parse question end");
    }
}


using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public enum QuizState{PreGame, Quizing, Intermission, Review}

public class QuizManager : MonoBehaviour
{
    [SerializeField] public List<QuestionClass> QuestionList;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI explanationText;
    public TextMeshProUGUI countDownText;
    public List<TextMeshProUGUI> textList;
    public List<bool> answerArray;

    private int MinAnswers;
    [SerializeField] private int InterissionTimer = 3;

    private QuizState quizState = QuizState.PreGame;
    private int QuestionCount = 0;
    private bool enteringState = true;
    private bool EndCountDown;

    private QuestionClass CurrentQuestion;

    public void AssignQuestion(QuestionClass question)
    {
        QuestionList.Add(question);
        question.QuestionIndex = QuestionList.Count - 1;
        question.IndexCheck = question.QuestionIndex;
        Debug.Log(question.Question + " Has been added to the quiz Manager");
        UpdateMinAnswers();

    }

    private void UpdateMinAnswers()
    {
        MinAnswers = 0;

        foreach(QuestionClass question in QuestionList)
        {
            if (MinAnswers < question.Answers.Count)
            {
                MinAnswers = question.Answers.Count;
            }
        }

        if(MinAnswers >= textList.Count)
        {
            int diff = MinAnswers - textList.Count;

            for(int i = 0; i < diff; i++)
            {
                textList.Add(null);
            }
        }

        if(MinAnswers >= answerArray.Count)
        {
            int diff = MinAnswers - answerArray.Count;

            for(int i = 0; i < diff; i++)
            {
                answerArray.Add(false);
            }
        }
    }

    public void ChangeListOrder(QuestionClass question, int value)
    {
        int currentindex = QuestionList.IndexOf(question);

        if(currentindex + value < 0 || currentindex + value >= QuestionList.Count)
        {
            Debug.Log("Index goes outside of possible values");
            return;
        }

        value += currentindex;

        QuestionList.Remove(question);
        QuestionList.Insert(value, question);
    }

    public void QuizState()
    {
        switch(quizState)
        {
            case QuizState.PreGame:
            {
                break;
            }
            case QuizState.Quizing:
            {
                StartQuestion(QuestionCount);
                break;
            }
            case QuizState.Intermission:
            {
                StartIntermission(QuestionCount);
                break;
            }
            case QuizState.Review:
            {
                break;
            }
        }
    }

    private void StartQuestion(int CurrentQuestionCount)
    {
        if(enteringState == true)
        {
            enteringState = false;

            CurrentQuestion = QuestionList[CurrentQuestionCount];
            titleText.text = CurrentQuestion.Question;
            for (int i = 0; i < QuestionList.Count; i++)
            {
                textList[i].text = CurrentQuestion.Answers[i];
            }

            StartCoroutine(CountdownMethod(CurrentQuestion.Timer, i));
        }
    }

    public void UIButtonAnswerMethod(int AnswerNumber)
    {
        if(quizState == QuizState.Quizing)
        {
            answerArray[QuestionCount] = CurrentQuestion.AnswerKey[AnswerNumber - 1];
            EndCountDown = true;
            FinishQuestion();
        }
        
    }

    private IEnumerator CountdownMethod(int Count, int Index)
    {
        EndCountDown = false;
        while(Count >= 0)
        {
            if(EndCountDown == true)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            Count -= 1;
        }

        
        switch(quizState)
        {
            
            case QuizState.Quizing:
            {
                answerArray[i] = false;
                FinishQuestion();
                break;
            }
            case QuizState.Intermission:
            {
                QuestionCount++;
                enteringState == true;
                break;
            }
        }
        
    }

    private void FinishQuestion()
    {
        quizState = QuizState.Intermission;
        enteringState = true;

        if(QuestionCount + 1 == QuestionList.Count)
        {
            quizState = QuizState.Review;
        }
    }

    private void StartIntermission(int QuestionCount)
    {
        if(enteringState == true)
        {
            enteringState = false;
            titleText.text = CurrentQuestion.Explanation;
        
            for (int i = 0; i < QuestionList.Count; i++)
            {
                if(CurrentQuestion.AnswerKey[i] == false)
                {
                    textList[i].text = "";
                }
            }

            StartCoroutine(CountdownMethod(InterissionTimer, i));
        }
    }

    private void EnteringReviewState()
    {
        int score = 0;
        string reviewString = "";
        for (int i = 0; i < QuestionList.Count; i++)
        {
            QuestionClass question = QuestionList[i];
            reviewString = reviewString + "Question " + (i+1) + ": " + question.Question + "  -  Answered: ";
            if(answerArray[i] == true)
            {
                reviewString = reviewString + " Correct. ";
                score ++;
            }
            else
            {
                reviewString = reviewString + " Incorrect. ";
            }

            reviewString = reviewString + "<br> the correct answer is: ";

            for(int A = 0; A < AnswerKey.Count; A++)
            {
                if(question.AnswerKey[A] = true)
                {
                    reviewString = reviewString + question.Answers[A] + ", ";
                }
            }  
                
            reviewString = reviewString + "<br> Explanation: " + question.Explanation + "<br>";
        }

        reviewString = reviewString + " Final Score: " + score + "/" + QuestionList.Count;
    }


}

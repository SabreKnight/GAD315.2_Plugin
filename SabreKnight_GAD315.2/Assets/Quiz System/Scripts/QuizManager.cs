using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Text;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [SerializeField] public List<QuestionClass> QuestionList;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI ReviewText;
    public TextMeshProUGUI countDownText;
    public TMP_InputField nameInputField;
    public List<TextMeshProUGUI> textList;
    [HideInInspector] public List<bool> answerArray;
    public List<GameObject> ReviewDisableObjectList = new List<GameObject>();
    public List<GameObject> ReviewEnableObjectList = new List<GameObject>();

    private int MinAnswers;
    [SerializeField] public int InterissionTimer = 3;

     private QuizState quizState = QuizState.PreGame;
    //private int quizState = 1;
     private int QuestionCount = 0;
    private bool enteringState = true;
    private bool EndCountDown;

    private int score = 0;
    private int maxScore = 0;

    private Coroutine CurrentCountdown;
    private QuestionClass CurrentQuestion;

    public void AssignQuestion(QuestionClass question)
    {
        QuestionList.Add(question);
        question.QuestionIndex = QuestionList.Count - 1;
        question.IndexCheck = question.QuestionIndex;
        Debug.Log(question.Question + " Has been added to the quiz Manager");
        UpdateMinAnswers();

    }

    public void ClearManager()
    {
        QuestionList.Clear();
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

    private void Update()
    {
        QuizStateMethod();
    }

    public void QuizStateMethod()
    {
        switch(quizState)
        {
            case QuizState.PreGame:
            {
                PreGameStart();
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
                EnteringReviewState();
                break;
            }
        }
    }

    private void PreGameStart()
    {
        foreach(GameObject obj in ReviewDisableObjectList)
        {
            obj.SetActive(true);
        }
        foreach(GameObject obj in ReviewEnableObjectList)
        {
            obj.SetActive(false);
        }

        answerArray.Clear();
        quizState = QuizState.Quizing;
    }

    private void StartQuestion(int CurrentQuestionCount)
    {
        if(enteringState == true)
        {
            enteringState = false;

            CurrentQuestion = QuestionList[CurrentQuestionCount];
            titleText.text = CurrentQuestion.Question;
            for (int i = 0; i <= QuestionList.Count; i++)
            {
                textList[i].text = CurrentQuestion.Answers[i];
            }

            CurrentCountdown = StartCoroutine(CountdownMethod(CurrentQuestion.Timer));
        }
    }

    public void UIButtonAnswerMethod(int AnswerNumber)
    {
        if(quizState == QuizState.Quizing)
        {
            answerArray.Add(CurrentQuestion.AnswerKey[AnswerNumber - 1]);
            if(CurrentCountdown != null)
            {
                StopCoroutine(CurrentCountdown);
                CurrentCountdown = null;
            }
            EndCountDown = true;
            FinishQuestion();
        }
        
    }

    private IEnumerator CountdownMethod(int Count)
    {
        EndCountDown = false;
        while(Count >= 0)
        {
            countDownText.text = Count + "S";
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
                
                answerArray.Add(false);
                enteringState = true;
                FinishQuestion();
                break;
            }
            case QuizState.Intermission:
            {
                QuestionCount++;
                enteringState = true;
                if(QuestionCount >= QuestionList.Count)
                {
                    quizState = QuizState.Review;
                }
                else
                {
                    quizState = QuizState.Quizing;
                }
                break;
            }
        }
        
    }

    private void FinishQuestion()
    {
        quizState = QuizState.Intermission;
        enteringState = true;
    }

    private void StartIntermission(int QuestionCount)
    {
        if(enteringState == true)
        {
            enteringState = false;
            titleText.text = CurrentQuestion.Explanation;
        
            for (int i = 0; i <= QuestionList.Count; i++)
            {
                if(CurrentQuestion.AnswerKey[i] == false)
                {
                    textList[i].text = "";
                }
            }

            StartCoroutine(CountdownMethod(InterissionTimer));
        }
    }

    private void EnteringReviewState()
    {
        if(enteringState == false)
        {
            return;
        }
        enteringState = false;
        string reviewString = "";
        for (int i = 0; i < QuestionList.Count; i++)
        {
            QuestionClass question = QuestionList[i];
            reviewString = reviewString + "Question " + (i+1) + ": " + question.Question + "  -  Answered: ";
            if(answerArray[i] == true)
            {
                reviewString = reviewString + " Correct. ";
                score += QuestionList[i].PointValue;;
            }
            else
            {
                reviewString = reviewString + " Incorrect. ";
            }

            maxScore += QuestionList[i].PointValue;

            reviewString = reviewString + "<br> the correct answer is: ";

            for(int A = 0; A < QuestionList[i].AnswerKey.Count; A++)
            {
                if(question.AnswerKey[A] == true)
                {
                    reviewString = reviewString + question.Answers[A] + ", ";
                }
            }  
                
            reviewString = reviewString + "<br> Explanation: " + question.Explanation + "<br>";
        }

        reviewString = reviewString + " Final Score: " + score + "/" + maxScore;
        ReviewText.text = reviewString;

        foreach(GameObject obj in ReviewDisableObjectList)
        {
            obj.SetActive(false);
        }
        foreach(GameObject obj in ReviewEnableObjectList)
        {
            obj.SetActive(true);
        }
    }

    public void WriteResponseData()
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            
            string userInput = nameInputField.text;

            if(string.IsNullOrEmpty(userInput))
            {
                userInput = "Anonymous";
            }

            string filePath = Path.Combine(Application.persistentDataPath, userInput +"_ResponseAnalytics.save");

        
            if (!File.Exists(filePath))
            {
                Debug.LogError($"[CSV Tool] CSV file not found at {filePath}. Likely new test taker");
                //sb.AppendLine(userInput + " - Quiz response Data:");
            }

            try
            {
                sb.AppendLine(userInput + " - Quiz response Data");
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 1; i < lines.Length; i++)
                {
                    sb.AppendLine(lines[i]);
                }
            }
            catch (Exception ex)
            {
                //Debug.LogError($"[Quiz Tool] Failed to read file: {ex.Message}");
            }
            

            sb.AppendLine("----");

            for (int i = 0; i < QuestionList.Count; i++)
            {
                QuestionClass question = QuestionList[i];

                string questionLine = "Question " + (i+1) + ": " + question.Question + "  -  Answered: ";

                if(answerArray[i] == true)
                {
                    questionLine = questionLine + " Correct. ";
                }
                else
                {
                    questionLine = questionLine + " Incorrect. ";
                }

                questionLine = questionLine + " - Correct Answer: ";

                for(int A = 0; A < QuestionList[i].AnswerKey.Count; A++)
                {
                    if(question.AnswerKey[A] == true)
                    {
                        questionLine = questionLine + question.Answers[A] + ", ";
                    }
                } 

                sb.AppendLine(questionLine);
            }
            sb.AppendLine("Final Score: " + score + "/" + maxScore);

            File.WriteAllText(filePath, sb.ToString());
            Debug.Log($"[Quiz Tool] Successfully wrote CSV to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Quiz Tool] Failed to write CSV {ex.Message}");
        }
    }
}

public enum QuizState { PreGame, Quizing, Intermission, Review }
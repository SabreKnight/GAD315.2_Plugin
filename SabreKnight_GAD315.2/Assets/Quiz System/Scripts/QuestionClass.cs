using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class QuestionClass 
{
    public string Question;
    public int Timer = 30;
    public string Category = "Misc";
    public List<string> Answers = new List<string>();
    public List<bool> AnswerKey = new List<bool>();
    public string Explanation;

    [HideInInspector] public bool displayFolderBool = false;
    [HideInInspector] public bool SelectedQuestion = false;
    [HideInInspector] public int IndexCheck;
    [HideInInspector] public int QuestionIndex;

    public QuestionClass()
    {
        Timer = 30;
        Category = "Misc";
    }

}

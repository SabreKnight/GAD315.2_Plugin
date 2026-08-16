using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class QuestionClass 
{
    public string Question;
    public int Timer = 30;
    public string Category = "Misc";
    public List<string> Answers = new List<string>();
    public List<bool> AnswerKey = new List<bool>();

    public bool displayFolderBool = true;

    public QuestionClass()
    {
        Timer = 30;
        Category = "Misc";
    }

    public QuestionClass(string Q, int I, string C)
    {
        Question = Q;
        Timer = I;
        Category = C;
    }
}

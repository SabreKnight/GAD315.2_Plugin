using UnityEngine;
using UnityEditor;
using UnityEditorInternal; 
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class QuizEditorWindow : EditorWindow
{
    private static Vector2 scrollPos;
    private static QuizManager quizManagerRef;
    private static List<QuestionClass> questionList;

    [MenuItem("Quiz Tools/Quiz Editor")] // location of the editor window
    public static void ShowWindow() // opens editor window
    {
        QuizEditorWindow window = GetWindow<QuizEditorWindow>("Quiz Editor");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GetManager();
        DisplayQuestions();
        DisplayTMPro();
        EditorGUILayout.EndScrollView();

    }

    private void DisplayQuestions()
    {
        if(quizManagerRef.QuestionList == null)
        {
            return;
        }

        int ChangeVal = 0;
        QuestionClass classref = null;
        QuestionClass removeRef = null;
        for (int i = 0; i < quizManagerRef.QuestionList.Count; i++)
        {
            string title = "Question " + (i+1) + ": " + quizManagerRef.QuestionList[i].Question;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("- Change Question Index: ", EditorStyles.boldLabel);

            if (GUILayout.Button("-1", GUILayout.Width(30)))   // creates a button for the user
            {
                classref = quizManagerRef.QuestionList[i];
                ChangeVal = -1;
            }

            if (GUILayout.Button("+1", GUILayout.Width(30)))   // creates a button for the user
            {
                classref = quizManagerRef.QuestionList[i];
                ChangeVal = 1;
            }
            if (GUILayout.Button("Remove question"))   // creates a button for the user
            {
                removeRef = quizManagerRef.QuestionList[i];
            }

            EditorGUILayout.EndHorizontal();

        }

        if(classref != null)
        {
            quizManagerRef.ChangeListOrder(classref, ChangeVal);
        }
        if(removeRef != null)
        {
            quizManagerRef.QuestionList.Remove(removeRef);
        }
    }

    private void DisplayTMPro()
    {
        GUILayout.Space(10);
        quizManagerRef.titleText = (TextMeshProUGUI)EditorGUILayout.ObjectField("Question UI text: ", quizManagerRef.titleText, typeof(TextMeshProUGUI), true);
        GUILayout.Space(3);
        bool TextCheck = false;
        for(int i = 0; i <= quizManagerRef.QuestionList.Count; i++)
        {
            quizManagerRef.textList[i] = (TextMeshProUGUI)EditorGUILayout.ObjectField("Answer/Button " + (i+1) + " UI text: ", quizManagerRef.textList[i], typeof(TextMeshProUGUI), true);
            if(quizManagerRef.textList[i] == null)
            {
                TextCheck = true;
            }
        }

        if(TextCheck == true)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Text Mesh Pro GUI fields must have a reference!", EditorStyles.boldLabel);
        }
        
    }

    private void GetManager()
    {
        GUILayout.Space(10);
        if(quizManagerRef == null)
        {
            QuizManager[] managerArray = FindObjectsByType<QuizManager>(FindObjectsSortMode.InstanceID);

            if(managerArray == null || managerArray.Length == 0)
            {
                GameObject newObj = new GameObject("Quiz System Manager ");
                quizManagerRef = newObj.AddComponent<QuizManager>();

            }
            else if(managerArray.Length > 1)
            {
                EditorGUILayout.LabelField("Multiple Managers in scene - Quiz Manager must be assigned", EditorStyles.boldLabel);
            }
            else
            {
                quizManagerRef = managerArray[0];
            }
        }

        QuizManager TempquizManagerRef = (QuizManager)EditorGUILayout.ObjectField("Quiz Manager:", quizManagerRef, typeof(QuizManager), true);

        quizManagerRef = TempquizManagerRef;
    }
}

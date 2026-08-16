using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Text;

public class QuizLibraryWindow : EditorWindow
{
    private static string LibraryDataPath = "LibraryFile";

    // organises all quesitons based on their category, then each category goes their respective list
    public static Dictionary<string, List<QuestionClass>> LibraryDictionary = new Dictionary<string, List<QuestionClass>>();
    public static QuestionClass displayQuestion;

    private static bool CategoryFolder = true;
    private static Vector2 scrollPos;


    [MenuItem("Quiz Tools/Question Library")] // location of the editor window
    public static void ShowWindow() // opens editor window
    {
        QuizLibraryWindow window = GetWindow<QuizLibraryWindow>("Question Library");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        LibraryDataPath = EditorGUILayout.TextField("Library CSV File: ", LibraryDataPath);
        if (GUILayout.Button("Save Quiz Library"))   // creates a button for the user
        {
            WriteLibraryData();
        }

        if (GUILayout.Button("read Quiz Library"))   // creates a button for the user
        {
            ReadLibraryData();
        }

        if (GUILayout.Button("Clear Question Library"))   // creates a button for the user
        {
            LibraryDictionary.Clear();
        }

        DisplayQuestion();
        EditorGUILayout.EndScrollView();
    }

    void CullQuestions()
    {

        foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
        {
            List<QuestionClass> cullList = new List<QuestionClass>();

            foreach(QuestionClass question in Category.Value)
            {
                if(string.IsNullOrEmpty(question.Question))
                {
                    cullList.Add(question);
                
                }
            }

            foreach(QuestionClass question in cullList)
            {
                Debug.Log("removed question: " + question.Question);
                Category.Value.Remove(question);
            }
        }

        foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
        {
            if(Category.Value.Count == 0)
            {
                LibraryDictionary.Remove(Category.Key);
            }
        }
    }
    

    void DisplayQuestion()
    {
        //GUILayout.FlexibleSpace();
        GUIStyle customFoldout = new GUIStyle(EditorStyles.foldout);
        customFoldout.fixedWidth = 10f;

        if(LibraryDictionary.Count == 0)
        {
            EditorGUILayout.LabelField("Questions have been loaded", EditorStyles.boldLabel);
            return;
        }

        CategoryFolder = EditorGUILayout.BeginFoldoutHeaderGroup(CategoryFolder, "Categories"); 
        if(CategoryFolder)
        {
        
            EditorGUI.indentLevel++;
            foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                
                EditorGUILayout.LabelField(Category.Key, EditorStyles.boldLabel);
                
                foreach(QuestionClass question in Category.Value)
                {
                    
                    
                    if(string.IsNullOrEmpty(question.Question))
                    {
                        
                        continue;
                    }
                    GUILayout.Space(5);

                    EditorGUILayout.BeginHorizontal();
                    
                    question.displayFolderBool = EditorGUILayout.BeginFoldoutHeaderGroup(question.displayFolderBool, "Question: ");
                    question.Question = EditorGUILayout.TextField(question.Question);
                    EditorGUILayout.EndHorizontal();

                    if(question.displayFolderBool)
                    {
                        EditorGUILayout.IntField(question.Timer);

                        for(int i = 0; i < question.Answers.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            question.Answers[i] = EditorGUILayout.TextField("Answer " + (i + 1) + ": ",  question.Answers[i]);
                            
                            //Debug.Log(question.AnswerKey[i]);
                            question.AnswerKey[i] = EditorGUILayout.Toggle("Correct: ", question.AnswerKey[i]);
                            EditorGUILayout.EndHorizontal();

                        }
                        
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
                GUILayout.Space(10);
            }
        }
        
    }


    private void ReadLibraryData()
    {
        LibraryDictionary.Clear();
        string filePath = Path.Combine(Application.streamingAssetsPath, LibraryDataPath);
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"[CSV Tool] CSV file not found at {filePath}. Run 'Write CSV' first.");
            return;
        }

        try
        {
            // Read all lines from the CSV file
            string[] lines = File.ReadAllLines(filePath);

            Debug.Log("[CSV Tool] --- Starting CSV Read ---");

            QuestionClass CurrentQuestion = new QuestionClass();
            // Loop through each line (skip index 0 if you want to ignore headers)
            for (int i = 1; i < lines.Length; i++)
            {
                
                string line = lines[i];

                // Skip completely empty lines
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Split cells by comma separation
                string[] cells = line.Split(',');

                // Process the cells (e.g., log them or update your editor variables)
                if (i == 0)
                {
                    Debug.Log($"Headers: {string.Join(" | ", cells)}");

                }

                else if (i % 2 == 1)
                {
                    //CurrentQuestion = new QuestionClass();
                    int Count = 1;
                    string linetext = null;
                    foreach(string item in cells)
                    {
                        switch(Count)
                        {
                            case 1:
                            {
                                linetext = linetext + " question: " + item;
                                CurrentQuestion.Question = item;
                                Count ++;
                                break;
                            }
                            case 2:
                            {
                                linetext = linetext + ", Time: " + item;
                                CurrentQuestion.Timer = int.Parse(item);
                                Count ++;
                                break;
                            }
                            case 3:
                            {
                                linetext = linetext + ", Category: " + item;
                                CurrentQuestion.Category = item;
                                Count ++;
                                break;
                            }
                            default:
                            {
                                linetext = linetext + ", Answer " + (Count - 3) + ": " + item;
                                CurrentQuestion.Answers.Add(item); 
                                Count ++;
                                break;
                            }
                        }
                        
                    }
                    Debug.Log(linetext);
                    continue;
                    
                }
                else
                {
                    if(CurrentQuestion == null)
                    {
                        return;
                    }

                    string linetext = null;
                    foreach(string item in cells)
                    {
                        CurrentQuestion.AnswerKey.Add(bool.Parse(item));
                        linetext = linetext + ", " + item;
                    }
                    Debug.Log(linetext);
                }

                if(!LibraryDictionary.ContainsKey(CurrentQuestion.Category))
                {
                    LibraryDictionary.Add(CurrentQuestion.Category, new List<QuestionClass>());
                }
                
                LibraryDictionary[CurrentQuestion.Category].Add(CurrentQuestion);
                
                
                CurrentQuestion = new QuestionClass();
                //CurrentQuestion = null;
            }
            
            Debug.Log("[CSV Tool] --- Finished CSV Read ---");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CSV Tool] Failed to read CSV: {ex.Message}");
        }
        CullQuestions();

    }

    private void WriteLibraryData()
    {
        try
        {
            StringBuilder sb = new StringBuilder();

            // Append Header Row
            
            sb.AppendLine("Question, QuestionTime, Category, Answers");

            foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
            {
                foreach(QuestionClass question in Category.Value)
                {
                    string top = question.Question + ", " + question.Timer.ToString() + ", " + question.Category;
                    string bottom = null;
                    for(int i = 0; i < question.Answers.Count; i++)
                    {
                        top = top + ", " + question.Answers[i];
                        
                        bottom = bottom + question.AnswerKey[i].ToString();
                        if(i < question.Answers.Count - 1)
                        {
                            bottom = bottom +  ", "; 
                        }
                    }

                    sb.AppendLine(top);
                    sb.AppendLine(bottom);
                }
            }

            // Write the text to the file (overwrites existing data)
            string filePath = Path.Combine(Application.streamingAssetsPath, LibraryDataPath);
            File.WriteAllText(filePath, sb.ToString());

            // Force Unity to refresh the project window to show the new asset
            AssetDatabase.Refresh();

            Debug.Log($"[CSV Tool] Successfully wrote CSV to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write CSV {ex.Message}");
        }
    }

    


      
        


}

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

    // organises all questions based on their category, then each category goes to their respective question list
    public static Dictionary<string, List<QuestionClass>> LibraryDictionary = new Dictionary<string, List<QuestionClass>>();
    public static QuestionClass NewQuestion;
    public static bool NewQuestionFolder;

    private static bool CategoryFolder;
    private static Vector2 scrollPos;
    private static bool OnlySelected = false;
    private static bool LibraryOverride;

    private static QuizManager quizManagerRef;

    [MenuItem("Quiz Tools/Question Library")] // location of the editor window
    public static void ShowWindow() // opens editor window
    {
        QuizLibraryWindow window = GetWindow<QuizLibraryWindow>("Question Library");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DataButtons();
        ManagerButtons();
        DisplayQuestion();
        CreateQuestionButtons();

        EditorGUILayout.EndScrollView();
    }

    void DataButtons()
    {
        LibraryDataPath = EditorGUILayout.TextField("Library file Name: ", LibraryDataPath);

        EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Quiz to Library File"))   // creates a button for the user
            {
                WriteLibraryData();
            }

            if(GUILayout.Button("Save Selected to Library File"))
            {
                OnlySelected = true;
                WriteLibraryData();
                OnlySelected = false;
            }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Quiz Library File"))   // creates a button for the user
            {
                ReadLibraryData();
            }

            GUILayout.Space(20f);

            LibraryOverride = EditorGUILayout.Toggle("Override Current Data", LibraryOverride);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete All Questions in Tool"))   // creates a button for the user
            {
                LibraryDictionary.Clear();
            }

            if (GUILayout.Button("Delete Selected Questions in Tool"))   // creates a button for the user
            {
                CullQuestions(true);
            }

        EditorGUILayout.EndHorizontal();
    }

    void ManagerButtons()
    {
        if(LibraryDictionary.Count == 0)
        {
            return;
        }
        GUILayout.Space(20f);
        EditorGUIUtility.labelWidth = 100f;

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


        quizManagerRef = (QuizManager)EditorGUILayout.ObjectField("Quiz Manager:", quizManagerRef, typeof(QuizManager), true);

        if (GUILayout.Button("Assign Selected Questions to Manager"))   // creates a button for the user
        {
            AddToManager();
        }
        if (GUILayout.Button("Clear Questions in manager"))   // creates a button for the user
        {
            
        }

    }

    void CullQuestions(bool CullSelected)
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

                if(CullSelected == true && question.SelectedQuestion == true)
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

        List<string> cullCat = new List<string>();
        foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
        {
            if(Category.Value.Count == 0)
            {
                cullCat.Add(Category.Key);
            }
        }

        foreach(string category in cullCat)
        {
            LibraryDictionary.Remove(category);
        }
    }

    void DisplayQuestion()
    {
        //GUILayout.FlexibleSpace();
        GUIStyle customFoldout = new GUIStyle(EditorStyles.foldout);
        customFoldout.fixedWidth = 10f;
        GUILayout.Space(30f);

        if(LibraryDictionary.Count == 0)
        {
            EditorGUILayout.LabelField("No Questions Have been Loaded", EditorStyles.boldLabel);
            return;
        }
        
        EditorGUILayout.BeginHorizontal();

            bool Invertbool = false;
            bool selectAll = false;
            if (GUILayout.Button("Invert Question Selections"))   // creates a button for the user
            {
                Invertbool = true;   
            }
            if (GUILayout.Button("Select All Questions"))   // creates a button for the user
            {
                selectAll = true;  
            }

        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 80f;
        CategoryFolder = EditorGUILayout.BeginFoldoutHeaderGroup(CategoryFolder, "Categories"); 
        if(CategoryFolder)
        {
        
            EditorGUI.indentLevel++;
            foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
            {
                bool CatBool = false;
                EditorGUILayout.BeginHorizontal();
                
                    EditorGUILayout.LabelField(Category.Key, EditorStyles.boldLabel);
                    if (GUILayout.Button("Select Whole Category"))   // creates a button for the user
                    {
                        CatBool = true;    
                    }
                
                EditorGUILayout.EndHorizontal();
                
                foreach(QuestionClass question in Category.Value)
                {
                    if(string.IsNullOrEmpty(question.Question))
                    {
                        continue;
                    }
                    GUILayout.Space(10);

                    if(CatBool == true || selectAll == true)
                    {
                        question.SelectedQuestion = true;
                    }
                    if(Invertbool == true)
                    {
                        question.SelectedQuestion = !question.SelectedQuestion;
                    }
                    

                    EditorGUILayout.BeginHorizontal();
                    
                    question.displayFolderBool = EditorGUILayout.Foldout(question.displayFolderBool, "Question: ");
                   
                    question.Question = EditorGUILayout.TextField(question.Question);
                    question.SelectedQuestion = EditorGUILayout.Toggle("Selected: ", question.SelectedQuestion);
                    EditorGUILayout.EndHorizontal();

                    if(question.displayFolderBool)
                    {
                        EditorGUIUtility.labelWidth = 120f;
                        EditorGUILayout.IntField("Question Timer: ", question.Timer);
                        EditorGUIUtility.labelWidth = 80f;

                        for(int i = 0; i < question.Answers.Count; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            question.Answers[i] = EditorGUILayout.TextField("Answer " + (i + 1) + ": ",  question.Answers[i]);
                            
                            //Debug.Log(question.AnswerKey[i]);
                            question.AnswerKey[i] = EditorGUILayout.Toggle("Correct: ", question.AnswerKey[i]);

                            if (GUILayout.Button("Remove Answer"))   
                            {
                                NewQuestion.Answers.RemoveAt(i);
                                NewQuestion.AnswerKey.RemoveAt(i);
                            }

                            EditorGUILayout.EndHorizontal();

                        }
                        EditorGUIUtility.labelWidth = 140f;
                        question.Explanation = EditorGUILayout.TextField("Answer Explanation:", question.Explanation);
                        EditorGUIUtility.labelWidth = 80f;
                    }
                }
                GUILayout.Space(10);
            }
            
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
    }

    private void CreateQuestionButtons()
    {
        GUILayout.Space(20f);
        if(NewQuestion == null)
        {
            NewQuestion = new QuestionClass();
        }

        EditorGUILayout.BeginHorizontal();

            NewQuestionFolder = EditorGUILayout.BeginFoldoutHeaderGroup(NewQuestionFolder, "New Question?");
            NewQuestion.Question = EditorGUILayout.TextField("Question:", NewQuestion.Question);  

        EditorGUILayout.EndHorizontal();

        if(NewQuestionFolder)
        {
            EditorGUIUtility.labelWidth = 120f;
            EditorGUILayout.IntField("Question Timer: ", NewQuestion.Timer);
            EditorGUIUtility.labelWidth = 80f;

            if (GUILayout.Button("Add Answer"))   // creates a button for the user
            {
                NewQuestion.Answers.Add("");
                NewQuestion.AnswerKey.Add(false);
            }

            for(int i = 0; i < NewQuestion.Answers.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                NewQuestion.Answers[i] = EditorGUILayout.TextField("Answer " + (i + 1) + ": ",  NewQuestion.Answers[i]);
                
                //Debug.Log(question.AnswerKey[i]);
                NewQuestion.AnswerKey[i] = EditorGUILayout.Toggle("Correct: ", NewQuestion.AnswerKey[i]);

                if (GUILayout.Button("Remove Answer"))   // creates a button for the user
                {
                    NewQuestion.Answers.RemoveAt(i);
                    NewQuestion.AnswerKey.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();

            }
            EditorGUIUtility.labelWidth = 140f;
            NewQuestion.Category = EditorGUILayout.TextField("Question Category:", NewQuestion.Category);
            NewQuestion.Explanation = EditorGUILayout.TextField("Answer Explanation:", NewQuestion.Explanation);
            EditorGUIUtility.labelWidth = 80f;
        }

        EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Question to Tool Library"))   // creates a button for the user
            {
                if(!string.IsNullOrEmpty(NewQuestion.Question) || NewQuestion.Timer > 0 || !string.IsNullOrEmpty(NewQuestion.Category) || NewQuestion.Answers.Count >= 1)
                {
                    if(!LibraryDictionary.ContainsKey(NewQuestion.Category))
                    {
                        LibraryDictionary.Add(NewQuestion.Category, new List<QuestionClass>());
                    }

                    LibraryDictionary[NewQuestion.Category].Add(NewQuestion);
                }
            }
            
            if (GUILayout.Button("Clear New Question Data"))   // creates a button for the user
            {
                NewQuestion = new QuestionClass();
                CullQuestions(false);
            }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void AddToManager()
    {
        if(quizManagerRef.QuestionList == null)
        {
            quizManagerRef.QuestionList = new List<QuestionClass>();
        }

        foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
        {
            List<QuestionClass> cullList = new List<QuestionClass>();

            foreach(QuestionClass question in Category.Value)
            {
                if(question.SelectedQuestion == true)
                {
                    quizManagerRef.AssignQuestion(question);
                }
            }
        }
    }

    private void ReadLibraryData()
    {
        if(LibraryOverride == true)
        {
            LibraryDictionary.Clear();
        }

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

                if (i == 0)
                {
                    Debug.Log($"[Quiz Tool] Headers: {string.Join(" | ", cells)}");

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
                            case 4:
                            {
                                linetext = linetext + ", Explanation: " + item;
                                CurrentQuestion.Explanation = item;
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
                    Debug.Log("[Quiz Tool]" + linetext);
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
                    Debug.Log("[Quiz Tool]" + linetext);
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
        CullQuestions(false);

    }

    private void WriteLibraryData()
    {
        try
        {
            StringBuilder sb = new StringBuilder();

            // Append Header Row
            
            if(LibraryDictionary == null || LibraryDictionary.Count == 0)
            {
                Debug.LogError("[Quiz Tool] No questions in library to save");
                return;
            }

            sb.AppendLine("Question, QuestionTime, Category, Explanation, Answers");

            int Q = 0;
            foreach(KeyValuePair<string, List<QuestionClass>> Category in LibraryDictionary)
            {
                foreach(QuestionClass question in Category.Value)
                {
                    if(OnlySelected == true)
                    {
                        if(!question.SelectedQuestion)
                        {
                            continue;
                        }
                        Q ++;
                    }

                    string top = question.Question + "," + question.Timer.ToString() + "," + question.Category + "," + question.Explanation;
                    string bottom = null;
                    for(int i = 0; i < question.Answers.Count; i++)
                    {
                        top = top + "," + question.Answers[i];
                        
                        bottom = bottom + question.AnswerKey[i].ToString();
                        if(i < question.Answers.Count - 1)
                        {
                            bottom = bottom +  ","; 
                        }
                    }

                    sb.AppendLine(top);
                    sb.AppendLine(bottom);
                }
            }

            if(OnlySelected == true && Q == 0)
            {
                Debug.LogError("[Quiz Tool] No Questions have been selected to save");
                return;
            }

            // Write the text to the file (overwrites existing data)
            string filePath = Path.Combine(Application.streamingAssetsPath, LibraryDataPath);
            File.WriteAllText(filePath, sb.ToString());

            // Force Unity to refresh the project window to show the new asset
            AssetDatabase.Refresh();

            Debug.Log($"[Quiz Tool] Successfully wrote CSV to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Quiz Tool] Failed to write CSV {ex.Message}");
        }
    }
}

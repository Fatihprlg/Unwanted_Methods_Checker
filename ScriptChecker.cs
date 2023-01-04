using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;


public class ScriptChecker : EditorWindow
{
    private List<string> unwantedWords;
    private List<string> checkList;
    private GUIContent unwantedWordsContent, checkContent;
    private int newCount;
    [MenuItem("Script Checker/Unwanted Words Checker")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ScriptChecker));
    }

    private void OnEnable()
    {
        unwantedWords = new List<string>();
        unwantedWordsContent = new GUIContent("Unwanted Word List");
        checkContent = new GUIContent("Check Scripts");
        checkList = new List<string>();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(unwantedWordsContent);
        var list = unwantedWords;
        newCount = Mathf.Max(0, EditorGUILayout.IntField("Count", list.Count));
        while (newCount < list.Count)
            list.RemoveAt( list.Count - 1 );
        while (newCount > list.Count)
            list.Add(null);
 
        for(int i = 0; i < list.Count; i++)
        {
            list[i] = EditorGUILayout.TextField(list[i]);
        }

        if (GUILayout.Button(checkContent))
        {
            CheckScripts();
        }
    }

    private void CheckScripts()
    {
        checkList.Clear();
        foreach (string unwantedWord in unwantedWords.Where(unwantedWord => !string.IsNullOrEmpty(unwantedWord) || !string.IsNullOrWhiteSpace(unwantedWord)))
        {
            checkList.Add(unwantedWord);
        }
        if (checkList == null || checkList.Count < 1)
        {
            Debug.LogError("Unwanted words list is empty or null!");
            return;
        }
        Assembly asm = Assembly.GetExecutingAssembly();
        var classes = asm.GetTypes().Where(@class => @class.IsSubclassOf(typeof(MonoBehaviour)) && !@class.IsAbstract);
        CheckClasses(classes);
    }

    private void CheckClasses(IEnumerable<Type> classes)
    {
        string nameOfInstruction = "";
        foreach (Type @class in classes)
        {
            var methodBase = @class.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                               BindingFlags.DeclaredOnly);
            foreach (MethodInfo methodInfo in methodBase)
            {
                var instructions = MethodBodyReader.GetInstructions(methodInfo);
                if (instructions.Exists(instruction =>
                    {
                        if (checkList.Contains(instruction.GetName()))
                        {
                            nameOfInstruction = instruction.GetName();
                            return true;
                        }
                        else return false;
                    }))
                {
                    Debug.LogError($"{methodInfo.Name} using {nameOfInstruction}! Remove from {@class.FullName}.");
                }
            }
        }
    }

}

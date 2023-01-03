using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using Object = UnityEngine.Object;

public class ScriptChecker : MonoBehaviour
{
    [SerializeField] private List<string> unwantedWords;

    [EditorButton]
    public void CheckScripts()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        var classes = asm.GetTypes().Where(@class => @class.IsSubclassOf(typeof(MonoBase)) && !@class.IsAbstract);
        CheckClasses(classes);
    }

    private void CheckClasses(IEnumerable<Type> classes)
    {
        foreach (Type @class in classes)
        {
            var methodBase = @class.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                               BindingFlags.DeclaredOnly);
            foreach (MethodInfo methodInfo in methodBase)
            {
                var instructions = MethodBodyReader.GetInstructions(methodInfo);
                if (instructions.Exists(instruction => unwantedWords.Contains(instruction.GetName())))
                {
                    Debug.LogError($"{methodInfo.Name} using one of unwanted words! Remove from {@class.FullName}.");
                }
            }
        }
    }

}
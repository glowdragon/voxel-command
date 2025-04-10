using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ImperativeExample3 : MonoBehaviour
{
    private int a;

    private int b;

    public int A
    {
        get => a;
        set
        {
            if (a != value)
            {
                a = value;
                UpdateC();
            }
        }
    }

    public int B
    {
        get => b;
        set
        {
            if (b != value)
            {
                b = value;
                UpdateC();
            }
        }
    }

    public int C { get; private set; }

    void Start()
    {
        UpdateC();
    }

    private void UpdateC()
    {
        C = a + b;
    }
}

/*

Properties with Setters

+ More efficient than polling (UpdateC only runs on change)

- Requires manual implementation of property setters and change notification logic
- Requires workaround for Inspector exposure (Custom Editor)

*/

#if UNITY_EDITOR
[CustomEditor(typeof(ImperativeExample3))]
public class ImperativeExample3Editor : Editor
{
    public override void OnInspectorGUI()
    {
        var targetScript = (ImperativeExample3)target;
        EditorGUI.BeginChangeCheck();
        int newA = EditorGUILayout.IntField("A", targetScript.A);
        int newB = EditorGUILayout.IntField("B", targetScript.B);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetScript, "Change A or B");
            targetScript.A = newA;
            targetScript.B = newB;
            EditorUtility.SetDirty(targetScript);
        }
        EditorGUI.BeginDisabledGroup(true); 
        EditorGUILayout.IntField("C", targetScript.C);
        EditorGUI.EndDisabledGroup();
    }
}
#endif

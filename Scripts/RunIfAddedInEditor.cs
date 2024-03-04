using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class RunIfAddedInEditor : MonoBehaviour
{
    #if UNITY_EDITOR
    void Awake()
    {
        if (!EditorApplication.isPlaying)
        {
            GetComponent<QuestLogger>().CallWhenAddedToScenceInEditor();
        }
    }
    #endif

}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(TextController))]
public class TextControllerEditor : Editor
{
    internal LanguageItemsList languageItemsList;

    internal string[] _choices;

    int choiceIndex;

    internal void OnEnable()
    {
        languageItemsList = GameObject.Find("LanguageController").GetComponent<LanguageController>().itemsList;

        _choices = new string[languageItemsList.Keys.Count];

        for (int i = 0; i < languageItemsList.Keys.Count; i++)
        {
            _choices[i] = languageItemsList.Keys[i];
        }
    }

    public override void OnInspectorGUI()
    {
        var textController = target as TextController;

        choiceIndex = languageItemsList.Keys.IndexOf(textController.key);

        EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

        choiceIndex = EditorGUILayout.Popup("Language Item", choiceIndex, _choices);

        EditorGUI.EndDisabledGroup();

        if (!EditorApplication.isPlaying && choiceIndex != -1)
        {
            textController.key = _choices[choiceIndex];

            textController.setText(_choices[choiceIndex]);
        }

        if (GUI.changed & !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(textController);
            EditorSceneManager.MarkSceneDirty(textController.gameObject.scene);
        }
    }
}

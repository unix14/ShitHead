using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LanguageController))]
public class LanguageControllerEditor : Editor
{
    string languageItemKey = "";

    bool warningLanguageItem = false;
    string warningMessageLanguageItem = "";

    public override void OnInspectorGUI()
    {
        bool warningLanguage = false;
        string warningMessageLanguage = "";

        var languageController = target as LanguageController;

        if (languageController.languages == null)
        {
            languageController.languages = new List<string>();
        }

        GUIStyle backgroundTexture = new GUIStyle();
        backgroundTexture.normal.background = createTexture(600, 1, new Color(1.0f, 1.0f, 1.0f, 0.2f));

        GUILayout.Space(10);

        GUILayout.BeginVertical(backgroundTexture);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.EndVertical();

        GUILayout.Space(10);

        GUILayout.BeginVertical(backgroundTexture);

        EditorGUILayout.Space();

        GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

        for (int i = 0; i < languageController.languages.Count; i++)
        {
            GUI.SetNextControlName(i.ToString());

            languageController.languages[i] = EditorGUILayout.TextField(languageController.languages[i], GUILayout.Height(18));
            GUILayout.Space(5);
        }

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUIStyle customButton = new GUIStyle(GUI.skin.button);

        customButton.fixedWidth = 65;
        customButton.fixedHeight = 22;

        string focusedControl = GUI.GetNameOfFocusedControl();

        if (GUILayout.Button("Add", customButton))
        {
            languageController.languages.Add("New Language");
        }

        EditorGUI.BeginDisabledGroup(focusedControl == "" || int.Parse(focusedControl) >= languageController.languages.Count);

        if (GUILayout.Button("Remove", customButton))
        {
            languageController.languages.RemoveAt(int.Parse(focusedControl));
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (languageController.languages.Count != languageController.languages.Distinct().Count())
        {
            warningMessageLanguage = "Warning : Language Name Is Repeated";
            warningLanguage = true;
        }

        if (languageController.languages.IndexOf("") != -1)
        {
            warningMessageLanguage = "Warning : Language Name Is Empty";
            warningLanguage = true;
        }

        if (warningLanguage)
        {
            GUILayout.BeginHorizontal();

            GUIStyle customLabel = new GUIStyle(GUI.skin.label);

            customLabel.normal.textColor = Color.blue;
            customLabel.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField(warningMessageLanguage, customLabel);

            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        GUILayout.EndVertical();

        GUILayout.Space(10);

        GUILayout.BeginVertical(backgroundTexture);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(warningLanguage);

        EditorGUILayout.LabelField("Language Items", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        GUILayout.EndVertical();

        GUILayout.Space(10);

        if (languageController.itemsList == null)
        {
            languageController.itemsList = new LanguageItemsList();
        }

        if (!warningLanguage)
        {
            foreach (string key in languageController.itemsList.Keys)
            {
                GUILayout.BeginVertical(backgroundTexture);

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(key, EditorStyles.boldLabel, GUILayout.MaxWidth(165));

                GUILayout.FlexibleSpace();
               
                if (GUILayout.Button("Remove", customButton))
                {
                    languageController.itemsList.Remove(key);
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space();

                if (!languageController.itemsList.ContainsKey(key))
                {
                    break;
                }

                foreach (string languageName in languageController.languages)
                {
                    GUILayout.BeginHorizontal();

                    if (languageController.itemsList.Get(key).ContainsKey(languageName))
                    {
                        languageController.itemsList.Get(key).Set(languageName, EditorGUILayout.TextField(languageName, languageController.itemsList.Get(key).Get(languageName)));
                    }
                    else
                    {
                        languageController.itemsList.Get(key).Add(languageName,"");
                    }

                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                GUILayout.EndVertical();

                GUILayout.Space(10);
            }
        }

        GUILayout.Space(20);
       
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUIStyle customTextField = new GUIStyle(GUI.skin.textField);

        customTextField.alignment = TextAnchor.MiddleCenter;

        customTextField.fixedHeight = 24;

        languageItemKey = EditorGUILayout.TextField("", languageItemKey, customTextField, GUILayout.MaxWidth(170));

        if (GUILayout.Button("Add", customButton))
        {
            warningMessageLanguageItem = "";
            warningLanguageItem = false;

            if (languageController.itemsList.ContainsKey(languageItemKey))
            {
                warningMessageLanguageItem = "Warning : Item Already Exist";
                warningLanguageItem = true;
            }

            if (languageItemKey == "")
            {
                warningMessageLanguageItem = "Warning : Item Name Is Empty";
                warningLanguageItem = true;
            }

            if (!warningLanguageItem)
            {
                LanguageItem languageItem = new LanguageItem();

                foreach (string languageName in languageController.languages)
                {
                    languageItem.Add(languageName, "");
                    Debug.Log(languageName + " " + languageItemKey + " " + languageItem + " " + languageItemKey + " ");
                }

                languageController.itemsList.Add(languageItemKey, languageItem);
            }

            languageItemKey = "";
        }

        if (languageItemKey != "")
        {
            warningLanguageItem = false;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (warningLanguageItem)
        {
            GUILayout.BeginHorizontal();

            GUIStyle customLabel = new GUIStyle(GUI.skin.label);

            customLabel.normal.textColor = Color.blue;
            customLabel.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField(warningMessageLanguageItem, customLabel);

            GUILayout.EndHorizontal();
        }

        EditorGUI.EndDisabledGroup();

        GUILayout.Space(20);

        if (GUI.changed & !EditorApplication.isPlaying)
        {
            EditorUtility.SetDirty(languageController);
            EditorSceneManager.MarkSceneDirty(languageController.gameObject.scene);
        }
    }

    Texture2D createTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}





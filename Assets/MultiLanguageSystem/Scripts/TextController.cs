using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextController : MonoBehaviour
{
    public string key;

    LanguageItemsList languageItemsList;

    void OnEnable()
    {
        StartCoroutine(updateLanguage());
    }

    IEnumerator updateLanguage()
    {
        string language;

        while (true)
        {
            setText(key);

            language = PlayerPrefs.GetString("language");

            yield return new WaitUntil(() => PlayerPrefs.GetString("language") != language);
        }
    }

    public void setText(string _Key)
    {
        key = _Key;

        languageItemsList = GameObject.Find("LanguageController").GetComponent<LanguageController>().itemsList;

        if (languageItemsList.ContainsKey(key) && languageItemsList.Get(key).ContainsKey(PlayerPrefs.GetString("language")))
        {
            string text = languageItemsList.Get(key).Get(PlayerPrefs.GetString("language"));

            var textUIComponents = GetComponents(typeof(Text));
            
            foreach (Text textUI in textUIComponents)
            {
                textUI.text = text;
            }

            var textMeshComponents = GetComponents(typeof(TextMesh));

            foreach (TextMesh textMesh in textMeshComponents)
            {
                textMesh.text = text;
            }

            var textMeshProComponents = GetComponents(typeof(TextMeshPro));

            foreach (TextMeshPro textMeshPro in textMeshProComponents)
            {
                textMeshPro.text = text;
            }

            var textMeshProUGUIComponents = GetComponents(typeof(TextMeshProUGUI));

            foreach (TextMeshProUGUI textMeshProUGUI in textMeshProUGUIComponents)
            {
                textMeshProUGUI.text = text;
            }
        }
    }
}

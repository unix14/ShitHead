using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public GameObject explainCanvas;

    void Awake()
    {
        GameObject.Find("LanguageSwitcher/Switch/Value").GetComponent<Text>().text = PlayerPrefs.GetString("language");
    }

    public void changLanguage(string option)
    {
        List<string> languages = GameObject.Find("LanguageController").GetComponent<LanguageController>().languages;

        int index = languages.IndexOf(PlayerPrefs.GetString("language"));

        switch (option)
        {
            case "nextLanguage":

                index = index + 1 < languages.Count ? index + 1 : 0;

                GameObject.Find("LanguageController").SendMessage("setLanguage", languages[index]);

                //GameObject.Find("LanguageSwitcher/Switch/Value").GetComponent<Text>().text = languages[index];


                break;

            case "prevLanguage":

                index = index > 0 ? index - 1 : languages.Count - 1;

                GameObject.Find("LanguageController").SendMessage("setLanguage", languages[index]);

                GameObject.Find("LanguageSwitcher/Switch/Value").GetComponent<Text>().text = languages[index];

                break;
        }
    }
}

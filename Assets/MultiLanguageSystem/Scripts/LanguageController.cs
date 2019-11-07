using System.Collections.Generic;
using UnityEngine;

public class LanguageController : MonoBehaviour
{
    [SerializeField]
    public List<string> languages;

    [SerializeField]
    public LanguageItemsList itemsList;

    void Awake()
    {
        if (!PlayerPrefs.HasKey("language"))
        {
            PlayerPrefs.SetString("language", "");
        }

        if (PlayerPrefs.GetString("language") == "" && languages.Count > 0)
        {
            PlayerPrefs.SetString("language", languages[0]);
        }
    }

    public void setLanguage(string language)
    {
        PlayerPrefs.SetString("language", language);
    }
}

[System.Serializable]
public class LanguageItemsList
{
    public List<string> Keys;

    public List<LanguageItem> Values;

    public LanguageItemsList()
    {
        Keys = new List<string>();
        Values = new List<LanguageItem>();
    }

    public void Add(string key, LanguageItem value)
    {
        Keys.Add(key);
        Values.Add(value);
    }

    public void Remove(string key)
    {
        int index = Keys.IndexOf(key);

        Keys.RemoveAt(index);
        Values.RemoveAt(index);
    }

    public bool ContainsKey(string key)
    {
        int index = Keys.IndexOf(key);

        return index == -1 ? false : true;
    }

    public LanguageItem Get(string key)
    {
        int index = Keys.IndexOf(key);

        return index == -1 ? new LanguageItem() : Values[index];
    }

    public void Set(string key, LanguageItem value)
    {
        int index = Keys.IndexOf(key);

        Values[index] = value;
    }
}

[System.Serializable]
public class LanguageItem
{
    public List<string> Keys;

    public List<string> Values;

    public LanguageItem()
    {
        Keys = new List<string>();
        Values = new List<string>();
    }

    public void Add(string key, string value)
    {
        Keys.Add(key);
        Values.Add(value);
    }

    public void Remove(string key)
    {
        int index = Keys.IndexOf(key);

        Keys.RemoveAt(index);
        Values.RemoveAt(index);
    }

    public bool ContainsKey(string key)
    {
        int index = Keys.IndexOf(key);

        return index == -1 ? false : true;
    }

    public string Get(string key)
    {
        int index = Keys.IndexOf(key);

        return index == -1 ? null : Values[index];
    }

    public void Set(string key, string value)
    {
        int index = Keys.IndexOf(key);

        if(index >=0 && index < Keys.Count)
        {
            Values[index] = value;
        }
    }
}

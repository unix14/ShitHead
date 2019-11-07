using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogoScript : MonoBehaviour
{

    public Sprite hebLogo;
    public Sprite engLogo;

    private Image logoImage;

    public bool isInHebrew = true;

    // Start is called before the first frame update
    void Start()
    {
        isInHebrew = PlayerPrefs.GetString("user_lang", "Hebrew") == "Hebrew";

        logoImage = GetComponent<Image>();
    }

    private void Update()
    {
        setLogoLanguage();
    }

    public void setLogoLanguage()
    {
        if (isInHebrew)
        {
            logoImage.sprite = hebLogo;
        }
        else
        {
            logoImage.sprite = engLogo;
        }
    }

    public void switchLogo()
    {
        isInHebrew = !isInHebrew;
        setLogoLanguage();

        if (isInHebrew)
        {
            PlayerPrefs.SetString("user_lang", "Hebrew");
        }
        else
        {
            PlayerPrefs.SetString("user_lang", "English");
        }
    }
}

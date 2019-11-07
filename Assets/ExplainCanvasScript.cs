using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplainCanvasScript : MonoBehaviour
{

    public bool isHebrew = true;
    public GameObject engCanvas;
    public GameObject hebCanvas;

    // Start is called before the first frame update
    void Start()
    {
        isHebrew = PlayerPrefs.GetString("user_lang", "Hebrew") == "Hebrew";
    }

    // Update is called once per frame
    void Update()
    {
        if (isHebrew)
        {
            hebCanvas.gameObject.active = true;
            engCanvas.gameObject.active = false;
        }
        else
        {
            hebCanvas.gameObject.active = false;
            engCanvas.gameObject.active = true;
        }
    }

    public void switchLang()
    {
        isHebrew = !isHebrew;
    }
}

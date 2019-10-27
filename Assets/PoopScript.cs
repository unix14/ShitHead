using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    
    }
    public Texture[] frames;
    public int framesPerSecond = 30;

    void Update() {
        int index = (int) Mathf.Floor(Time.time * framesPerSecond % frames.Length);
        GetComponent<Renderer>().material.mainTexture = frames[index];
    }


  
}

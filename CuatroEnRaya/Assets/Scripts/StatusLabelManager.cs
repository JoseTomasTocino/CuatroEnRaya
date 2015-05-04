using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StatusLabelManager : MonoBehaviour {

    Text text;
    public static string statusText;

	// Use this for initialization
	void Awake () 
    {
        text = GetComponent<Text>();
        statusText = "";
	}
	
	// Update is called once per frame
	void Update () 
    {
        text.text = statusText;
	}
}

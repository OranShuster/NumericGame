using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintCurrentTime : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.gameObject.GetComponent<Text>().text = UserInformation.SystemTime.Now().ToString(Constants.DateFormatDebug);
	}
}

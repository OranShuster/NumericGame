﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VersionText : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		gameObject.GetComponent<Text>().text = Application.version;
	}
	
}

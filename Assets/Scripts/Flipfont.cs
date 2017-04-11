using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq; 
 
public class Flipfont : MonoBehaviour
{

    public Text MyText; //You can also make this public and attach your UI text here.

    string _individualLine = ""; //Control individual line in the multi-line text component.

    int _numberOfAlphabetsInSingleLine = 20;

    void Awake()
    {
        MyText = GetComponent<Text>();
    }

    void Start()
    {

        var reversedString="";
        var listofWords = MyText.text.Split(' ').ToList(); //Extract words from the sentence

        foreach (var s in listofWords)
        {

            if (_individualLine.Length >= _numberOfAlphabetsInSingleLine)
            {
                reversedString += Reverse(_individualLine) + "\n"; //Add a new line feed at the end, since we cannot accomodate more characters here.
                _individualLine = ""; //Reset this string for new line.
            }

            _individualLine += s + " ";
        }
        if (_individualLine != "")
            reversedString += Reverse(_individualLine) + "\n";
        MyText.text = reversedString;
    }

    public static string Reverse(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

}
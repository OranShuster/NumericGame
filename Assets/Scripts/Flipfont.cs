using System;
using System.Linq;

public class Flipfont
{
    public static string ReverseText(string str, int lineLength=15)
    {
        string individualLine = ""; //Control individual line in the multi-line text component.
        var reversedString = "";
        var listofWords = str.Split(' ').ToList(); //Extract words from the sentence

        foreach (var s in listofWords)
        {
            if (individualLine.Length >= lineLength)
            {
                reversedString += ReverseLine(individualLine) + "\n"; //Add a new line feed at the end, since we cannot accomodate more characters here.
                individualLine = ""; //Reset this string for new line.
            }
            individualLine += s + " ";
        }
        individualLine = individualLine.Substring(0, individualLine.Length - 1);
        if (individualLine != "")
            reversedString += ReverseLine(individualLine);
        return reversedString;
    }

    private static string ReverseLine(string s)
    {
        var charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

}
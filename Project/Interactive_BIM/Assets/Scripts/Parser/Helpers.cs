using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Helpers
{
    /// <summary>
    /// Splits a comma-separated line into an array of strings.
    /// Credit to Tomer-Barkan on the Unity Forums and ridgerunner on StackOverflow 
    /// <para/>
    /// (http://answers.unity3d.com/questions/144200/are-there-any-csv-reader-for-unity3d-without-needi.html)
    /// </summary>
    /// <param name="line">CSV Line to split</param>
    /// <returns></returns>
    public static string[] SplitCsvLine(string line)
    {
        string pattern = @"
     # Match one value in valid CSV string.
     (?!\s*$)                                      # Don't match empty last value.
     \s*                                           # Strip whitespace before value.
     (?:                                           # Group for value alternatives.
       '(?<val>[^'\\]*(?:\\[\S\s][^'\\]*)*)'       # Either $1: Single quoted string,
     | ""(?<val>[^""\\]*(?:\\[\S\s][^""\\]*)*)""   # or $2: Double quoted string,
     | (?<val>[^,'""\s\\]*(?:\s+[^,'""\s\\]+)*)    # or $3: Non-comma, non-quote stuff.
     )                                             # End group of value alternatives.
     \s*                                           # Strip whitespace after value.
     (?:,|$)                                       # Field ends on comma or EOS.
     ";
        string[] values = (from Match m in Regex.Matches(line, pattern,
                RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline)
            select m.Groups[1].Value).ToArray();
        return values;
    }
}

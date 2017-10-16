using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CsvParser
{
    public List<string> Keys = new List<string>();
    public List<string[]> Values = new List<string[]>();

    public void Parse(string file)
    {
        StreamReader sr = new StreamReader(file);
        while (!sr.EndOfStream)
        {
            var values = SplitCsvLine(sr.ReadLine());
            if (Keys.Count == 0)
            {
                Keys = values.ToList();
                continue;
            }
            Values.Add(values);
        }
    }

    private string[] SplitCsvLine(string line)
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

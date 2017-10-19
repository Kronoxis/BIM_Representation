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
            var values = Helpers.SplitCsvLine(sr.ReadLine());
            if (Keys.Count == 0)
            {
                Keys = values.ToList();
                continue;
            }
            Values.Add(values);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIM_Generator : MonoBehaviour
{
    public string FilePath;


	private void Start ()
	{
	    if (string.IsNullOrEmpty(FilePath))
	    {
	        Debug.LogError("Please specify a FilePath");
            return;
	    }

	    var parser = new BIM_Parser(FilePath);
	    if (!parser.IsValid()) return;
	    while (!parser.IsEnd())
	    {
	        parser.GetNextIfcTopologicalRepresentationItem();
	    }
	}
}

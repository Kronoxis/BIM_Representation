using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTags : MeshEditor
{
    public uint Tag = 0;
    public string IfcType = "";

    // Use this for initialization
    private void Awake()
	{
        // FORMAT:
        // 'Name',Tag,IfcType x
        // where x (optional) is added by FME because two objects cannot have the same name

        // Remove x
	    var lastSpace = name.LastIndexOf(' ');
        if (lastSpace > name.LastIndexOf("Ifc"))
        {
            name = name.Substring(0, lastSpace);
        }
        
        // Split into values
	    var values = Helpers.SplitCsvLine(name);
	    if (values.Length != 3)
	    {
	        Destroy(this);
            return;
	    }

        // Get values
	    Tag = uint.Parse(values[1]);
	    IfcType = values[2];
        
        // Store mesh
	    MeshLibrary.AddGameObject(Tag, IfcType, gameObject);
	}

    private void GetTag()
    {
        var colonIdx = name.LastIndexOf(':');
        if (colonIdx != -1)
        {
            var begin = colonIdx + 1;
            var spaceIdx = name.LastIndexOf(' ');
            if (spaceIdx != -1 && spaceIdx > begin)
            {
                var length = spaceIdx - begin;
                Tag = uint.Parse(name.Substring(begin, length));
            }
            else
            {
                Tag = uint.Parse(name.Substring(begin));
            }
        }
    }

    private void GetIfcType()
    {
        var openBracketIdx = name.LastIndexOf('(');
        if (openBracketIdx != -1)
        {
            var begin = openBracketIdx + 1;
            var closeBracketIdx = name.LastIndexOf(')');
            if (closeBracketIdx != -1 && closeBracketIdx > begin)
            {
                var length = closeBracketIdx - begin;
                IfcType = name.Substring(begin, length);
            }
            else
            {
                IfcType = name.Substring(begin);
            }
        }
    }
}

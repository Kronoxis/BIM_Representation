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
        // Name:Tag x (Type)
        // where x (optional) is added by FME because two objects cannot have the same name
	    GetTag();
	    GetIfcType();
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

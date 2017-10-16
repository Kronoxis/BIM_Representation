using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTag : MeshEditor
{
    public uint Tag = 0;

	// Use this for initialization
	private void Awake()
	{
        // FORMAT:
        // Name:Tag x
        // where x (optional) is added by FME because two objects cannot have the same name
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

	    MeshLibrary.AddGameObject(Tag, gameObject);
	}
}

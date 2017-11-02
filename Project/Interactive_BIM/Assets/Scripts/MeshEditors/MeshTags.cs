using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshTags : MonoBehaviour
{
    public string Name = "";
    public uint Tag = 0;
    public string IfcType = "";

    public void AddTags(MeshFilter mf)
    {
        var script = mf.gameObject.GetComponent<MeshTags>();
        if (!script)
        {
            script = mf.gameObject.AddComponent<MeshTags>();
            script.SetTags();
        }

        // Store mesh
        MeshLibrary.AddGameObject(script.Tag, script.IfcType, mf.gameObject);
    }

    private void SetTags()
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
	    if (values.Length < 3)
	    {
	        DestroyImmediate(this);
            return;
	    }

        // Get values
        Name = values[0];
	    Tag = uint.Parse(values[1]);
	    IfcType = values[2]; 
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshTags : MonoBehaviour
{
    public uint Tag = 0;
    public string IfcType = "";

    public void AddTags(MeshFilter mf)
    {
        var script = mf.gameObject.GetComponent<MeshTags>();
        if (script == null)
            script = mf.gameObject.AddComponent<MeshTags>();
        script.SetTags();
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
	    if (values.Length != 3)
	    {
	        DestroyImmediate(this);
            return;
	    }

        // Get values
	    Tag = uint.Parse(values[1]);
	    IfcType = values[2];
        
        // Store mesh
	    MeshLibrary.AddGameObject(Tag, IfcType, gameObject);
	}
}

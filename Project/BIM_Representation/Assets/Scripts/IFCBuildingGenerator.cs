using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFCBuildingGenerator : MonoBehaviour
{
    public List<string> Files = new List<string>();
    public uint BatchSize = 1000;

    private List<IFCDataContainer> DataContainers = new List<IFCDataContainer>();

	// Use this for initialization
	void Start () {
	    foreach (var file in Files)
	    {
	        DataContainers.Add(new IFCDataContainer(file));
	    }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metadata : MonoBehaviour
{
    public uint Id;
    public List<string> Keys = new List<string>();
    public List<string> Properties = new List<string>();

    public void SetMetadata(IFCEntity e)
    {
        if (e == null) return;
        Id = e.Id;
        Keys = e.Keys;
        Properties = e.Properties;
    }
}

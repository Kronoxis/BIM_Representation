using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIM_Generator : MonoBehaviour
{
    public string FilePath;

    private Dictionary<uint, IfcEntity> _entities = new Dictionary<uint, IfcEntity>();

	void Start ()
	{
	    var parser = new BIM_Parser(FilePath);
	    while (!parser.IsEnd())
	    {
            var entity = parser.GetNextIfcEntity();
	        _entities.Add(entity.Id, entity);
	    }
	}
}

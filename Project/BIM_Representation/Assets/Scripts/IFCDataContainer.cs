using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IFCDataContainer
{
    //private MultiValueDictionary<IFCEntityTypes, IFCGeometricRepresentationItem> _items = new MultiValueDictionary<IFCEntityTypes, IFCGeometricRepresentationItem>();
    private ILookup<IFCEntityTypes, IFCEntity> _itemsByType;
    private ILookup<uint, IFCEntity> _itemsById;

    public IFCDataContainer(string filePath, uint batchSize = 500)
    {

        // Create the parser
        var parser = new IFCParser(filePath);

        // Check if the parser and file are valid
        if (!parser.IsValid()) return;
        Debug.Log(filePath + ": Valid! Let's parse!");

        // Parse and save Data 
        var items = parser.ReadData(batchSize);
        _itemsByType = items.ToLookup(item => item.Type, item => item);
        _itemsById = items.ToLookup(item => item.Id, item => item);
        var points = _itemsByType[IFCEntityTypes.IFCCARTESIANPOINT].ToList();
        foreach (var p in points)
        {
            var cartesianPoint = new IFCCartesianPoint(p);
            var coords = cartesianPoint.GetValueList("Coordinates", value => float.Parse(value));
            Debug.Log("Point #" + cartesianPoint.Id + ": " + Helpers.ListToString(coords));
        }
        // Close the parser
        parser.Close();
    }
}

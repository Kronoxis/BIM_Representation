using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#region Header Block
public struct HeaderBlock
{
    public bool Valid;
    public string FileDescription;
    public string FileName;
    public string FileSchema;

    public HeaderBlock(StreamReader sr)
    {
        Valid = false;
        FileDescription = GetHeaderProperty(sr.ReadLine());
        FileName = GetHeaderProperty(sr.ReadLine());
        FileSchema = GetHeaderProperty(sr.ReadLine());
    }

    private static string GetHeaderProperty(string line)
    {
        var begin = line.IndexOf('(') + 1;
        var length = line.Length - 2 - begin;
        return line.Substring(begin, length);
    }

    public override string ToString()
    {
        return "FileDescription: " + FileDescription + "\nFileName: " + FileName + "\nFileSchema: " + FileSchema;
    }
}
#endregion

#region Entity
public struct IfcEntity
{
    public uint Id;
    public IIfcTopologicalRepresentationItem Item;
    public Type T;

    public IfcEntity(uint id, IIfcTopologicalRepresentationItem item)
    {
        Id = id;
        Item = item;
        T = Item.GetType();
    }
}

//public struct IfcEntity : IIfcTopologicalRepresentationItem
//{
//    public IfcEntityTypes EntityType;
//    public List<string> Data;

//    public IfcEntity(string line)
//    {
//        EntityType = GetEntityType(line);
//        Data = SplitLine(line);
//    }

//    private static IfcEntityTypes GetEntityType(string line)
//    {
//        var begin = line.IndexOf('=') + 2;
//        var length = line.Length - line.IndexOf('(') - begin;
//        var lineType = line.Substring(begin, length);
//        var ifcEntityTypes = Enum.GetValues(typeof(IfcEntityTypes)).Cast<IfcEntityTypes>().ToList();
//        foreach (var ifcEntityType in ifcEntityTypes)
//        {
//            if (lineType.ToUpper().Contains(ifcEntityType.ToString().ToUpper()))
//            {
//                return ifcEntityType;
//            }
//        }
//        return IfcEntityTypes.Null;
//    }

//    private static List<string> SplitLine(string line, char delim = ',')
//    {
//        char[] delims = {delim};
//        var begin = line.IndexOf('(') + 1;
//        var length = line.Length - 2 - begin;
//        return line.Substring(begin, length).Split(delims).ToList();
//    }
//}
#endregion

#region Topological Representation Item Interface
public interface IIfcTopologicalRepresentationItem
{
}
#endregion

#region Null
public struct IfcNull : IIfcTopologicalRepresentationItem
{
}
#endregion

#region Point
public interface IPoint : IIfcTopologicalRepresentationItem
{
    Vector3 GetVector3();
}

public struct IfcCartesionPoint : IPoint
{
    public List<float> Coordinates;

    public IfcCartesionPoint(string line)
    {
        var begin = line.IndexOf('(') + 2;
        var end = line.Length - 3 - begin;
        Coordinates = ParseHelpers.StringToFloatArr(line.Substring(begin, end)).ToList();
    }

    Vector3 IPoint.GetVector3()
    {
        if (Coordinates.Count < 3)
        {
            Debug.LogError("Cartesian Point does not have 3 dimensions");
            return Vector3.zero;
        }
        return new Vector3(Coordinates[0], Coordinates[1], Coordinates[2]);
    }
}
#endregion

#region Loop
public interface IIfcLoop : IIfcTopologicalRepresentationItem
{
}

public struct IfcPolyLoop : IIfcLoop
{
    public List<IPoint> Points;

    public IfcPolyLoop(List<IPoint> points)
    {
        Points = points;
    }
}
#endregion

#region Face Bound
public interface IIfcFaceBound : IIfcTopologicalRepresentationItem
{
}

public struct IfcFaceOuterBound : IIfcFaceBound
{
    public IIfcLoop Loop;
    public bool Orientation;

    public IfcFaceOuterBound(IIfcLoop loop, bool orientation)
    {
        Loop = loop;
        Orientation = orientation;
    }
}
#endregion

#region Face
public struct IfcFace : IIfcTopologicalRepresentationItem
{
    public IIfcFaceBound FaceBound;

    public IfcFace(IIfcFaceBound faceBound)
    {
        FaceBound = faceBound;
    }
}
#endregion

#region Connected Face Set
public interface IIfcConnectedFaceSet : IIfcTopologicalRepresentationItem
{
}

public struct IfcClosedShell : IIfcConnectedFaceSet
{
    public List<IfcFace> Faces;

    public IfcClosedShell(List<IfcFace> faces)
    {
        Faces = faces;
    }
}
#endregion
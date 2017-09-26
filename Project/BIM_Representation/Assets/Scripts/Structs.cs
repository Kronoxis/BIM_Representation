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
//public struct IfcEntity
//{
//    public IIfcTopologicalRepresentationItem Item;
//    public Type ItemType;

//    public IfcEntity(IIfcTopologicalRepresentationItem item)
//    {
//        Item = item;
//        ItemType = Item.GetType();
//    }

//    public uint GetId()
//    {
//        return Convert.ChangeType(Item, ItemType).ToString();
//        return Item.
//    }

//    public override string ToString()
//    {
//        return Convert.ChangeType(Item, ItemType).ToString();
//    }
//}

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

//#region Topological Representation Item Interface
//public interface IIfcTopologicalRepresentationItem
//{
//    string ToString();
//}
//#endregion

//#region Null
//public struct IfcNull : IIfcTopologicalRepresentationItem
//{
//    public uint Id;

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        return "Null";
//    }

//    public static bool operator ==(IfcNull a, IfcNull b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcNull a, IfcNull b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion

//#region Point
//public interface IPoint : IIfcTopologicalRepresentationItem
//{
//    Vector3 GetVector3();
//}

//public struct IfcCartesionPoint : IPoint
//{
//    public uint Id;
//    public List<float> Coordinates;

//    public IfcCartesionPoint(string line)
//    {
//        Id = ParseHelpers.GetId(line);
//        var begin = line.IndexOf('(') + 2;
//        var end = line.Length - 3 - begin;
//        Coordinates = ParseHelpers.StringToFloatArr(line.Substring(begin, end)).ToList();
//    }

//    Vector3 IPoint.GetVector3()
//    {
//        if (Coordinates.Count < 3)
//        {
//            Debug.LogError("Cartesian Point does not have 3 dimensions");
//            return Vector3.zero;
//        }
//        return new Vector3(Coordinates[0], Coordinates[1], Coordinates[2]);
//    }

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        string toRet = "ID: # " + Id + ": IfcCartesionPoint(";
//        foreach (var c in Coordinates)
//        {
//            toRet += c + (c == Coordinates[Coordinates.Count - 1] ? ")" : ", ");
//        }
//        return toRet;
//    }

//    public static bool operator ==(IfcCartesionPoint a, IfcCartesionPoint b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcCartesionPoint a, IfcCartesionPoint b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion

//#region Loop
//public interface IIfcLoop : IIfcTopologicalRepresentationItem
//{
//}

//public struct IfcPolyLoop : IIfcLoop
//{
//    public uint Id;
//    public List<IPoint> Points;

//    public IfcPolyLoop(string line, List<IPoint> points)
//    {
//        Id = ParseHelpers.GetId(line);
//        Points = points;
//    }

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        string toRet = "ID: #" + Id + ": IfcPolyLoop(";
//        foreach (var p in Points)
//        {
//            toRet += p + (p == Points[Points.Count - 1] ? ")" : ", ");
//        }
//        return toRet;
//    }

//    public static bool operator ==(IfcPolyLoop a, IfcPolyLoop b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcPolyLoop a, IfcPolyLoop b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion

//#region Face Bound
//public interface IIfcFaceBound : IIfcTopologicalRepresentationItem
//{
//}

//public struct IfcFaceOuterBound : IIfcFaceBound
//{
//    public uint Id;
//    public IIfcLoop Loop;
//    public bool Orientation;

//    public IfcFaceOuterBound(string line, IIfcLoop loop, bool orientation)
//    {
//        Id = ParseHelpers.GetId(line);
//        Loop = loop;
//        Orientation = orientation;
//    }

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        return "ID: #" + Id + ": IfcFaceOuterBound(" + Loop + ", " + Orientation + ")";
//    }

//    public static bool operator ==(IfcFaceOuterBound a, IfcFaceOuterBound b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcFaceOuterBound a, IfcFaceOuterBound b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion

//#region Face
//public struct IfcFace : IIfcTopologicalRepresentationItem
//{
//    public uint Id;
//    public IIfcFaceBound FaceBound;

//    public IfcFace(string line, IIfcFaceBound faceBound)
//    {
//        Id = ParseHelpers.GetId(line);
//        FaceBound = faceBound;
//    }

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        return "ID: #" + Id + ": IfcFaceOuterBound(" + FaceBound + ")";
//    }

//    public static bool operator ==(IfcFace a, IfcFace b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcFace a, IfcFace b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion

//#region Connected Face Set
//public interface IIfcConnectedFaceSet : IIfcTopologicalRepresentationItem
//{
//}

//public struct IfcClosedShell : IIfcConnectedFaceSet
//{
//    public uint Id;
//    public List<IfcFace> Faces;

//    public IfcClosedShell(string line, List<IfcFace> faces)
//    {
//        Id = ParseHelpers.GetId(line);
//        Faces = faces;
//    }

//    string IIfcTopologicalRepresentationItem.ToString()
//    {
//        string toRet = "ID: #" + Id + ": IfcFaceOuterBound(";
//        foreach (var face in Faces)
//        {
//            toRet += face + (face == Faces[Faces.Count - 1] ? ")" : ", ");
//        }
//        return toRet;
//    }

//    public static bool operator ==(IfcClosedShell a, IfcClosedShell b)
//    {
//        return a.Id == b.Id;
//    }

//    public static bool operator !=(IfcClosedShell a, IfcClosedShell b)
//    {
//        return a.Id != b.Id;
//    }
//}
//#endregion
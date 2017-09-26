using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Topological Representation Item Base
public abstract class IfcTopologicalRepresentationItem
{
    public uint Id;

    public override string ToString()
    {
        return "ID: #" + Id + ": ";
    }

    public uint GetId()
    {
        return Id;
    }

    public static bool operator ==(IfcTopologicalRepresentationItem a, IfcTopologicalRepresentationItem b)
    {
        return a.Id == b.Id;
    }

    public override bool Equals(System.Object obj)
    {
        var other = obj as IfcTopologicalRepresentationItem;
        if ((object)other == null)
            return false;

        return Id != other.Id;
    }

    public static bool operator !=(IfcTopologicalRepresentationItem a, IfcTopologicalRepresentationItem b)
    {
        return a.Id != b.Id;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ (int)Id;
    }
}
#endregion

#region Null
public class IfcNull : IfcTopologicalRepresentationItem
{
    public override string ToString()
    {
        return "Null";
    }
}
#endregion

#region Point
public abstract class Point : IfcTopologicalRepresentationItem
{
}

public class IfcCartesionPoint : Point
{
    public List<float> Coordinates;

    public IfcCartesionPoint(uint id, string line)
    {
        Id = id;
        var begin = line.IndexOf('(') + 2;
        var length = line.Length - 3 - begin;
        Coordinates = ParseHelpers.StringToFloatArr(line.Substring(begin, length)).ToList();
    }

    public Vector3 GetVector3()
    {
        if (Coordinates.Count < 3)
        {
            Debug.LogError("Cartesian Point does not have 3 dimensions");
            return Vector3.zero;
        }
        return new Vector3(Coordinates[0], Coordinates[1], Coordinates[2]);
    }

    public new string ToString()
    {
        string toRet = "ID: # " + Id + ": IfcCartesionPoint(";
        foreach (var c in Coordinates)
        {
            toRet += c + (c == Coordinates[Coordinates.Count - 1] ? ")" : ", ");
        }
        return toRet;
    }
}
#endregion

#region Loop
public abstract class IfcLoop : IfcTopologicalRepresentationItem
{
}

public class IfcPolyLoop : IfcLoop
{
    public List<Point> Points;

    public IfcPolyLoop(uint id, string line)
    {
        Id = id;
        Points = new List<Point>();
        foreach (var vId in ParseHelpers.GetValueIds(line, "(("))
        {
            Points.Add(BIM_Parser.GetItem<Point>(vId));
        }
    }

    public new string ToString()
    {
        string toRet = "ID: #" + Id + ": IfcPolyLoop(";
        foreach (var p in Points)
        {
            toRet += p + (p == Points[Points.Count - 1] ? ")" : ", ");
        }
        return toRet;
    }
}
#endregion

#region Face Bound
public abstract class IfcFaceBound : IfcTopologicalRepresentationItem
{
}

public class IfcFaceOuterBound : IfcFaceBound
{
    public IfcLoop Loop;
    public bool Orientation;

    public IfcFaceOuterBound(uint id, string line)
    {
        Id = id;
        var values = ParseHelpers.GetValues(line, "(");
        Loop = BIM_Parser.GetItem<IfcLoop>(ParseHelpers.GetValueId(values[0]));
        Orientation = values[1].Contains("T");
    }

    public new string ToString()
    {
        return "ID: #" + Id + ": IfcFaceOuterBound(" + Loop + ", " + Orientation + ")";
    }
}
#endregion

#region Face
public class IfcFace : IfcTopologicalRepresentationItem
{
    public IfcFaceBound FaceBound;

    public IfcFace(uint id, string line)
    {
        Id = id;
        var vIds = ParseHelpers.GetValueIds(line, "((");
        FaceBound = BIM_Parser.GetItem<IfcFaceBound>(vIds[0]);
    }

    public new string ToString()
    {
        return "ID: #" + Id + ": IfcFaceOuterBound(" + FaceBound + ")";
    }
}
#endregion

#region Connected Face Set
public abstract class IfcConnectedFaceSet : IfcTopologicalRepresentationItem
{
}

public class IfcClosedShell : IfcTopologicalRepresentationItem
{
    public List<IfcFace> Faces;

    public IfcClosedShell(uint id, string line)
    {
        Id = id;
        Faces = new List<IfcFace>();
        foreach (var vId in ParseHelpers.GetValueIds(line, "(("))
        {
            Faces.Add(BIM_Parser.GetItem<IfcFace>(vId));
        }
    }

    public new string ToString()
    {
        string toRet = "ID: #" + Id + ": IfcFaceOuterBound(";
        foreach (var face in Faces)
        {
            toRet += face + (face == Faces[Faces.Count - 1] ? ")" : ", ");
        }
        return toRet;
    }
}
#endregion
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
#region Topological Representation Item Base
public abstract class IfcTopologicalRepresentationItem
{
    public uint Id;

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
}
#endregion

#region Point
public abstract class Point : IfcTopologicalRepresentationItem
{
    public List<float> Coordinates;

    public Vector3 GetVector3()
    {
        if (Coordinates.Count < 3)
        {
            Debug.LogError("Cartesian Point does not have 3 dimensions");
            return Vector3.zero;
        }
        return new Vector3(Coordinates[0], Coordinates[1], Coordinates[2]);
    }
}

public class IfcCartesionPoint : Point
{

    public IfcCartesionPoint(uint id, string line)
    {
        Id = id;
        var begin = line.IndexOf('(') + 2;
        var length = line.Length - 3 - begin;
        Coordinates = ParseHelpers.StringToFloatArr(line.Substring(begin, length)).ToList();
    }
}
#endregion

#region Loop
public abstract class IfcLoop : IfcTopologicalRepresentationItem
{
    public List<uint> PointRefs;
}

public class IfcPolyLoop : IfcLoop
{

    public IfcPolyLoop(uint id, string line)
    {
        Id = id;
        PointRefs = new List<uint>();
        foreach (var vId in ParseHelpers.GetValueIds(line, "(("))
        {
            PointRefs.Add(vId);
        }
    }
}
#endregion

#region Face Bound
public abstract class IfcFaceBound : IfcTopologicalRepresentationItem
{
    public uint LoopRef;
    public bool Orientation;
}

public class IfcFaceOuterBound : IfcFaceBound
{
    public IfcFaceOuterBound(uint id, string line)
    {
        Id = id;
        var values = ParseHelpers.GetValues(line, "(");
        LoopRef = ParseHelpers.GetValueId(values[0]);
        Orientation = values[1].Contains("T");
    }
}
#endregion

#region Face
public class IfcFace : IfcTopologicalRepresentationItem
{
    public uint FaceBoundRef;

    public IfcFace(uint id, string line)
    {
        Id = id;
        var vIds = ParseHelpers.GetValueIds(line, "((");
        FaceBoundRef = vIds[0];
    }
}
#endregion

#region Connected Face Set
public abstract class IfcConnectedFaceSet : IfcTopologicalRepresentationItem
{
    public List<uint> FaceRefs;

    public List<Vector3> GetVertices(BIM_Generator generator)
    {
        List<Vector3> verts = new List<Vector3>();
        foreach (var fRef in FaceRefs)
        {
            var face = generator.GetItem<IfcFace>(fRef);
            var faceBound = generator.GetItem<IfcFaceOuterBound>(face.FaceBoundRef);
            var loop = generator.GetItem<IfcPolyLoop>(faceBound.LoopRef);
            var points = generator.GetItems<IfcCartesionPoint>(loop.PointRefs);
            foreach (var p in points)
            {
                verts.Add(p.GetVector3());
            }
        }
        return verts;
    }

    public List<int> GetIndices(BIM_Generator generator)
    {
        List<int> indices = new List<int>();
        foreach (var fRef in FaceRefs)
        {
            var face = generator.GetItem<IfcFace>(fRef);
            var faceBound = generator.GetItem<IfcFaceOuterBound>(face.FaceBoundRef);
            var loop = generator.GetItem<IfcPolyLoop>(faceBound.LoopRef);
            var points = generator.GetItems<IfcCartesionPoint>(loop.PointRefs);
            foreach (var p in points)
            {
                indices.Add(indices.Count);
            }
        }
        return indices;
    }
}

public class IfcClosedShell : IfcConnectedFaceSet
{
    public IfcClosedShell(uint id, string line)
    {
        Id = id;
        FaceRefs = new List<uint>();
        foreach (var vId in ParseHelpers.GetValueIds(line, "(("))
        {
            FaceRefs.Add(vId);
        }
    }
}
#endregion
*/
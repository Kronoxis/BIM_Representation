using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#region Enums
public enum IFCEntityTypes
{
    IFCPOINT,
    IFCCARTESIANPOINT,
    IFCPOINTONCURVE,
    IFCPOINTONSURFACE,

    IFCVERTEX,
    IFCVERTEXPOINT,

    IFCDIRECTION,

    IFCAXIS2PLACEMENT,
    IFCAXIS2PLACEMENT2D,
    IFCAXIS2PLACEMENT3D,

    IFCCURVE,
    IFCCOMPOSITECURVE,
    IFC2DCOMPOSITECURVE,
    IFCPOLYLINE,
    IFCTRIMMEDCURVE,
    IFCBSPLINECURVE,
    IFCRATIONALBEZIERCURVE,
    IFCCIRCLE,
    IFCELLIPSE,
    IFCLINE,
    IFCOFFSETCURVE2D,
    IFCOFFSETCURVE3D,

    IFCGRIDAXIS,

    IFCVIRTUALGRIDINTERSECTION,

    IFCGRIDPLACEMENT,
    IFCLOCALPLACEMENT,

    IFCEDGE,
    IFCORIENTEDEDGE,
    IFCEDGECURVE,
    IFCSUBEDGE,

    IFCPOLYLOOP,
    IFCVERTEXLOOP,
    IFCEDGELOOP,

    IFCFACEBOUND,
    IFCFACEOUTERBOUND,

    IFCFACE,
    IFCFACESURFACE,

    IFCCONNECTEDFACESET,
    IFCCLOSEDSHELL,
    IFCOPENSHELL,

    NULL
}

public enum IFCPropertyTypes
{
    INTEGER,
    FLOAT,
    BOOLEAN,
    ENTITY,
    LIST,
    ENUM,
    STRING,

    NULL
}

public enum IFCProfileTypes
{
    CURVE,
    AREA
}

public enum IFCMasterRepresentations
{
    CARTESIAN,
    PARAMETER,
    UNSPECIFIED
}

public enum IFCCurveForm
{
    POLYLINE_FORM,
    CIRCULAR_ARC,
    ELLIPTIC_ARC,
    PARABOLIC_ARC,
    HYPERBOLIC_ARC,
    UNSPECIFIED
}
#endregion

#region Property
public interface IIFCPropertyField
{
}

public struct IFCPropertyField<T> : IIFCPropertyField
{
    private T _value;

    public IFCPropertyField(T value)
    {
        _value = value;
    }

    public T GetValue()
    {
        return _value;
    }

    public void SetValue(T value)
    {
        _value = value;
    }
}

public class IFCProperties
{
    private List<IIFCPropertyField> _properties;

    public IFCProperties()
    {
        _properties = new List<IIFCPropertyField>();
    }

    public void AddProperty<T>(T value)
    {
        _properties.Add(new IFCPropertyField<T>(value));
    }

    public IIFCPropertyField GetProperty(int index)
    {
        if (index < 0 || index >= _properties.Count) return null;
        return _properties[index];
    }

    public IFCPropertyField<T> GetProperty<T>(int index)
    {
        if (index < 0 || index >= _properties.Count) return new IFCPropertyField<T>();
        return (IFCPropertyField<T>)_properties[index];
    }
    public List<IIFCPropertyField> GetProperties()
    {
        return _properties;
    }

    public List<IFCPropertyField<T>> GetProperties<T>()
    {
        List<IFCPropertyField<T>> toRet = new List<IFCPropertyField<T>>();
        foreach (var value in _properties)
            toRet.Add((IFCPropertyField<T>) value);
        return toRet;
    }
}
#endregion

#region Geometric Representation Items
public class IFCEntity
{
    public uint Id;
    public IFCEntityTypes Type;
    public IFCProperties Properties;

    protected List<KeyValuePair<string, Type>> Keys = new List<KeyValuePair<string, Type>>();
    protected Dictionary<string, IIFCPropertyField> Variables = new Dictionary<string, IIFCPropertyField>();

    #region Constructors
    public IFCEntity()
    {
        Id = 0;
        Type = IFCEntityTypes.NULL;
        Properties = new IFCProperties();
    }

    public IFCEntity(uint id, IFCEntityTypes type, string propertiesStr, char delim)
    {
        Id = id;
        Type = type;
        List<char> buffer = new List<char>();
        buffer.AddRange(propertiesStr.ToCharArray());
        Properties = ParseProperties(buffer, delim);
    }
    #endregion

    #region Copy Entity with customized variables
    protected void SetVariables(IFCEntity e)
    {
        Id = e.Id;
        Type = e.Type;
        Properties = e.Properties;

        for (int i = 0; i < Keys.Count; ++i)
        {
            Variables[Keys[i].Key] = Properties.GetProperty(i);
        }
    }
    #endregion

    #region Getters
    public T GetValue<T>(string name, Func<string, T> converter)
    {
        // Check matching type
        var givenType = typeof(T);
        var expectedType = GetValueType(name);
        if (givenType != expectedType)
        {
            Debug.LogError("IFCEntity.GetValue() >> Type (" + givenType + ") did not match Variable (" + expectedType + ")!");
            return default(T);
        }
        // Convert variable to type
        return converter(((IFCPropertyField<string>) Variables[name]).GetValue());
    }

    public List<T> GetValueList<T>(string name, Func<string, T> converter)
    {
        // Check matching type
        var givenType = typeof(List<T>);
        var expectedType = GetValueType(name);
        if (givenType != expectedType)
        {
            Debug.LogError("IFCEntity.GetValueList() >> Type (" + givenType + ") did not match Variable (" + expectedType + ")!");
            return new List<T>();
        }
        // Convert list of variables to type
        List<T> toRet = new List<T>();
        foreach (var s in ((IFCPropertyField<IFCProperties>) Variables[name]).GetValue().GetProperties<string>())
        {
            toRet.Add(converter(s.GetValue()));
        }
        return toRet;
    }

    public Type GetValueType(string name)
    {
        foreach (var key in Keys)
            if (key.Key == name) return key.Value;
        return null;
    }
    #endregion

    #region Parse line to properties
    private IFCProperties ParseProperties(List<char> buffer, char delim)
    {
        IFCProperties toRet = new IFCProperties();

        string property = "";
        while (buffer.Count > 0)
        {
            // Get first character in buffer
            char c = buffer[0];
            // Shift buffer
            buffer.RemoveAt(0);

            // Brackets encapsulate a list of properties (= another IFCProperties variable)
            if (c == '(')
            {
                toRet.AddProperty(ParseProperties(buffer, delim));
                continue;
            }

            // Delimiter marks end of one property
            if (c == delim)
            {
                // Add property
                if (!string.IsNullOrEmpty(property))
                    toRet.AddProperty(property);
                // Clear property
                property = "";
                continue;
            }

            // Closing bracket marks end of a list of properties
            if (c == ')')
            {
                break;
            }

            // Add character to property string
            property += c;
        }
        // Add final property if not empty
        if (!string.IsNullOrEmpty(property))
            toRet.AddProperty(property);
        return toRet;
    }
    #endregion
}

#region IFCPOINT
public abstract class IIFCPoint : IFCEntity
{
    public Vector3 GetVector3()
    {
        var coords = GetValueList("Coordinates", Helpers.ConvertToFloat);
        return new Vector3(coords[0], coords[1], coords[2]);
    }
}

public class IFCCartesianPoint : IIFCPoint
{
    // 0:   Coordinates             List<float>
    public IFCCartesianPoint(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Coordinates", typeof(List<float>)),
        };

        SetVariables(e);
    }
}

public class IFCPointOnCurve : IIFCPoint
{
    // 0:   BasisCurve              uint
    // 1:   PointParameter          float

    public IFCPointOnCurve(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("BasisCurve", typeof(uint)),
            new KeyValuePair<string, Type>("PointParameter", typeof(float)),
        };

        SetVariables(e);
    }
}

public class IFCPointOnSurface : IIFCPoint
{
    // 0:   BasisSurface            uint
    // 1:   PointParameterU         float
    // 2:   PointParameterV         float 

    public IFCPointOnSurface(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("BasisSurface", typeof(uint)),
            new KeyValuePair<string, Type>("PointParameterU", typeof(float)),
            new KeyValuePair<string, Type>("PointParameterV", typeof(float)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCVERTEX
public abstract class IIFCVertex : IFCEntity
{
}

public class IFCVertexPoint : IIFCVertex
{
    // 0:   VertexGeometry          uint

    public IFCVertexPoint(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("VertexGeometry", typeof(uint)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCDIRECTION
public abstract class IIFCDirection : IFCEntity
{
}

public class IFCDirection : IIFCDirection
{
    // 0:   DirectionRatios         List<float>

    public IFCDirection(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("DirectionRatios", typeof(List<float>)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCLOOP

public abstract class IIFCLoop : IFCEntity
{
}

public class IFCPolyLoop : IIFCLoop
{
    // 0:   Polygon                 List<uint>

    public IFCPolyLoop(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Polygon", typeof(List<uint>)),
        };

        SetVariables(e);
    }
}

public class IFCVertexLoop : IIFCLoop
{
    // 0:   LoopVertex              uint

    public IFCVertexLoop(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("LoopVertex", typeof(uint)),
        };

        SetVariables(e);
    }
}

public class IFCEdgeLoop : IIFCLoop
{
    // 0:   EdgeList                List<uint>

    public IFCEdgeLoop(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("EdgeList", typeof(List<uint>)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCFACEBOUND
public abstract class IIFCFaceBound : IFCEntity
{
}

public class IFCFaceBound : IIFCFaceBound
{
    // 0:   Bound                   uint
    // 1:   Orientation             bool

    public IFCFaceBound(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Bound", typeof(uint)),
            new KeyValuePair<string, Type>("Orientation", typeof(bool)),
        };

        SetVariables(e);
    }
}

public class IFCFaceOuterBound : IIFCFaceBound
{
    // 0:   Bound                   uint
    // 1:   Orientation             bool

    public IFCFaceOuterBound(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Bound", typeof(uint)),
            new KeyValuePair<string, Type>("Orientation", typeof(bool)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCFACE
public abstract class IIFCFace : IFCEntity
{
}

public class IFCFace : IIFCFace
{
    // 0:   Bounds                  List<uint>
    public IFCFace(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Bounds", typeof(List<uint>)),
        };

        SetVariables(e);
    }
}

public class IFCFaceSurface : IIFCFace
{
    // 0:   Bounds                  List<uint>
    // 1:   SameSense               bool
    public IFCFaceSurface(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("Bounds", typeof(List<uint>)),
            new KeyValuePair<string, Type>("SameSense", typeof(bool)),
        };

        SetVariables(e);
    }
}
#endregion

#region IFCCONNECTEDFACESET
public abstract class IIFCConnectedFaceSet : IFCEntity
{
    public void GetMeshFilterBuffers(IFCDataContainer container,
        out List<Vector3> vertices, out List<int> indices, out List<Vector3> normals, out List<Vector2> uvs)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        foreach (var faceId in GetValueList("CfsFaces", Helpers.ConvertToUint))
        {
            var face = container.GetEntity(faceId);
            foreach (var boundId in face.GetValueList("Bounds", Helpers.ConvertToUint))
            {
                var bound = container.GetEntity(boundId);
                var loopId = bound.GetValue("Bound", Helpers.ConvertToUint);
                var loop = container.GetEntity(loopId);
                List<Vector3> vertsFromLoop = new List<Vector3>();
                foreach (var pointId in loop.GetValueList("Polygon", Helpers.ConvertToUint))
                {
                    var point = container.GetEntity<IIFCPoint>(pointId);
                    vertsFromLoop.Add(point.GetVector3());
                    //vertices.Add(point.GetVector3());
                }
                AddToBuffer(vertsFromLoop, ref vertices, ref indices, ref normals, ref uvs);
            }
        }
    }

    private void AddToBuffer(List<Vector3> verts, 
        ref List<Vector3> vertices, ref List<int> indices, ref List<Vector3> normals, ref List<Vector2> uvs)
    {
        List<int> vertRefs = new List<int>();
        int index = indices.Count;
        Vector3 normal = CalculateNormal(verts[0], verts[1], verts[2]);
        Vector2 uv = new Vector2(0, 0);

        for (int i = 2; i < verts.Count; ++i)
        {
            vertRefs.Add(0);
            vertRefs.Add(i - 1);
            vertRefs.Add(i);
        }

        for (int i = 0; i < vertRefs.Count; ++i)
        {
            AddToBuffer(verts[vertRefs[i]], index + i, normal, uv,
                ref vertices, ref indices, ref normals, ref uvs);
        }
    }

    private void AddToBuffer(Vector3 vert, int index, Vector3 normal, Vector2 uv, 
        ref List<Vector3> vertices, ref List<int> indices, ref List<Vector3> normals, ref List<Vector2> uvs)
    {
        vertices.Add(vert);
        indices.Add(index);
        normals.Add(normal);
        uvs.Add(uv);
    }

    private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).normalized;
    }
}

public class IFCClosedShell : IIFCConnectedFaceSet
{
    // 0:   CfsFaces                List<uint>

    public IFCClosedShell(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("CfsFaces", typeof(List<uint>)),
        };

        SetVariables(e);
    }
}

public class IFCOpenShell : IIFCConnectedFaceSet
{
    // 0:   CfsFaces                List<uint>

    public IFCOpenShell(IFCEntity e)
    {
        Keys = new List<KeyValuePair<string, Type>>()
        {
            new KeyValuePair<string, Type>("CfsFaces", typeof(List<uint>)),
        };

        SetVariables(e);
    }
}
#endregion
#endregion
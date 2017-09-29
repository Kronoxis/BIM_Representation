using System;
using System.Collections;
using System.Collections.Generic;
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
    private List<IIFCPropertyField> _values;

    public IFCProperties()
    {
        _values = new List<IIFCPropertyField>();
    }

    public void AddValue<T>(T value)
    {
        _values.Add(new IFCPropertyField<T>(value));
    }

    public IIFCPropertyField GetValue(int index)
    {
        if (index < 0 || index >= _values.Count) return null;
        return _values[index];
    }

    public IFCPropertyField<T> GetValue<T>(int index)
    {
        if (index < 0 || index >= _values.Count) return new IFCPropertyField<T>();
        return (IFCPropertyField<T>)_values[index];
    }
    public List<IIFCPropertyField> GetValues()
    {
        return _values;
    }

    public List<IFCPropertyField<T>> GetValues<T>()
    {
        List<IFCPropertyField<T>> toRet = new List<IFCPropertyField<T>>();
        foreach (var value in _values)
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

    protected static List<KeyValuePair<string, Type>> Keys = new List<KeyValuePair<string, Type>>();
    protected Dictionary<string, IIFCPropertyField> Variables = new Dictionary<string, IIFCPropertyField>();
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

    protected void SetVariables(IFCEntity e)
    {
        Id = e.Id;
        Type = e.Type;
        Properties = e.Properties;

        for (int i = 0; i < Keys.Count; ++i)
        {
            Variables[Keys[i].Key] = Properties.GetValue(i);
        }
    }

    public T GetValue<T>(string name, Func<string, T> converter)
    {
        // Check matching type
        var givenType = typeof(T);
        var expectedType = GetType(name);
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
        var expectedType = GetType(name);
        if (givenType != expectedType)
        {
            Debug.LogError("IFCEntity.GetValueList() >> Type (" + givenType + ") did not match Variable (" + expectedType + ")!");
            return new List<T>();
        }
        // Convert list of variables to type
        List<T> toRet = new List<T>();
        foreach (var s in ((IFCPropertyField<IFCProperties>) Variables[name]).GetValue().GetValues<string>())
        {
            toRet.Add(converter(s.GetValue()));
        }
        return toRet;
    }

    public Type GetType(string name)
    {
        foreach (var key in Keys)
            if (key.Key == name) return key.Value;
        return null;
    }

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
                toRet.AddValue(ParseProperties(buffer, delim));
                continue;
            }

            // Delimiter marks end of one property
            if (c == delim)
            {
                // Add property
                if (!string.IsNullOrEmpty(property))
                    toRet.AddValue(property);
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
            toRet.AddValue(property);
        return toRet;
    }
    #endregion
}

#region IFCPOINT
public abstract class IIFCPoint : IFCEntity
{
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
#endregion
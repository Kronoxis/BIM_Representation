using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

#region Enums
public enum IFCUnitEnum
{
    NULL,
    ABSORBEDDOSEUNIT,
    AMOUNTOFSUBSTANCEUNIT,
    AREAUNIT,
    DOSEEQUIVALENTUNIT,
    ELECTRICCAPACITANCEUNIT,
    ELECTRICCHARGEUNIT,
    ELECTRICCONDUCTANCEUNIT,
    ELECTRICCURRENTUNIT,
    ELECTRICRESISTANCEUNIT,
    ELECTRICVOLTAGEUNIT,
    ENERGYUNIT,
    FORCEUNIT,
    FREQUENCYUNIT,
    ILLUMINANCEUNIT,
    INDUCTANCEUNIT,
    LENGTHUNIT,
    LUMINOUSFLUXUNIT,
    LUMINOUSINTENSITYUNIT,
    MAGNETICFLUXDENSITYUNIT,
    MAGNETICFLUXUNIT,
    MASSUNIT,
    PLANEANGLEUNIT,
    POWERUNIT,
    PRESSUREUNIT,
    RADIOACTIVITYUNIT,
    SOLIDANGLEUNIT,
    THERMODYNAMICTEMPERATUREUNIT,
    TIMEUNIT,
    VOLUMEUNIT,
    USERDEFINED
}

public enum IFCSIPrefix
{
    NULL = 0,
    EXA = 18,
    PETA = 15,
    TERA = 12,
    GIGA = 9,
    MEGA = 6,
    KILO = 3,
    HECTO = 2,
    DECA = 1,
    DECI = -1,
    CENTI = -2,
    MILLI = -3,
    MICRO = -6,
    NANO = -9,
    PICO = -12,
    FEMTO = -15,
    ATTO = -18
}

public enum IFCSIUnitName
{
    NULL,
    AMPERE,
    BECQUEREL,
    CANDELA,
    COULOMB,
    CUBIC_METRE,
    DEGREE_CELSIUS,
    FARAD,
    GRAM,
    GRAY,
    HENRY,
    HERTZ,
    JOULE,
    KELVIN,
    LUMEN,
    LUX,
    METRE,
    MOLE,
    NEWTON,
    OHM,
    PASCAL,
    RADIAN,
    SECOND,
    SIEMENS,
    SIEVERT,
    SQUARE_METRE,
    STERADIAN,
    TESLA,
    VOLT,
    WATT,
    WEBER
}

public enum IFCBuildingElementProxyTypeEnum
{
    NULL,
    COMPLEX,
    ELEMENT,
    PARTIAL,
    PROVISIONFORVOID,
    USERDEFINED,
    NOTDEFINED
}

public enum IFCGeometricProjectionEnum
{
    NULL,
    GRAPH_VIEW,
    SKETCH_VIEW,
    MODEL_VIEW,
    PLAN_VIEW,
    REFLECTED_PLAN_VIEW,
    SECTION_VIEW,
    ELEVATION_VIEW,
    USERDEFINED,
    NOTDEFINED
}

//public enum IFCPropertyTypes
//{
//    INTEGER,
//    FLOAT,
//    BOOLEAN,
//    ENTITY,
//    LIST,
//    ENUM,
//    STRING,
//
//    NULL
//}
//
//public enum IFCProfileTypes
//{
//    CURVE,
//    AREA
//}
//
//public enum IFCMasterRepresentations
//{
//    CARTESIAN,
//    PARAMETER,
//    UNSPECIFIED
//}
//
//public enum IFCCurveForm
//{
//    POLYLINE_FORM,
//    CIRCULAR_ARC,
//    ELLIPTIC_ARC,
//    PARABOLIC_ARC,
//    HYPERBOLIC_ARC,
//    UNSPECIFIED
//}
#endregion

#region IFCProperty
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
            toRet.Add((IFCPropertyField<T>)value);
        return toRet;
    }
}
#endregion

#region IFCEntity
public class IFCEntity
{
    public uint Id;
    public Type EntityType;
    public IFCProperties Properties;

    private List<KeyValuePair<string, Type>> Keys = new List<KeyValuePair<string, Type>>();
    private Dictionary<string, IIFCPropertyField> Variables = new Dictionary<string, IIFCPropertyField>();

    #region Constructors
    public IFCEntity()
    {
        Id = 0;
        EntityType = null;
        Properties = null;
    }

    public IFCEntity(uint id, Type type, string propertiesStr, char delim)
    {
        Id = id;
        EntityType = type;
        List<char> buffer = new List<char>();
        buffer.AddRange(propertiesStr.ToCharArray());
        Properties = ParseProperties(buffer, delim);
    }

    public IFCEntity(IFCEntity e)
    {
        Id = e.Id;
        EntityType = e.EntityType;
        Properties = e.Properties;
    }
    #endregion

    #region Methods used in Contructor of derived class
    protected void AddKey(string name, Type type)
    {
        Keys.Add(new KeyValuePair<string, Type>(name, type));
    }

    protected void SetVariables(IFCEntity e)
    {
        Id = e.Id;
        EntityType = e.EntityType;
        Properties = e.Properties;

        for (int i = 0; i < Keys.Count; ++i)
        {
            Variables[Keys[i].Key] = Properties.GetProperty(i);
        }
    }
    #endregion

    #region Getters
    public Type GetPropertyType(string name)
    {
        var matches = Keys.Where(k => k.Key == name).ToArray();
        if (matches.Length < 1)
        {
            Debug.LogError("Couldn't find Property with name " + name);
            return typeof(IFCEntity);
        }
        return matches[0].Value;
    }

    public uint GetIdProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(uint)))
            return default(uint);
        return Helpers.PropertyToId(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<uint> GetIdListProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(List<uint>)))
            return default(List<uint>);
        List<uint> toRet = new List<uint>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToId(x.GetValue())));
        return toRet;
    }

    public int GetIntProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(int)))
            return default(int);
        return Helpers.PropertyToInt(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<int> GetIntListProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(List<int>)))
            return default(List<int>);
        List<int> toRet = new List<int>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToInt(x.GetValue())));
        return toRet;
    }

    public float GetFloatProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(float)))
            return default(float);
        return Helpers.PropertyToFloat(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<float> GetFloatListProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(List<float>)))
            return default(List<float>);
        List<float> toRet = new List<float>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToFloat(x.GetValue())));
        return toRet;
    }

    public bool GetBoolProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(bool)))
            return default(bool);
        return Helpers.PropertyToBool(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<bool> GetBoolListProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(List<bool>)))
            return default(List<bool>);
        List<bool> toRet = new List<bool>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToBool(x.GetValue())));
        return toRet;
    }

    public string GetStringProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(string)))
            return default(string);
        return Helpers.PropertyToString(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<string> GetStringListProperty(string name)
    {
        if (!CheckMatchingType(name, typeof(List<float>)))
            return default(List<string>);
        List<string> toRet = new List<string>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToString(x.GetValue())));
        return toRet;
    }

    public T GetEnumProperty<T>(string name) where T : IConvertible
    {
        if (!CheckMatchingType(name, typeof(T)))
            return default(T);
        return Helpers.PropertyToEnum<T>(((IFCPropertyField<string>)Variables[name]).GetValue());
    }

    public List<T> GetEnumListProperty<T>(string name) where T : IConvertible
    {
        if (!CheckMatchingType(name, typeof(List<T>)))
            return default(List<T>);
        List<T> toRet = new List<T>();
        ((IFCPropertyField<IFCProperties>)Variables[name]).GetValue().GetProperties<string>()
            .ForEach(x => toRet.Add(Helpers.PropertyToEnum<T>(x.GetValue())));
        return toRet;
    }

    private bool CheckMatchingType(string name, Type given)
    {
        if (GetPropertyType(name) != given)
        {
            Debug.LogError("IFCEntity variable " + name + " is of type " + name.GetType() + ", not type " + given);
            return false;
        }
        return true;
    }
    #endregion

    #region Properties from line
    private IFCProperties ParseProperties(List<char> buffer, char delim)
    {
        IFCProperties toRet = new IFCProperties();

        //string property = "";
        StringBuilder property = new StringBuilder();
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
                if (property.Length != 0)
                    toRet.AddProperty(property.ToString());
                // Clear property
                property.Remove(0, property.Length);
                continue;
            }

            // Closing bracket marks end of a list of properties
            if (c == ')')
            {
                break;
            }

            // Add character to property string
            property.Append(c);
        }
        // Add final property if not empty
        if (property.Length != 0)
            toRet.AddProperty(property.ToString());
        return toRet;
    }
    #endregion
}
#endregion

#region IFCPoint
public abstract class IIFCPoint : IFCEntity
{
    public Vector3 GetVector3()
    {
        var coords = GetFloatListProperty("Coordinates");
        return new Vector3(coords[0], coords[1], coords[2]);
    }
}

public class IFCCartesianPoint : IIFCPoint
{
    // 0:   Coordinates             List<float>
    public IFCCartesianPoint(IFCEntity e)
    {
        AddKey("Coordinates", typeof(List<float>));

        SetVariables(e);
    }
}

public class IFCPointOnCurve : IIFCPoint
{
    // 0:   BasisCurve              uint
    // 1:   PointParameter          float

    public IFCPointOnCurve(IFCEntity e)
    {
        AddKey("BasisCurve", typeof(uint));
        AddKey("PointParameter", typeof(float));

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
        AddKey("BasisSurface", typeof(uint));
        AddKey("PointParameterU", typeof(float));
        AddKey("PointParameterV", typeof(float));

        SetVariables(e);
    }
}
#endregion

#region IFCVertex
public abstract class IIFCVertex : IFCEntity
{
}

public class IFCVertexPoint : IIFCVertex
{
    // 0:   VertexGeometry          uint

    public IFCVertexPoint(IFCEntity e)
    {
        AddKey("VertexGeometry", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region IFCDirection
public abstract class IIFCDirection : IFCEntity
{
}

public class IFCDirection : IIFCDirection
{
    // 0:   DirectionRatios         List<float>

    public IFCDirection(IFCEntity e)
    {
        AddKey("DirectionRatios", typeof(List<float>));

        SetVariables(e);
    }
}
#endregion

#region IFCLoop
public abstract class IIFCLoop : IFCEntity
{
}

public class IFCPolyLoop : IIFCLoop
{
    // 0:   Polygon                 List<uint>

    public IFCPolyLoop(IFCEntity e)
    {
        AddKey("Polygon", typeof(List<uint>));

        SetVariables(e);
    }
}

public class IFCVertexLoop : IIFCLoop
{
    // 0:   LoopVertex              uint

    public IFCVertexLoop(IFCEntity e)
    {
        AddKey("LoopVertex", typeof(uint));

        SetVariables(e);
    }
}

public class IFCEdgeLoop : IIFCLoop
{
    // 0:   EdgeList                List<uint>

    public IFCEdgeLoop(IFCEntity e)
    {
        AddKey("EdgeList", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region IFCFaceBound
public abstract class IIFCFaceBound : IFCEntity
{
}

public class IFCFaceBound : IIFCFaceBound
{
    // 0:   Bound                   uint
    // 1:   Orientation             bool

    public IFCFaceBound(IFCEntity e)
    {
        AddKey("Bound", typeof(uint)); ;
        AddKey("Orientation", typeof(bool));

        SetVariables(e);
    }
}

public class IFCFaceOuterBound : IIFCFaceBound
{
    // 0:   Bound                   uint
    // 1:   Orientation             bool

    public IFCFaceOuterBound(IFCEntity e)
    {
        AddKey("Bound", typeof(uint));
        AddKey("Orientation", typeof(bool));

        SetVariables(e);
    }
}
#endregion

#region IFCFace
public abstract class IIFCFace : IFCEntity
{
}

public class IFCFace : IIFCFace
{
    // 0:   Bounds                  List<uint>
    public IFCFace(IFCEntity e)
    {
        AddKey("Bounds", typeof(List<uint>));

        SetVariables(e);
    }
}

public class IFCFaceSurface : IIFCFace
{
    // 0:   Bounds                  List<uint>
    // 1:   SameSense               bool
    public IFCFaceSurface(IFCEntity e)
    {
        AddKey("Bounds", typeof(List<uint>));
        AddKey("SameSense", typeof(bool));

        SetVariables(e);
    }
}
#endregion

#region IFCConnectedFaceSet
public abstract class IIFCConnectedFaceSet : IFCEntity
{
    public void GetMeshFilterBuffers(IFCDataContainer container,
        out List<Vector3> vertices, out List<int> indices, out List<Vector3> normals, out List<Vector2> uvs)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        foreach (var faceId in GetIdListProperty("CfsFaces"))
        {
            var face = container.GetEntity(faceId);
            foreach (var boundId in face.GetIdListProperty("Bounds"))
            {
                var bound = container.GetEntity(boundId);
                var loopId = bound.GetIdProperty("Bound");
                var loop = container.GetEntity(loopId);
                List<Vector3> vertsFromLoop = new List<Vector3>();
                foreach (var pointId in loop.GetIdListProperty("Polygon"))
                {
                    var point = container.GetEntity<IIFCPoint>(pointId);
                    vertsFromLoop.Add(point.GetVector3());
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
        AddKey("CfsFaces", typeof(List<uint>));

        SetVariables(e);
    }
}

public class IFCOpenShell : IIFCConnectedFaceSet
{
    // 0:   CfsFaces                List<uint>

    public IFCOpenShell(IFCEntity e)
    {
        AddKey("CfsFaces", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region IFCNamedUnit

public abstract class IIFCNamedUnit : IFCEntity
{
}

public class IFCSIUnit : IIFCNamedUnit
{
    // 0:   Dimensions              uint
    // 1:   UnitType                IFCUnitEnum
    // 2:   Prefix                  IFCSIPrefix
    // 3:   Name                    IFCSIUnitName

    public IFCSIUnit(IFCEntity e)
    {
        AddKey("Dimensions", typeof(uint));
        AddKey("UnitType", typeof(IFCUnitEnum));
        AddKey("Prefix", typeof(IFCSIPrefix));
        AddKey("Name", typeof(IFCSIUnitName));

        SetVariables(e);
    }
}
#endregion

#region IFCBuildingElement
public abstract class IIFCBuildingElement : IFCEntity
{
}

public class IFCBuildingElementProxy : IIFCBuildingElement
{
    // 0:   GlobalId                string
    // 1:   OwnerHistory            uint
    // 2:   Name                    string
    // 3:   Description             string
    // 4:   ObjectType              string
    // 5:   ObjectPlacement         uint
    // 6:   Representation          uint
    // 7:   Tag                     string
    // 8:   PredefinedType          IFCBuildingElementProxyTypeEnum

    public IFCBuildingElementProxy(IFCEntity e)
    {
        AddKey("GlobalId", typeof(string));
        AddKey("OwnerHistory", typeof(uint));
        AddKey("Name", typeof(string));
        AddKey("Description", typeof(string));
        AddKey("ObjectType", typeof(string));
        AddKey("ObjectPlacement", typeof(uint));
        AddKey("Representation", typeof(uint));
        AddKey("Tag", typeof(string));
        AddKey("PredefinedType", typeof(IFCBuildingElementProxyTypeEnum));

        SetVariables(e);
    }
}
#endregion

#region IFCObjectPlacement
public abstract class IIFCObjectPlacement : IFCEntity
{
}

public class IFCLocalPlacement : IIFCObjectPlacement
{
    // 0:   PlacementRelTo          uint
    // 1:   RelativePlacement       uint

    public IFCLocalPlacement(IFCEntity e)
    {
        AddKey("PlacementRelTo", typeof(uint));
        AddKey("RelativePlacement", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region IFCPlacement
public abstract class IIFCPlacement : IFCEntity
{
}

public class IFCAxis2Placement2D : IIFCPlacement
{
    // 0:   Location                uint
    // 1:   RefDirection            uint

    public IFCAxis2Placement2D(IFCEntity e)
    {
        AddKey("Location", typeof(uint));
        AddKey("RefDirection", typeof(uint));

        SetVariables(e);
    }
}

public class IFCAxis2Placement3D : IIFCPlacement
{
    // 0:   Location                uint
    // 1:   Axis                    uint
    // 2:   RefDirection            uint

    public IFCAxis2Placement3D(IFCEntity e)
    {
        AddKey("Location", typeof(uint));
        AddKey("Axis", typeof(uint));
        AddKey("RefDirection", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region IFCRepresentationContext
public abstract class IIFCRepresentationContext : IFCEntity
{
}

public class IFCGeometricRepresentationContext : IIFCRepresentationContext
{
    // 0:   ContextIdentifier           string
    // 1:   ContextType                 string
    // 2:   CoordinateSpaceDimension    int
    // 3:   Precision                   float
    // 4:   WorldCoordinateSystem       uint
    // 5:   TrueNorth                   uint

    public IFCGeometricRepresentationContext(IFCEntity e)
    {
        AddKey("ContextIdentifier", typeof(string));
        AddKey("ContextType", typeof(string));
        AddKey("CoordinateSpaceDimension", typeof(int));
        AddKey("Precision", typeof(float));
        AddKey("WorldCoordinateSystem", typeof(uint));
        AddKey("TrueNorth", typeof(uint));

        SetVariables(e);
    }
}

public class IFCGeometricRepresentationSubContext : IIFCRepresentationContext
{
    // 0:   ContextIdentifier           string
    // 1:   ContextType                 string
    // 2:   CoordinateSpaceDimension    int
    // 3:   Precision                   float
    // 4:   WorldCoordinateSystem       uint
    // 5:   TrueNorth                   uint
    // 6:   ParentContext               uint
    // 7:   TargetScale                 float
    // 8:   TargetView                  IFCGeometricProjectionEnum
    // 9:   UserDefinedTargetView       string

    public IFCGeometricRepresentationSubContext(IFCEntity e)
    {
        AddKey("ContextIdentifier", typeof(string));
        AddKey("ContextType", typeof(string));
        AddKey("CoordinateSpaceDimension", typeof(int));
        AddKey("Precision", typeof(float));
        AddKey("WorldCoordinateSystem", typeof(uint));
        AddKey("TrueNorth", typeof(uint));
        AddKey("ParentContext", typeof(uint));
        AddKey("TargetScale", typeof(float));
        AddKey("TargetView", typeof(IFCGeometricProjectionEnum));
        AddKey("UserDefinedTargetView", typeof(string));

        SetVariables(e);
    }
}
#endregion
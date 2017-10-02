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

public enum IFCElementCompositionEnum
{
    COMPLEX,
    ELEMENT,
    PARTIAL
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
    private static IEnumerable<Type> _entityTypes = typeof(IFCEntity).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(IFCEntity)));

    private static Dictionary<string, Type> _entityTypesMap = new Dictionary<string, Type>();

    public static Type GetEntityType(string s)
    {
        if (!_entityTypesMap.ContainsKey(s))
            _entityTypesMap[s] = FindMatchingType(s);
        return _entityTypesMap[s];
    }

    private static Type FindMatchingType(string s)
    {
        var matches = _entityTypes.Where(t => t.ToString().ToUpper().Equals(s.ToUpper())).ToArray();
        if (matches.Length < 1) return typeof(IFCEntity);
        return matches[0];
    }

    public string File;
    public uint Id;
    public Type EntityType;
    public IFCProperties Properties;

    private List<KeyValuePair<string, Type>> Keys = new List<KeyValuePair<string, Type>>();
    private Dictionary<string, IIFCPropertyField> Variables = new Dictionary<string, IIFCPropertyField>();

    #region Constructors
    public IFCEntity()
    {
        File = null;
        Id = 0;
        EntityType = null;
        Properties = null;
    }

    public IFCEntity(IFCEntity e)
    {
        File = e.File;
        Id = e.Id;
        EntityType = e.EntityType;
        Properties = e.Properties;
    }

    public IFCEntity(string file, uint id, Type type, string propertiesStr, char delim)
    {
        File = file;
        Id = id;
        EntityType = type;
        List<char> buffer = new List<char>();
        buffer.AddRange(propertiesStr.ToCharArray());
        Properties = ParseProperties(buffer, delim);
    }
    #endregion

    #region Methods used in Contructor of derived class
    protected void AddKey(string name, Type type)
    {
        Keys.Add(new KeyValuePair<string, Type>(name, type));
    }

    protected void SetVariables(IFCEntity e)
    {
        File = e.File;
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

    public T GetReference<T>(string name) where T : IFCEntity
    {
        return IFCDataManager.GetDataContainer(File).GetEntity<T>(GetIdProperty(name));
    }

    public T GetReference<T>(string[] names) where T : IFCEntity
    {
        IFCEntity toRet = this;
        foreach (var name in names)
        {
            toRet = toRet.GetReference<IFCEntity>(name);
        }
        return (T)toRet;
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
                property.Remove(0, property.Length);
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

#region <IIFCPoint> IFCCartesianPoint | IFCPointOnCurve | IFCPointOnSurface
/// <summary>
/// <seealso cref="IFCCartesianPoint "/> |
/// <seealso cref="IFCPointOnCurve "/> |
/// <seealso cref="IFCPointOnSurface "/> |
/// </summary>
public abstract class IIFCPoint : IFCEntity
{
    public Vector3 GetVector3()
    {
        var coords = GetFloatListProperty("Coordinates");
        return new Vector3(coords[0], coords[1], coords[2]);
    }
}

/// <summary>
/// List(float) Coordinates |
/// </summary>
public class IFCCartesianPoint : IIFCPoint
{
    public IFCCartesianPoint(IFCEntity e)
    {
        AddKey("Coordinates", typeof(List<float>));

        SetVariables(e);
    }
}

/// <summary>
/// uint BasisCruve |
/// float PointParameter |
/// </summary>
public class IFCPointOnCurve : IIFCPoint
{
    public IFCPointOnCurve(IFCEntity e)
    {
        AddKey("BasisCurve", typeof(uint));
        AddKey("PointParameter", typeof(float));

        SetVariables(e);
    }
}

/// <summary>
/// uint BasisSurface |
/// float PointParameterU |
/// float PointParameterV |
/// </summary>
public class IFCPointOnSurface : IIFCPoint
{
    public IFCPointOnSurface(IFCEntity e)
    {
        AddKey("BasisSurface", typeof(uint));
        AddKey("PointParameterU", typeof(float));
        AddKey("PointParameterV", typeof(float));

        SetVariables(e);
    }
}
#endregion

#region <IIFCVertex> IFCVertexPoint
/// <summary>
/// <seealso cref="IFCVertexPoint "/> |
/// </summary>
public abstract class IIFCVertex : IFCEntity
{
}

/// <summary>
/// uint VertexGeometry |
/// </summary>
public class IFCVertexPoint : IIFCVertex
{
    public IFCVertexPoint(IFCEntity e)
    {
        AddKey("VertexGeometry", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region <IIFCDirection> IFCDirection
/// <summary>
/// <seealso cref="IFCDirection "/>
/// </summary>
public abstract class IIFCDirection : IFCEntity
{
}

/// <summary>
/// List(float) DirectionRatios |
/// </summary>
public class IFCDirection : IIFCDirection
{
    public IFCDirection(IFCEntity e)
    {
        AddKey("DirectionRatios", typeof(List<float>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCLoop> IFCPolyLoop | IFCVertexLoop | IFCEdgeLoop
/// <summary>
/// <seealso cref="IFCPolyLoop "/> | 
/// <seealso cref="IFCVertexLoop "/> | 
/// <seealso cref="IFCEdgeLoop "/> |
/// </summary>
public abstract class IIFCLoop : IFCEntity
{
}

/// <summary>
/// List(uint) Polygon |
/// </summary>
public class IFCPolyLoop : IIFCLoop
{
    public IFCPolyLoop(IFCEntity e)
    {
        AddKey("Polygon", typeof(List<uint>));

        SetVariables(e);
    }
}

/// <summary>
/// uint LoopVertex |
/// </summary>
public class IFCVertexLoop : IIFCLoop
{
    public IFCVertexLoop(IFCEntity e)
    {
        AddKey("LoopVertex", typeof(uint));

        SetVariables(e);
    }
}

/// <summary>
/// List(uint) EdgeList |
/// </summary>
public class IFCEdgeLoop : IIFCLoop
{
    public IFCEdgeLoop(IFCEntity e)
    {
        AddKey("EdgeList", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCFaceBound> IFCFaceBound | IFCFaceOuterBound
/// <summary>
/// <seealso cref="IFCFaceBound "/> | 
/// <seealso cref="IFCFaceOuterBound "/> |
/// </summary>
public abstract class IIFCFaceBound : IFCEntity
{
}

/// <summary>
/// uint Bound |
/// bool Orientation |
/// </summary>
public class IFCFaceBound : IIFCFaceBound
{
    public IFCFaceBound(IFCEntity e)
    {
        AddKey("Bound", typeof(uint)); ;
        AddKey("Orientation", typeof(bool));

        SetVariables(e);
    }
}

/// <summary>
/// uint Bound |
/// bool Orientation |
/// </summary>
public class IFCFaceOuterBound : IIFCFaceBound
{
    public IFCFaceOuterBound(IFCEntity e)
    {
        AddKey("Bound", typeof(uint));
        AddKey("Orientation", typeof(bool));

        SetVariables(e);
    }
}
#endregion

#region <IIFCFace> IFCFace | IFCFaceSurface
/// <summary>
/// <seealso cref="IFCFace "/> | 
/// <seealso cref="IFCFaceSurface "/> |
/// </summary>
public abstract class IIFCFace : IFCEntity
{
}

/// <summary>
/// List(uint) Bounds |
/// </summary>
public class IFCFace : IIFCFace
{
    public IFCFace(IFCEntity e)
    {
        AddKey("Bounds", typeof(List<uint>));

        SetVariables(e);
    }
}

/// <summary>
/// List(uint) Bounds | 
/// bool SameSense |
/// </summary>
public class IFCFaceSurface : IIFCFace
{
    public IFCFaceSurface(IFCEntity e)
    {
        AddKey("Bounds", typeof(List<uint>));
        AddKey("SameSense", typeof(bool));

        SetVariables(e);
    }
}
#endregion

#region <IIFCConnectedFaceSet> IFCClosedShell | IFCOpenShell
/// <summary>
/// <seealso cref="IFCClosedShell "/> | 
/// <seealso cref="IFCOpenShell "/> |
/// </summary>
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

/// <summary>
/// List(uint) CfsFaces |
/// </summary>
public class IFCClosedShell : IIFCConnectedFaceSet
{
    public IFCClosedShell(IFCEntity e)
    {
        AddKey("CfsFaces", typeof(List<uint>));

        SetVariables(e);
    }
}

/// <summary>
/// List(uint) CfsFaces |
/// </summary>
public class IFCOpenShell : IIFCConnectedFaceSet
{
    public IFCOpenShell(IFCEntity e)
    {
        AddKey("CfsFaces", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCNamedUnit> IFCSIUnit
/// <summary>
/// <seealso cref="IFCSIUnit "/> |
/// </summary>
public abstract class IIFCNamedUnit : IFCEntity
{
}

/// <summary>
/// uint Dimensions |
/// IFCUnitEnum UnitType |
/// IFCSIPrefix Prefix |
/// IFCSIUnitName Name |
/// </summary>
public class IFCSIUnit : IIFCNamedUnit
{
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

#region <IIFCBuildingElement> IFCBuildingElementProxy
/// <summary>
/// <seealso cref="IFCBuildingElementProxy "/> |
/// </summary>
public abstract class IIFCBuildingElement : IFCEntity
{
}

/// <summary>
/// string GlobalId |               
/// uint OwnerHistory |           
/// string Name |                     
/// string Description |             
/// string ObjectType |             
/// uint ObjectPlacement |      
/// uint Representation |
/// string Tag |
/// IFCBuildingElementProxyTypeEnum PredefinedType |
/// </summary>
public class IFCBuildingElementProxy : IIFCBuildingElement
{
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

#region <IIFCObjectPlacement> IFCLocalPlacement
/// <summary>
/// <seealso cref="IFCLocalPlacement "/> |
/// </summary>
public abstract class IIFCObjectPlacement : IFCEntity
{
}

/// <summary>
/// uint PlacementRelTo |
/// uint RelativePlacement |
/// </summary>
public class IFCLocalPlacement : IIFCObjectPlacement
{
    public IFCLocalPlacement(IFCEntity e)
    {
        AddKey("PlacementRelTo", typeof(uint));
        AddKey("RelativePlacement", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region <IIFCPlacement> IFCAxis2Placement2D | IFCAxis2Placement3D
/// <summary>
/// <seealso cref="IFCAxis2Placement2D "/> |
/// <seealso cref="IFCAxis2Placement3D "/> |
/// </summary>
public abstract class IIFCPlacement : IFCEntity
{
}

/// <summary>
/// uint Location | 
/// uint RefDirection |
/// </summary>
public class IFCAxis2Placement2D : IIFCPlacement
{
    public IFCAxis2Placement2D(IFCEntity e)
    {
        AddKey("Location", typeof(uint));
        AddKey("RefDirection", typeof(uint));

        SetVariables(e);
    }
}

/// <summary>
/// uint Location |
/// uint Axis |
/// uint RefDirection |
/// </summary>
public class IFCAxis2Placement3D : IIFCPlacement
{
    public IFCAxis2Placement3D(IFCEntity e)
    {
        AddKey("Location", typeof(uint));
        AddKey("Axis", typeof(uint));
        AddKey("RefDirection", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region <IIFCRepresentationContext> IFCGeometricRepresentationContext | IFCGeometricRepresentationSubContext
/// <summary>
/// <seealso cref="IFCGeometricRepresentationContext "/> |
/// <seealso cref="IFCGeometricRepresentationSubContext "/> |
/// </summary>
public abstract class IIFCRepresentationContext : IFCEntity
{
}

/// <summary>
/// string ContextIdentifier | 
/// string ContextType | 
/// int CoordinateSpaceDimension | 
/// float Precision | 
/// uint WorldCoordinateSystem | 
/// uint TrueNorth | 
/// </summary>
public class IFCGeometricRepresentationContext : IIFCRepresentationContext
{
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

/// <summary>
/// string ContextIdentifier | 
/// string ContextType | 
/// int CoordinateSpaceDimension | 
/// float Precision | 
/// uint WorldCoordinateSystem | 
/// uint TrueNorth | 
/// uint ParentContext | 
/// float TargetScale | 
/// IFCGeometricProjectionEnum TargetView | 
/// string UserDefinedTargetView | 
/// </summary>
public class IFCGeometricRepresentationSubContext : IIFCRepresentationContext
{
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

#region <IIFCRelConnects> IFCRelContainedInSpatialStructure
/// <summary>
/// <seealso cref="IFCRelContainedInSpatialStructure "/> |
/// </summary>
public abstract class IIFCRelConnects : IFCEntity
{
}

/// <summary>
/// string GlobalId | 
/// uint OwnerHistory | 
/// string Name | 
/// string Description | 
/// List(uint) RelatedElements | 
/// List(uint) RelatingStructure | 
/// </summary>
public class IFCRelContainedInSpatialStructure : IIFCRelConnects
{
    public IFCRelContainedInSpatialStructure(IFCEntity e)
    {
        AddKey("GlobalId", typeof(string));
        AddKey("OwnerHistory", typeof(uint));
        AddKey("Name", typeof(string));
        AddKey("Description", typeof(string));
        AddKey("RelatedElements", typeof(List<uint>));
        AddKey("RelatingStructure", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCManifoldSolidBrep> IFCFacetedBrep
/// <summary>
/// <seealso cref="IFCFacetedBrep "/> | 
/// </summary>
public abstract class IIFCManifoldSolidBrep : IFCEntity
{
}

/// <summary>
/// uint Outer | 
/// </summary>
public class IFCFacetedBrep : IIFCManifoldSolidBrep
{
    public IFCFacetedBrep(IFCEntity e)
    {
        AddKey("Outer", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region <IIFCShapeModel> IFCShapeRepresentation
/// <summary>
/// <seealso cref="IFCShapeRepresentation "/> |
/// </summary>
public abstract class IIFCShapeModel : IFCEntity
{
}

/// <summary>
/// uint ContextOfItems | 
/// string RepresentationIdentifier | 
/// string RepresentationType | 
/// List(uint) Items | 
/// </summary>
public class IFCShapeRepresentation : IIFCShapeModel
{
    public IFCShapeRepresentation(IFCEntity e)
    {
        AddKey("ContextOfItems", typeof(uint));
        AddKey("RepresentationIdentifier", typeof(string));
        AddKey("RepresentationType", typeof(string));
        AddKey("Items", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCProductRepresentation> IFCProductDefinitionShape
/// <summary>
/// <seealso cref="IFCProductDefinitionShape "/> | 
/// </summary>
public abstract class IIFCProductRepresentation : IFCEntity
{
}

/// <summary>
/// string Name |
/// string Description | 
/// List(uint) Representations
/// </summary>
public class IFCProductDefinitionShape : IIFCProductRepresentation
{
    public IFCProductDefinitionShape(IFCEntity e)
    {
        AddKey("Name", typeof(string));
        AddKey("Description", typeof(string));
        AddKey("Representations", typeof(List<uint>));

        SetVariables(e);
    }
}
#endregion

#region <IIFCRelDecomposes> IFCRelAggregates
/// <summary>
/// <seealso cref="IFCRelAggregates "/> | 
/// </summary>
public abstract class IIFCRelDecomposes : IFCEntity
{
}

/// <summary>
/// string GlobalId | 
/// uint OwnerHistory | 
/// string Name |
/// string Description | 
/// </summary>
public class IFCRelAggregates : IIFCRelDecomposes
{
    public IFCRelAggregates(IFCEntity e)
    {
        AddKey("GlobalId", typeof(string));
        AddKey("OwnerHistory", typeof(uint));
        AddKey("Name", typeof(string));
        AddKey("Description", typeof(string));

        SetVariables(e);
    }
}
#endregion

#region <IIFCSpatialStructureElement> IFCBuilding
/// <summary>
/// <seealso cref="IFCBuilding "/> | 
/// </summary>
public abstract class IIFCSpatialStructureElement : IFCEntity
{
}

/// <summary>
/// string GlobalId | 
/// uint OwnerHistory | 
/// string Name | 
/// string Description |  
/// string ObjectType | 
/// uint ObjectPlacement | 
/// uint Representation | 
/// string LongName | 
/// IFCElementCompositionEnum CompositionType | 
/// float ElevationOfRefHeight | 
/// float ElevationOfTerrain | 
/// uint BuildingAddress | 
/// </summary>
public class IFCBuilding : IIFCSpatialStructureElement
{
    public IFCBuilding(IFCEntity e)
    {
        AddKey("GlobalId", typeof(string));
        AddKey("OwnerHistory", typeof(uint));
        AddKey("Name", typeof(string));
        AddKey("Description", typeof(string));
        AddKey("ObjectType", typeof(string));
        AddKey("ObjectPlacement", typeof(uint));
        AddKey("Representation", typeof(uint));
        AddKey("LongName", typeof(string));
        AddKey("CompositionType", typeof(IFCElementCompositionEnum));
        AddKey("ElevationOfRefHeight", typeof(float));
        AddKey("ElevationOfTerrain", typeof(float));
        AddKey("BuildingAddress", typeof(uint));

        SetVariables(e);
    }
}
#endregion

#region IFCMeasureWithUnit
/// <summary>
/// List(float) ValueComponent |
/// uint UnitComponent | 
/// <para/>
/// ValueComponent type is hacky! ValueType(v) -> (v) => IFCProperties (= List of properties)
/// </summary>
public class IFCMeasureWithUnit : IFCEntity
{
    public IFCMeasureWithUnit(IFCEntity e)
    {
        AddKey("ValueComponent", typeof(List<float>));
        AddKey("UnitComponent", typeof(uint));
    }
}
#endregion
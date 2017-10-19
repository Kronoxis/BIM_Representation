using System;
using System.Collections.Generic;
using System.IO;
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
    NULL,
    COMPLEX,
    ELEMENT,
    PARTIAL
}

public enum IFCKnotType
{
    NULL,
    UNIFORM_KNOTS,
    QUASI_UNIFORM_KNOTS,
    PIECEWISE_BEZIER_KNOTS,
    UNSPECIFIED
}

public enum IFCTrimmingPreference
{
    NULL,
    CARTESIAN,
    PARAMETER,
    UNSPECIFIED
}

public enum IFCTextPath
{
    NULL,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public enum IFCOccupantTypeEnum
{
    NULL,
    ASSIGNEE,
    ASSIGNOR,
    LESSEE,
    LESSOR,
    LETTINGAGENT,
    OWNER,
    TENANT,
    USERDEFINED,
    NOTDEFINED
}

public enum IFCStateEnum
{
    NULL,
    READWRITE,
    READONLY,
    LOCKED,
    READWRITELOCKED,
    READONLYLOCKED
}

public enum IFCChangeActionEnum
{
    NULL,
    NOCHANGE,
    MODIFIED,
    ADDED,
    DELETED,
    NOTDEFINED
}
#endregion

#region Entities

#region IFCEntity
/// <summary>
/// Entity Base
/// </summary>
public class IFCEntity
{
    #region Static
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
    #endregion

    public FileInfo File;
    public uint Id;

    public List<string> Keys;
    public List<string> Properties;

    public IFCEntity()
    {
        File = null;
        Id = 0;
        Keys = new List<string>();
        Properties = new List<string>();
    }

    public IFCEntity(IFCEntity e)
    {
        File = e.File;
        Id = e.Id;
        Keys = e.Keys;
        Properties = e.Properties;
    }

    public IFCEntity(FileInfo file, uint id, string propertiesStr, char delim)
    {
        File = file;
        Id = id;
        Keys = new List<string>();
        Properties = IfcHelpers.SplitProperties(propertiesStr, delim);
    }

    protected void AddKey(string name)
    {
        Keys.Add(name);
    }

    public string GetProperty(string key)
    {
        for (int i = 0; i < Keys.Count; ++i)
        {
            if (Keys[i] == key) return Properties[i];
        }
        return "";
    }

    public IFCEntity GetReference(string key)
    {
        return IFCDataManager.GetDataContainer(File).GetEntity(GetProperty(key).AsId());
    }

    public T GetReference<T>(string key) where T : IFCEntity
    {
        return IFCDataManager.GetDataContainer(File).GetEntity<T>(GetProperty(key).AsId());
    }

    public List<IFCEntity> GetReferences(string key)
    {
        return IFCDataManager.GetDataContainer(File).GetEntities(GetProperty(key).AsList().AsIds());
    }

    public List<T> GetReferences<T>(string key) where T : IFCEntity
    {
        return IFCDataManager.GetDataContainer(File).GetEntities<T>(GetProperty(key).AsList().AsIds());
    }

    public bool Is<T>(bool includeChildren)
    {
        foreach (var type in _entityTypes)
        {
            if (type == typeof(T) || type.IsSubclassOf(typeof(T)))
            {
                if (GetType() == type)
                    return true;
            }
        }
        return false;
    }
}
#endregion

#region IFCOwnerHistory
/// <summary>
/// 
/// Properties:
/// <see cref="IFCPersonAndOrganization"/> OwningUser |
/// <see cref="IFCApplication"/> OwningApplication |
/// <see cref="IFCStateEnum"/> State |
/// <see cref="IFCChangeActionEnum"/> ChangeAction |
/// <see cref="int"/> LastModifiedDate |
/// <see cref="IFCPersonAndOrganization"/> LastModifyingUser |
/// <see cref="IFCApplication"/> LastModifyingApplication |
/// <see cref="int"/> CreationDate
/// </summary>
public class IFCOwnerHistory : IFCEntity
{
    public IFCOwnerHistory(IFCEntity e) : base(e)
    {
        AddKey("OwningUser");
        AddKey("OwningApplication");
        AddKey("State");
        AddKey("ChangeAction");
        AddKey("LastModifiedDate");
        AddKey("LastModifyingUser");
        AddKey("LastModifyingApplication");
        AddKey("CreationDate");
    }
}
#endregion

#region IFCPerson
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Identification |
/// <see cref="string"/> FamilyName |
/// <see cref="string"/> GivenName |
/// <see cref="string"/>[] MiddleNames |
/// <see cref="string"/>[] PrefixTitles |
/// <see cref="string"/>[] SuffixTitles |
/// <see cref="IFCActorRole"/>[] Roles |
/// <see cref="IFCAddress"/>[] Addresses 
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCPerson : IFCEntity
{
    public IFCPerson(IFCEntity e) : base(e)
    {
        AddKey("Identification");
        AddKey("FamilyName");
        AddKey("GivenName");
        AddKey("MiddleNames");
        AddKey("PrefixTitles");
        AddKey("SuffixTitles");
        AddKey("Roles");
        AddKey("Addresses");
    }
}
#endregion

#region IFCOrganization
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Identification |
/// <see cref="string"/> Name |
/// <see cref="string"/> Description |
/// <see cref="IFCActorRole"/>[] Roles |
/// <see cref="IFCAddress"/>[] Addresses
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCOrganization : IFCEntity
{
    public IFCOrganization(IFCEntity e) : base(e)
    {
        AddKey("Identification");
        AddKey("Name");
        AddKey("Description");
        AddKey("Roles");
        AddKey("Addresses");
    }
}
#endregion

#region IFCPersonAndOrganization
/// <summary>
/// 
/// Properties:
/// <see cref="IFCPerson"/> ThePerson |
/// <see cref="IFCOrganization"/> TheOrganization |
/// <see cref="IFCActorRole"/>[] Roles
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCPersonAndOrganization : IFCEntity
{
    public IFCPersonAndOrganization(IFCEntity e) : base(e)
    {
        AddKey("ThePerson");
        AddKey("TheOrganization");
        AddKey("Roles");
    }
}
#endregion 

#region IFCApplication
/// <summary>
/// 
/// Properties:
/// <see cref="IFCOrganization"/> ApplicationDeveloper |
/// <see cref="string"/> Version |
/// <see cref="string"/> ApplicationFullName |
/// <see cref="string"/> ApplicationIdentifier
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCApplication : IFCEntity
{
    public IFCApplication(IFCEntity e) : base(e)
    {
        AddKey("ApplicationDeveloper");
        AddKey("Version");
        AddKey("ApplicationFullName");
        AddKey("ApplicationIdentifier");
    }
}
#endregion

#region IFCDimensionalExponents
/// <summary>
/// 
/// Properties:
/// <see cref="int"/> LengthExponent |
/// <see cref="int"/> MassExponent |
/// <see cref="int"/> TimeExponent |
/// <see cref="int"/> ElectricCurrentExponent |
/// <see cref="int"/> ThermodynamicTemperatureExponent |
/// <see cref="int"/> AmountOfSubstanceExponent |
/// <see cref="int"/> LuminousIntensityExponent 
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCDimensionalExponents : IFCEntity
{
    public IFCDimensionalExponents(IFCEntity e) : base(e)
    {
        AddKey("LengthExponent");
        AddKey("MassExponent");
        AddKey("TimeExponent");
        AddKey("ElectricCurrentExponent");
        AddKey("ThermodynamicTemperatureExponent");
        AddKey("AmountOfSubstanceExponent");
        AddKey("LuminousIntensityExponent");
    }
}
#endregion

#region IIFCNamedUnit
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDimensionalExponents"/> Dimensions |
/// <see cref="IFCUnitEnum"/> UnitType
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IFCContextDependentUnit"/> |
/// <see cref="IFCConversionBasedUnit"/> |
/// <see cref="IFCSIUnit"/>
/// 
/// </summary>
public abstract class IIFCNamedUnit : IFCEntity
{
    protected IIFCNamedUnit(IFCEntity e) : base(e)
    {
        AddKey("Dimensions");
        AddKey("UnitType");
    }
}

#region IFCSIUnit
/// <summary>
/// 
/// Properties:
/// <see cref="IFCSIPrefix"/> Prefix |
/// <see cref="IFCSIUnitName"/> Name
/// 
/// <para/>Parent:
/// <see cref="IIFCNamedUnit"/>
/// 
/// </summary>
public class IFCSIUnit : IIFCNamedUnit
{
    public IFCSIUnit(IFCEntity e) : base(e)
    {
        AddKey("Prefix");
        AddKey("Name");
    }
}
#endregion

#endregion

#region IIFCObjectPlacement
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IFCGridPlacement"/> |
/// <see cref="IFCLocalPlacement"/>
/// 
/// </summary>
public abstract class IIFCObjectPlacement : IFCEntity
{
    protected IIFCObjectPlacement(IFCEntity e) : base(e)
    {
    }
}

#region IFCGridPlacement
/// <summary>
/// 
/// Properties:
/// <see cref="IFCVirtualGridIntersection"/> PlacementLocation |
/// <see cref="IFCGridPlacementDirectionSelect"/> PlacementRefDirection
/// 
/// <para/>Parent:
/// <see cref="IIFCObjectPlacement"/>
/// 
/// </summary>
public class IFCGridPlacement : IIFCObjectPlacement
{
    public IFCGridPlacement(IFCEntity e) : base(e)
    {
        AddKey("PlacementLocation");
        AddKey("PlacementRefDirection");
    }
}
#endregion

#region IFCLocalPlacement
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCObjectPlacement"/> PlacementRelTo |
/// <see cref="IIFCAxis2Placement"/> RelativePlacement
/// 
/// <para/>Parent:
/// <see cref="IIFCObjectPlacement"/>
/// 
/// </summary>
public class IFCLocalPlacement : IIFCObjectPlacement
{
    public IFCLocalPlacement(IFCEntity e) : base(e)
    {
        AddKey("PlacementRelTo");
        AddKey("RelativePlacement");
    }
}
#endregion

#endregion

#region IIFCRoot
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> GlobalId |
/// <see cref="IFCOwnerHistory"/> OwnerHistory |
/// <see cref="string"/> Name |
/// <see cref="string"/> Description
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IIFCObjectDefinition"/>
/// <see cref="IIFCPropertyDefinition"/>
/// <see cref="IIFCRelationship"/>
/// 
/// </summary>
public abstract class IIFCRoot : IFCEntity
{
    protected IIFCRoot(IFCEntity e) : base(e)
    {
        AddKey("GlobalId");
        AddKey("OwnerHistory");
        AddKey("Name");
        AddKey("Description");
    }
}

#region IIFCObjectDefinition
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCRoot"/>
/// 
/// <para/>Children:
/// <see cref="IIFCContext"/> |
/// <see cref="IIFCObject"/> |
/// 
/// </summary>
public abstract class IIFCObjectDefinition : IIFCRoot
{
    public IIFCObjectDefinition(IFCEntity e) : base(e)
    {
    }
}

#region IIFCContext
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> ObjectType |
/// <see cref="string"/> LongName |
/// <see cref="string"/> Phase |
/// <see cref="IFCRepresentationContext"/>[] RepresentationContexts |
/// <see cref="IFCUnitAssignment"/> UnitsInContext
/// 
/// <para/>Parent:
/// <see cref="IIFCObjectDefinition"/>
/// 
/// <para/>Children:
/// <see cref="IFCProject"/> |
/// <see cref="IFCProjectLibrary"/> 
/// 
/// </summary>
public abstract class IIFCContext : IIFCObjectDefinition
{
    public IIFCContext(IFCEntity e) : base(e)
    {
        AddKey("ObjectType");
        AddKey("LongName");
        AddKey("Phase");
        AddKey("RepresentationContexts");
        AddKey("UnitsInContext");
    }
}

#region IIFCRepresentationContext 
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> ContextIdentifier |
/// <see cref="string"/> ContextType
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IFCGeometricRepresentationContext"/>
/// 
/// </summary>
public abstract class IIFCRepresentationContext : IFCEntity
{
    protected IIFCRepresentationContext(IFCEntity e) : base(e)
    {
        AddKey("ContextIdentifier");
        AddKey("ContextType");
    }
}

#region IFCGeometricRepresentationContext 
/// <summary>
/// 
/// Properties:
/// <see cref="int"/> CoordinateSpaceDimension |
/// <see cref="float"/> Precision |
/// <see cref="IIFCAxis2Placement"/> WorldCoordinateSystem |
/// <see cref="IFCDirection"/> TrueNorth
/// 
/// <para/>Parent:
/// <see cref="IIFCRepresentationContext"/>
/// 
/// <para/>Children:
/// <see cref="IFCGeometricRepresentationSubContext"/>
/// 
/// </summary>
public class IFCGeometricRepresentationContext : IIFCRepresentationContext
{
    public IFCGeometricRepresentationContext(IFCEntity e) : base(e)
    {
        AddKey("CoordinateSpaceDimension");
        AddKey("Precision");
        AddKey("WorldCoordinateSystem");
        AddKey("TrueNorth");
    }
}

#region IFCGeometricRepresentationSubContext
/// <summary>
/// 
/// Properties:
/// <see cref="IFCGeometricRepresentationContext"/> ParentContext |
/// <see cref="float"/> TargetScale |
/// <see cref="IFCGeometricProjectionEnum"/> TargetView |
/// <see cref="string"/> UserDefinedTargetView
/// 
/// <para/>Parent:
/// <see cref="IFCGeometricRepresentationContext"/>
/// 
/// </summary>
public class IFCGeometricRepresentationSubContext : IFCGeometricRepresentationContext
{
    public IFCGeometricRepresentationSubContext(IFCEntity e) : base(e)
    {
        AddKey("ParentContext");
        AddKey("TargetScale");
        AddKey("TargetView");
        AddKey("UserDefinedTargetView");
    }
}
#endregion

#endregion

#endregion

#region IFCProject
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCContext"/>
/// 
/// </summary>
public class IFCProject : IIFCContext
{
    public IFCProject(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCProjectLibrary
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCContext"/>
/// 
/// </summary>
public class IFCProjectLibrary : IIFCContext
{
    public IFCProjectLibrary(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IIFCObject
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> ObjectType
/// 
/// <para/>Parent:
/// <see cref="IIFCObjectDefinition"/>
/// 
/// <para/>Children:
/// <see cref="IFCActor"/> |
/// <see cref="IIFCProduct"/> |
/// 
/// </summary>
public abstract class IIFCObject : IIFCObjectDefinition
{
    protected IIFCObject(IFCEntity e) : base(e)
    {
        AddKey("ObjectType");
    }
}

#region IFCActor
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCActorSelect"/> TheActor
/// 
/// <para/>Parent:
/// <see cref="IIFCObject"/>
/// 
/// <para/>Children:
/// <see cref="IFCOccupant"/>
/// 
/// </summary>
public class IFCActor : IIFCObject
{
    public IFCActor(IFCEntity e) : base(e)
    {
        AddKey("TheActor");
    }
}

#region IIFCActorSelect
/// <summary>
/// 
/// Select:
/// <see cref="IFCOrganization"/> |
/// <see cref="IFCPerson"/> |
/// <see cref="IFCPersonAndOrganization"/>
/// 
/// </summary>
public interface IIFCActorSelect
{
}
#endregion

#region IFCOccupant
/// <summary>
/// 
/// Properties:
/// <see cref="IFCOccupantTypeEnum"/> PredefinedType
/// 
/// <para/>Parent:
/// <see cref="IFCActor"/>
/// 
/// </summary>
public class IFCOccupant : IFCActor
{
    public IFCOccupant(IFCEntity e) : base(e)
    {
        AddKey("PredefinedType");
    }
}
#endregion

#endregion 

#region IFCProduct
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCObjectPlacement"/> ObjectPlacement |
/// <see cref="IIFCProductRepresentation"/> Representation
/// 
/// <para/>Parent:
/// <see cref="IIFCObject"/>
/// 
/// <para/>Children:
/// <see cref="IFCAnnotation"/> |
/// <see cref="IIFCElement"/> |
/// <see cref="IFCGrid"/> |
/// <see cref="IFCPort"/> |
/// <see cref="IFCProxy"/> |
/// <see cref="IIFCSpatialElement"/> |
/// <see cref="IFCStructuralActivity"/> |
/// <see cref="IFCStructuralItem"/>
/// 
/// </summary>
public abstract class IIFCProduct : IIFCObject
{
    protected IIFCProduct(IFCEntity e) : base(e)
    {
        AddKey("ObjectPlacement");
        AddKey("Representation");
    }
}

#region IFCAnnotation
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCProduct"/>
/// 
/// </summary>
public class IFCAnnotation : IIFCProduct
{
    public IFCAnnotation(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IIFCElement
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Tag
/// 
/// <para/>Parent:
/// <see cref="IIFCProduct"/>
/// 
/// <para/>Children:
/// <see cref="IIFCBuildingElement"/> |
/// <see cref="IFCCivilElement"/> | 
/// <see cref="IFCDistributionElement"/> |
/// <see cref="IFCElementAssembly"/> |
/// <see cref="IFCElementComponent"/> |
/// <see cref="IFCFeatureElement"/> |
/// <see cref="IFCFurnishingElement"/> |
/// <see cref="IFCGeographicElement"/> |
/// <see cref="IFCTransportElement"/> |
/// <see cref="IFCVirtualElement"/> 
/// 
/// </summary>
public abstract class IIFCElement : IIFCProduct
{
    protected IIFCElement(IFCEntity e) : base(e)
    {
        AddKey("Tag");
    }
}

#region IFCBuildingElement
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCElement"/>
/// 
/// <para/>Children:
/// <see cref="IFCBeam"/> |
/// <see cref="IFCBuildingElementProxy"/> |
/// <see cref="IFCChimney"/> |
/// <see cref="IFCColumn"/> |
/// <see cref="IFCCovering"/> |
/// <see cref="IFCCurtainWall"/> |
/// <see cref="IFCDoor"/> |
/// <see cref="IFCFooting"/> |
/// <see cref="IFCMember"/> |
/// <see cref="IFCPile"/> |
/// <see cref="IFCPlate"/> |
/// <see cref="IFCRailing"/> |
/// <see cref="IFCRamp"/> |
/// <see cref="IFCRampFlight"/> |
/// <see cref="IFCRoof"/> |
/// <see cref="IFCShadingDevice"/> |
/// <see cref="IFCSlab"/> |
/// <see cref="IFCStair"/> |
/// <see cref="IFCStairFlight"/> |
/// <see cref="IFCWall"/> |
/// <see cref="IFCWindow"/> 
/// 
/// </summary>
public abstract class IIFCBuildingElement : IIFCElement
{
    protected IIFCBuildingElement(IFCEntity e) : base(e)
    {
    }
}

#region IFCBeam
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCBeam : IIFCBuildingElement
{
    public IFCBeam(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCBuildingElementProxy
/// <summary>
/// 
/// Properties:
/// <see cref="IFCBuildingElementProxyTypeEnum"/> PredefinedType
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCBuildingElementProxy : IIFCBuildingElement
{
    public IFCBuildingElementProxy(IFCEntity e) : base(e)
    {
        AddKey("PredefinedType");
    }
}
#endregion

#region IFCChimney
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCChimney : IIFCBuildingElement
{
    public IFCChimney(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCColumn
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCColumn : IIFCBuildingElement
{
    public IFCColumn(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCCovering
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCCovering : IIFCBuildingElement
{
    public IFCCovering(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCCurtainWall
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCCurtainWall : IIFCBuildingElement
{
    public IFCCurtainWall(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCDoor
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCDoor : IIFCBuildingElement
{
    public IFCDoor(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCFooting
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCFooting : IIFCBuildingElement
{
    public IFCFooting(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCMember
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCMember : IIFCBuildingElement
{
    public IFCMember(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCPile
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCPile : IIFCBuildingElement
{
    public IFCPile(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCPlate
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCPlate : IIFCBuildingElement
{
    public IFCPlate(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCRailing
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCRailing : IIFCBuildingElement
{
    public IFCRailing(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCRamp
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCRamp : IIFCBuildingElement
{
    public IFCRamp(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCRampFlight
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCRampFlight : IIFCBuildingElement
{
    public IFCRampFlight(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCRoof
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCRoof : IIFCBuildingElement
{
    public IFCRoof(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCShadingDevice
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCShadingDevice : IIFCBuildingElement
{
    public IFCShadingDevice(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCSlab
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCSlab : IIFCBuildingElement
{
    public IFCSlab(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCStair
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCStair : IIFCBuildingElement
{
    public IFCStair(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCStairFlight
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCStairFlight : IIFCBuildingElement
{
    public IFCStairFlight(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCWall
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCWall : IIFCBuildingElement
{
    public IFCWall(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCWindow
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCBuildingElement"/>
/// 
/// </summary>
public class IFCWindow : IIFCBuildingElement
{
    public IFCWindow(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IFCCivilElement
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCElement"/>
/// 
/// </summary>
public class IFCCivilElement : IIFCElement
{
    public IFCCivilElement(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCDistributionElement
#endregion

#region IFCElementAssembly
#endregion

#region IFCElementComponent
#endregion

#region IFCFeatureElement
#endregion

#region IFCFurnishingElement
#endregion

#region IFCGeographicElement
#endregion

#region IFCTransportElement
#endregion

#region IFCVirtualElement
#endregion

#endregion

#region IFCGrid
#endregion

#region IFCPort
#endregion

#region IFCProxy
#endregion

#region IIFCSpatialElement
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> LongName
/// 
/// <para/>Parent:
/// <see cref="IIFCProduct"/>
/// 
/// <para/>Children:
/// <see cref="IFCExternalSpatialStructureElement"/> |
/// <see cref="IIFCSpatialStructureElement"/> |
/// <see cref="IFCSpatialZone"/>
/// 
/// </summary>
public abstract class IIFCSpatialElement : IIFCProduct
{
    protected IIFCSpatialElement(IFCEntity e) : base(e)
    {
        AddKey("LongName");
    }
}

#region IFCExternalSpatialStructureElement
#endregion

#region IIFCSpatialStructureElement
/// <summary>
/// 
/// Properties:
/// <see cref="IFCElementCompositionEnum"/>
/// 
/// <para/>Parent:
/// <see cref="IIFCSpatialElement"/>
/// 
/// <para/>Children:
/// <see cref="IFCBuilding"/> |
/// <see cref="IFCBuildingStorey"/> |
/// <see cref="IFCSite"/> |
/// <see cref="IFCSpace"/>
/// 
/// </summary>
public abstract class IIFCSpatialStructureElement : IIFCSpatialElement
{
    protected IIFCSpatialStructureElement(IFCEntity e) : base(e)
    {
        AddKey("CompositionType");
    }
}

#region IFCBuilding
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> ElevationOfRefHeight
/// <see cref="float"/> ElevationOfTerrain
/// <see cref="IFCPostalAddress"/> BuildingAddress
/// 
/// <para/>Parent:
/// <see cref="IIFCSpatialStructureElement"/>
/// 
/// </summary>
public class IFCBuilding : IIFCSpatialStructureElement
{
    public IFCBuilding(IFCEntity e) : base(e)
    {
        AddKey("ElevationOfRefHeight");
        AddKey("ElevationOfTerrain");
        AddKey("BuildingAddress");
    }
}
#endregion

#region IFCBuildingStorey
#endregion

#region IFCSite
#endregion

#region IFCSpace
#endregion

#endregion

#region IFCSpatialZone
#endregion

#endregion

#region IFCStructuralActivity
#endregion

#region IFCStructuralItem
#endregion

#endregion 

#endregion

#endregion

#region IIFCPropertyDefinition
#endregion

#region IIFCRelationship
#endregion

#endregion

#region IIFCRepresentation
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCRepresentationContext"/> ContextOfItems |
/// <see cref="string"/> RepresentationIdentifier |
/// <see cref="string"/> RepresentationType |
/// <see cref="IIFCRepresentationItem"/>[] Items
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IIFCShapeModel"/> |
/// <see cref="IIFCStyleModel"/> 
///  
/// </summary>
public abstract class IIFCRepresentation : IFCEntity
{
    protected IIFCRepresentation(IFCEntity e) : base(e)
    {
        AddKey("ContextOfItems");
        AddKey("RepresentationIdentifier");
        AddKey("RepresentationType");
        AddKey("Items");
    }
}

#region IIFCShapeModel
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCRepresentation"/>
/// 
/// <para/>Children:
/// <see cref="IFCShapeRepresentation"/>
/// <see cref="IFCTopologyRepresentation"/>
/// 
/// </summary>
public abstract class IIFCShapeModel : IIFCRepresentation
{
    protected IIFCShapeModel(IFCEntity e) : base(e)
    {
    }
}

#region IFCShapeRepresentation
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCShapeModel"/>
/// 
/// </summary>
public class IFCShapeRepresentation : IIFCShapeModel
{
    public IFCShapeRepresentation(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCTopologyRepresentation
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCShapeModel"/>
/// 
/// </summary>
public class IFCTopologyRepresentation : IIFCShapeModel
{
    public IFCTopologyRepresentation(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IIFCStyleModel
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCRepresentation"/>
/// 
/// <para/>Children:
/// <see cref="IFCStyledRepresentation"/>
/// 
/// </summary>
public abstract class IIFCStyleModel : IIFCRepresentation
{
    protected IIFCStyleModel(IFCEntity e) : base(e)
    {
    }
}

#region IFCStyledRepresentation
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCStyleModel"/>
/// 
/// </summary>
public class IFCStyledRepresentation : IIFCStyleModel
{
    public IFCStyledRepresentation(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#endregion

#region IIFCProductRepresentation
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Name | 
/// <see cref="string"/> Description |
/// <see cref="IIFCRepresentation"/> Representations
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children:
/// <see cref="IFCMaterialDefinitionRepresentation"/>
/// <see cref="IFCProductDefinitionShape"/>
/// 
/// </summary>
public abstract class IIFCProductRepresentation : IFCEntity
{
    public IIFCProductRepresentation(IFCEntity e) : base(e)
    {
        AddKey("Name");
        AddKey("Description");
        AddKey("Representations");
    }
}

#region IFCMaterialDefinitionRepresentation
/// <summary>
/// 
/// Properties:
/// <see cref="IFCMaterial"/> RepresentedMaterial
/// 
/// <para/>Parent: 
/// <see cref="IIFCProductRepresentation"/>
/// 
/// </summary>
public class IFCMaterialDefinitionRepresentation : IIFCProductRepresentation
{
    public IFCMaterialDefinitionRepresentation(IFCEntity e) : base(e)
    {
        AddKey("RepresentedMaterial");
    }
}
#endregion

#region IFCProductDefinitionShape
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCProductRepresentation"/>
/// </summary>
public class IFCProductDefinitionShape : IIFCProductRepresentation
{
    public IFCProductDefinitionShape(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IIFCRepresentationItem
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCEntity"/>
/// 
/// <para/>Children: 
/// <see cref="IIFCGeometricRepresentationItem"/> |
/// <see cref="IFCMappedItem"/> |
/// <see cref="IFCStyledItem"/> |
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// </summary>
public abstract class IIFCRepresentationItem : IFCEntity
{
    protected IIFCRepresentationItem(IFCEntity e) : base(e)
    {
    }
}

#region IIFCGeometricRepresentationItem
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCAnnotationFillArea"/> |
/// <see cref="IFCBooleanResult"/> |
/// <see cref="IFCBoundingBox"/> |
/// <see cref="IIFCCartesianPointList"/> |
/// <see cref="IIFCCartesianTransformationOperator"/> |
/// <see cref="IFCCompositeCurveSegment"/> |
/// <see cref="IIFCCsgPrimitive3D"/> |
/// <see cref="IIFCCurve"/> |
/// <see cref="IFCDirection"/> |
/// <see cref="IFCFaceBasedSurfaceModel"/> |
/// <see cref="IFCFillAreaStyleHatching"/> |
/// <see cref="IFCFillAreaStyleTiles"/> |
/// <see cref="IFCGeometricSet"/> |
/// <see cref="IFCHalfSpaceSolid"/> |
/// <see cref="IIFCLightSource"/> |
/// <see cref="IIFCPlacement"/> |
/// <see cref="IFCPlanarExtent"/> |
/// <see cref="IIFCPoint"/> |
/// <see cref="IFCSectionedSpine"/> |
/// <see cref="IFCShellBasedSurfaceModel"/> |
/// <see cref="IIFCSolidModel"/> |
/// <see cref="IIFCSurface"/> |
/// <see cref="IIFCTessellatedItem"/> |
/// <see cref="IFCTextLiteral"/> |
/// <see cref="IFCVector"/>                                           
/// 
/// </summary>
public abstract class IIFCGeometricRepresentationItem : IIFCRepresentationItem
{
    protected IIFCGeometricRepresentationItem(IFCEntity e) : base(e)
    {
    }
}

#region IFCAnnotationFillArea
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> OuterBoundary
/// <see cref="IIFCCurve"/>[] InnerBoundaries
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCAnnotationFillArea : IIFCGeometricRepresentationItem
{
    public IFCAnnotationFillArea(IFCEntity e) : base(e)
    {
        AddKey("OuterBoundary");
        AddKey("InnerBoundaries");
    }
}
#endregion

#region IFCBooleanResult
/// <summary>
/// 
/// Properties:
/// <see cref="IFCBooleanOperator"/> Operator |
/// <see cref="IFCBooleanOperand"/> FirstOperand |
/// <see cref="IFCBooleanOperand"/> SecondOperand
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCBooleanClippingResult"/>
/// 
/// </summary>
public class IFCBooleanResult : IIFCGeometricRepresentationItem
{
    public IFCBooleanResult(IFCEntity e) : base(e)
    {
        AddKey("Operator");
        AddKey("FirstOperand");
        AddKey("SecondOperand");
    }
}

#region IFCBooleanClippingResult
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCBooleanResult"/>
/// 
/// </summary>
public class IFCBooleanClippingResult : IFCBooleanResult
{
    public IFCBooleanClippingResult(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IFCBoundingBox
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/> Corner |
/// <see cref="float"/> XDim |
/// <see cref="float"/> YDim |
/// <see cref="float"/> ZDim
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCBoundingBox : IIFCGeometricRepresentationItem
{
    public IFCBoundingBox(IFCEntity e) : base(e)
    {
        AddKey("Corner");
        AddKey("XDim");
        AddKey("YDim");
        AddKey("ZDim");
    }
}
#endregion

#region IIFCCartesianPointList
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCCartesianPointList3D"/>
/// 
/// </summary>
public abstract class IIFCCartesianPointList : IIFCGeometricRepresentationItem
{
    protected IIFCCartesianPointList(IFCEntity e) : base(e)
    {
    }
}

#region IFCCartesianPointList3D
/// <summary>
/// 
/// Properties:
/// <see cref="float"/>[][] CoordList
/// 
/// <para/>Parent:
/// <see cref="IIFCCartesianPointList"/>
/// 
/// </summary>
public class IFCCartesianPointList3D : IIFCCartesianPointList
{
    public IFCCartesianPointList3D(IFCEntity e) : base(e)
    {
        AddKey("CoordList");
    }
}
#endregion

#endregion

#region IIFCCartesianTransformationOperator
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Axis1 |
/// <see cref="IFCDirection"/> Axis2 |
/// <see cref="IFCCartesianPoint"/> LocalOrigin |
/// <see cref="float"/> Scale
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCCartesianTransformationOperator2D"/> |
/// <see cref="IFCCartesianTransformationOperator3D"/>
/// 
/// </summary>
public abstract class IIFCCartesianTransformationOperator : IIFCGeometricRepresentationItem
{
    protected IIFCCartesianTransformationOperator(IFCEntity e) : base(e)
    {
        AddKey("Axis1");
        AddKey("Axis2");
        AddKey("LocalOrigin");
        AddKey("Scale");
    }
}

#region IFCCartesianTransformationOperator2D
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCCartesianTransformationOperator"/>
/// 
/// <para/>Children:
/// <see cref="IFCCartesianTransformationOperator2DnonUniform"/>
/// 
/// </summary>
public class IFCCartesianTransformationOperator2D : IIFCCartesianTransformationOperator
{
    public IFCCartesianTransformationOperator2D(IFCEntity e) : base(e)
    {
    }
}

#region IFCCartesianTransformationOperator2DnonUniform
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Scale2
/// 
/// <para/>Parent:
/// <see cref="IFCCartesianTransformationOperator2D"/>
/// 
/// </summary>
public class IFCCartesianTransformationOperator2DnonUniform : IFCCartesianTransformationOperator2D
{
    public IFCCartesianTransformationOperator2DnonUniform(IFCEntity e) : base(e)
    {
        AddKey("Scale2");
    }
}
#endregion

#endregion

#region IFCCartesianTransformationOperator3D
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Axis3
/// 
/// <para/>Parent:
/// <see cref="IIFCCartesianTransformationOperator"/>
/// 
/// <para/>Children:
/// <see cref="IFCCartesianTransformationOperator3DnonUniform"/>
/// 
/// </summary>
public class IFCCartesianTransformationOperator3D : IIFCCartesianTransformationOperator
{
    public IFCCartesianTransformationOperator3D(IFCEntity e) : base(e)
    {
        AddKey("Axis3");
    }
}

#region IFCCartesianTransformationOperator3DnonUniform
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Scale2 |
/// <see cref="float"/> Scale3
/// 
/// <para/>Parent:
/// <see cref="IFCCartesianTransformationOperator3D"/>
/// 
/// </summary>
public class IFCCartesianTransformationOperator3DnonUniform : IFCCartesianTransformationOperator3D
{
    public IFCCartesianTransformationOperator3DnonUniform(IFCEntity e) : base(e)
    {
        AddKey("Scale2");
        AddKey("Scale3");
    }
}
#endregion

#endregion

#endregion

#region IFCCompositeCurveSegment
/// <summary>
/// 
/// Properties:
/// <see cref="IFCTransitionCode"/> Transition |
/// <see cref="bool"/> SameSense |
/// <see cref="IIFCCurve"/> ParentCurve
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCReparametrisedCompositeCurveSegment"/>
/// 
/// </summary>
public class IFCCompositeCurveSegment : IIFCGeometricRepresentationItem
{
    public IFCCompositeCurveSegment(IFCEntity e) : base(e)
    {
        AddKey("Transition");
        AddKey("SameSense");
        AddKey("ParentCurve");
    }
}

#region IFCReparametrisedCompositeCurveSegment
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> ParamLength
/// 
/// <para/>Parent:
/// <see cref="IFCCompositeCurveSegment"/>
/// 
/// </summary>
public class IFCReparametrisedCompositeCurveSegment : IFCCompositeCurveSegment
{
    public IFCReparametrisedCompositeCurveSegment(IFCEntity e) : base(e)
    {
        AddKey("ParamLength");
    }
}
#endregion

#endregion

#region IIFCCsgPrimitive3D
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis2Placement3D"/> Position
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCBlock"/> |
/// <see cref="IFCRectangularPyramid"/> |
/// <see cref="IFCRightCircularCone"/> |
/// <see cref="IFCRightCircularCylinder"/> |
/// <see cref="IFCSphere"/>
/// 
/// </summary>
public abstract class IIFCCsgPrimitive3D : IIFCGeometricRepresentationItem
{
    protected IIFCCsgPrimitive3D(IFCEntity e) : base(e)
    {
        AddKey("Position");
    }
}

#region IFCBlock
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> XLength |
/// <see cref="float"/> YLength |
/// <see cref="float"/> ZLength 
/// 
/// <para/>Parent:
/// <see cref="IIFCCsgPrimitive3D"/>
/// 
/// </summary>
public class IFCBlock : IIFCCsgPrimitive3D
{
    public IFCBlock(IFCEntity e) : base(e)
    {
        AddKey("XLength");
        AddKey("YLength");
        AddKey("ZLength");
    }
}
#endregion

#region IFCRectangularPyramid
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> XLength |
/// <see cref="float"/> YLength |
/// <see cref="float"/> Height 
/// 
/// <para/>Parent:
/// <see cref="IIFCCsgPrimitive3D"/>
/// 
/// </summary>
public class IFCRectangularPyramid : IIFCCsgPrimitive3D
{
    public IFCRectangularPyramid(IFCEntity e) : base(e)
    {
        AddKey("XLength");
        AddKey("YLength");
        AddKey("Height");
    }
}
#endregion

#region IFCRightCircularCone
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Height |
/// <see cref="float"/> BottomRadius 
/// 
/// <para/>Parent:
/// <see cref="IIFCCsgPrimitive3D"/>
/// 
/// </summary>
public class IFCRightCircularCone : IIFCCsgPrimitive3D
{
    public IFCRightCircularCone(IFCEntity e) : base(e)
    {
        AddKey("Height");
        AddKey("BottomRadius");
    }
}
#endregion

#region IFCRightCircularCylinder
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Height |
/// <see cref="float"/> Radius
/// 
/// <para/>Parent:
/// <see cref="IIFCCsgPrimitive3D"/>
/// 
/// </summary>
public class IFCRightCircularCylinder : IIFCCsgPrimitive3D
{
    public IFCRightCircularCylinder(IFCEntity e) : base(e)
    {
        AddKey("Height");
        AddKey("Radius");
    }
}
#endregion

#region IFCSphere
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Radius
/// 
/// <para/>Parent:
/// <see cref="IIFCCsgPrimitive3D"/>
/// 
/// </summary>
public class IFCSphere : IIFCCsgPrimitive3D
{
    public IFCSphere(IFCEntity e) : base(e)
    {
        AddKey("Radius");
    }
}
#endregion

#endregion

#region IIFCCurve
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IIFCBoundedCurve"/> |
/// <see cref="IIFCConic"/> |
/// <see cref="IFCLine"/> |
/// <see cref="IFCOffsetCurve2D"/> |
/// <see cref="IFCOffsetCurve3D"/> |
/// <see cref="IFCPcurve"/> |
/// 
/// </summary>
public abstract class IIFCCurve : IIFCGeometricRepresentationItem
{
    protected IIFCCurve(IFCEntity e) : base(e)
    {
    }
}

#region IIFCBoundedCurve
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCCurve"/>
/// 
/// <para/>Children:
/// <see cref="IIFCBSplineCurve"/> |
/// <see cref="IFCCompositeCurve"/> |
/// <see cref="IFCPolyline"/> |
/// <see cref="IFCTrimmedCurve"/>
/// 
/// </summary>
public abstract class IIFCBoundedCurve : IIFCCurve
{
    protected IIFCBoundedCurve(IFCEntity e) : base(e)
    {
    }
}

#region IFCBSplineCurve
/// <summary>
/// 
/// Properties:
/// <see cref="int"/> Degree |
/// <see cref="IFCCartesianPoint"/>[] ControlPointsList |
/// <see cref="IFCBSplineCurveForm"/> CurveForm |
/// <see cref="bool"/> ClosedCurve
/// <see cref="bool"/> SelfIntersect
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedCurve"/>
/// 
/// <para/>Children:
/// <see cref="IFCBSplineCurveWithKnots"/>
/// 
/// </summary>
public abstract class IIFCBSplineCurve : IIFCBoundedCurve
{
    protected IIFCBSplineCurve(IFCEntity e) : base(e)
    {
        AddKey("Degree");
        AddKey("ControlPointsList");
        AddKey("CurveForm");
        AddKey("ClosedCurve");
        AddKey("SelfIntersect");
    }
}

#region IFCBSplineCurveWithKnots
/// <summary>
/// 
/// Properties:
/// <see cref="int"/>[] KnotMultiplicities |
/// <see cref="float"/>[] Knots |
/// <see cref="IFCKnotType"/> KnotSpec 
/// 
/// <para/>Parent:
/// <see cref="IIFCBSplineCurve"/>
/// 
/// <para/>Children:
/// <see cref="IFCRationalBSplineCurveWithKnots"/>
/// 
/// </summary>
public class IFCBSplineCurveWithKnots : IIFCBSplineCurve
{
    public IFCBSplineCurveWithKnots(IFCEntity e) : base(e)
    {
        AddKey("KnotMultiplicities");
        AddKey("Knots");
        AddKey("KnotSpec");
    }
}

#region IFCRationalBSplineCurveWithKnots
/// <summary>
/// 
/// Properties:
/// <see cref="float"/>[] WeightsData
/// 
/// <para/>Parent:
/// <see cref="IFCBSplineCurveWithKnots"/>
/// 
/// </summary>
public class IFCRationalBSplineCurveWithKnots : IFCBSplineCurveWithKnots
{
    public IFCRationalBSplineCurveWithKnots(IFCEntity e) : base(e)
    {
        AddKey("WeightsData");
    }
}
#endregion

#endregion

#endregion

#region IFCCompositeCurve
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCompositeCurveSegment"/>[] Segments |
/// <see cref="bool"/> SelfIntersect
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedCurve"/>
/// 
/// <para/>Children:
/// <see cref="IFCCompositeCurveOnSurface"/>
/// 
/// </summary>
public class IFCCompositeCurve : IIFCBoundedCurve
{
    public IFCCompositeCurve(IFCEntity e) : base(e)
    {
        AddKey("Segments");
        AddKey("SelfIntersect");
    }    
}

#region IFCCompositeCurveOnSurface
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCCompositeCurve"/>
/// 
/// <para/>Children:
/// <see cref="IFCBoundaryCurve"/>
/// 
/// </summary>
public class IFCCompositeCurveOnSurface : IFCCompositeCurve
{
    public IFCCompositeCurveOnSurface(IFCEntity e) : base(e)
    {
    }
}

#region IFCBoundaryCurve
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCCompositeCurveOnSurface"/>
/// 
/// <para/>Children:
/// <see cref="IFCOuterBoundaryCurve"/>
/// 
/// </summary>
public class IFCBoundaryCurve : IFCCompositeCurveOnSurface
{
    public IFCBoundaryCurve(IFCEntity e) : base(e)
    {
    }
}

#region IFCOuterBoundaryCurve
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCBoundaryCurve"/>
/// 
/// </summary>
public class IFCOuterBoundaryCurve : IFCBoundaryCurve
{
    public IFCOuterBoundaryCurve(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#endregion

#endregion

#region IFCPolyline
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/>[] Points
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedCurve"/>
/// 
/// </summary>
public class IFCPolyline : IIFCBoundedCurve
{
    public IFCPolyline(IFCEntity e) : base(e)
    {
        AddKey("Points");
    }
}
#endregion

#region IFCTrimmedCurve
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> BasisCurve |
/// <see cref="IFCTrimmingSelect"/>[] Trim1 |
/// <see cref="IFCTrimmingSelect"/>[] Trim2 |
/// <see cref="bool"/> SenseAgreement |
/// <see cref="IFCTrimmingPreference"/> MasterRepresentation
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedCurve"/>
/// 
/// </summary>
public class IFCTrimmedCurve : IIFCBoundedCurve
{
    public IFCTrimmedCurve(IFCEntity e) : base(e)
    {
        AddKey("BasisCurve");
        AddKey("Trim1");
        AddKey("Trim2");
        AddKey("SenseAgreement");
        AddKey("MasterRepresentation");
    }
}
#endregion

#endregion

#region IIFCConic
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis2Placement"/> Position
/// 
/// <para/>Parent:
/// <see cref="IIFCCurve"/>
/// 
/// <para/>Children:
/// <see cref="IFCCircle"/> |
/// <see cref="IFCEllipse"/>
/// </summary>
public abstract class IIFCConic : IIFCCurve
{
    protected IIFCConic(IFCEntity e) : base(e)
    {
        AddKey("Position");
    }
}

#region IFCCircle
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Radius
/// 
/// <para/>Parent:
/// <see cref="IIFCConic"/>
/// 
/// </summary>
public class IFCCircle : IIFCConic
{
    public IFCCircle(IFCEntity e) : base(e)
    {
        AddKey("Radius");
    }
}
#endregion

#region IFCEllipse
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> SemiAxis1 |
/// <see cref="float"/> SemiAxis2
/// 
/// <para/>Parent:
/// <see cref="IIFCConic"/>
/// 
/// </summary>
public class IFCEllipse : IIFCConic
{
    public IFCEllipse(IFCEntity e) : base(e)
    {
        AddKey("SemiAxis1");
        AddKey("SemiAxis2");
    }
}
#endregion

#endregion

#region IFCLine
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/> Pnt |
/// <see cref="IFCVector"/> Dir
/// 
/// <para/>Parent:
/// <see cref="IIFCCurve"/>
/// 
/// </summary>
public class IFCLine : IIFCCurve
{
    public IFCLine(IFCEntity e) : base(e)
    {
        AddKey("Pnt");
        AddKey("Dir");
    }
}
#endregion

#region IFCOffsetCurve2D
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> BasisCurve |
/// <see cref="float"/> Distance |
/// <see cref="bool"/> SelfIntersect
/// 
/// <para/>Parent:
/// <see cref="IIFCCurve"/>
/// 
/// </summary>
public class IFCOffsetCurve2D : IIFCCurve
{
    public IFCOffsetCurve2D(IFCEntity e) : base(e)
    {
        AddKey("BasisCurve");
        AddKey("Distance");
        AddKey("SelfIntersect");
    }
}
#endregion

#region IFCOffsetCurve3D
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> BasisCurve |
/// <see cref="float"/> Distance |
/// <see cref="bool"/> SelfIntersect |
/// <see cref="IFCDirection"/> RefDirection
/// 
/// <para/>Parent:
/// <see cref="IIFCCurve"/>
/// 
/// </summary>
public class IFCOffsetCurve3D : IIFCCurve
{
    public IFCOffsetCurve3D(IFCEntity e) : base(e)
    {
        AddKey("BasisCurve");
        AddKey("Distance");
        AddKey("SelfIntersect");
        AddKey("RefDirection");
    }
}
#endregion

#region IFCPcurve
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCSurface"/> BasisSurface |
/// <see cref="IIFCCurve"/> ReferenceCurve
/// 
/// <para/>Parent:
/// <see cref="IIFCCurve"/>
/// 
/// </summary>
public class IFCPcurve : IIFCCurve
{
    public IFCPcurve(IFCEntity e) : base(e)
    {
        AddKey("BasisSurface");
        AddKey("ReferenceCurve");
    }
}
#endregion

#endregion

#region IFCDirection
/// <summary>
/// 
/// Properties:
/// <see cref="float"/>[] DirectionRatios
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCDirection : IIFCGeometricRepresentationItem
{
    public IFCDirection(IFCEntity e) : base(e)
    {
        AddKey("DirectionRatios");
    }
}
#endregion

#region IFCFaceBasedSurfaceModel
/// <summary>
/// 
/// Properties:
/// <see cref="IFCConnectedFaceSet"/>[] FbsmFaces
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCFaceBasedSurfaceModel : IIFCGeometricRepresentationItem
{
    public IFCFaceBasedSurfaceModel(IFCEntity e) : base(e)
    {
        AddKey("FbsmFaces");
    }
}
#endregion

#region IFCFillAreaStyleHatching
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCurveStyle"/> HatchLineAppearance |
/// <see cref="IFCHatchLineDistanceSelect"/> StartOfNextHatchLine |
/// <see cref="IFCCartesianPoint"/> PointOfReferenceHatchLine |
/// <see cref="IFCCartesianPoint"/> PatternStart |
/// <see cref="IFCPlaneAngleMeasure"/> HatchLineAngle
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCFillAreaStyleHatching : IIFCGeometricRepresentationItem
{
    public IFCFillAreaStyleHatching(IFCEntity e) : base(e)
    {
        AddKey("HatchLineAppearance");
        AddKey("StartOfNextHatchLine");
        AddKey("PointOfReferenceHatchLine");
        AddKey("PatternStart");
        AddKey("HatchLineAngle");
    }
}
#endregion

#region IFCFillAreaStyleTiles
/// <summary>
/// 
/// Properties:
/// <see cref="IFCVector"/>[] TilingPattern |
/// <see cref="IFCStyledItem"/>[] Tiles |
/// <see cref="float"/> TilingScale
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCFillAreaStyleTiles : IIFCGeometricRepresentationItem
{
    public IFCFillAreaStyleTiles(IFCEntity e) : base(e)
    {
        AddKey("TilingPattern");
        AddKey("Tiles");
        AddKey("TilingScale");
    }
}
#endregion

#region IFCGeometricSet
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCGEometricSetSelect"/>[] Elements
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCGeometricCurveSet"/>
/// 
/// </summary>
public class IFCGeometricSet : IIFCGeometricRepresentationItem
{
    public IFCGeometricSet(IFCEntity e) : base(e)
    {
        AddKey("Elements");
    }
}

#region IFCGeometricCurveSet
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCGeometricSet"/>
/// 
/// </summary>
public class IFCGeometricCurveSet : IFCGeometricSet
{
    public IFCGeometricCurveSet(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IFCHalfSpaceSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCSurface"/> BaseSurface |
/// <see cref="bool"/> AgreementFlag
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCBoxedHalfSpace"/> |
/// <see cref="IFCPolygonalBoundedHalfSpace"/>
/// 
/// </summary>
public class IFCHalfSpaceSolid : IIFCGeometricRepresentationItem
{
    public IFCHalfSpaceSolid(IFCEntity e) : base(e)
    {
        AddKey("BaseSurface");
        AddKey("AgreementFlag");
    }
}

#region IFCBoxedHalfSpace
/// <summary>
/// 
/// Properties:
/// <see cref="IFCBoundingBox"/> Enclosure
/// 
/// <para/>Parent:
/// <see cref="IFCHalfSpaceSolid"/>
/// 
/// </summary>
public class IFCBoxedHalfSpace : IFCHalfSpaceSolid
{
    public IFCBoxedHalfSpace(IFCEntity e) : base(e)
    {
        AddKey("Enclosure");
    }
}
#endregion

#region IFCPolygonalBoundedHalfSpace
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis2Placement3D"/> Position |
/// <see cref="IIFCBoundedCurve"/> PolygonalBoundary
/// 
/// <para/>Parent:
/// <see cref="IFCHalfSpaceSolid"/>
/// 
/// </summary>
public class IFCPolygonalBoundedHalfSpace : IFCHalfSpaceSolid
{
    public IFCPolygonalBoundedHalfSpace(IFCEntity e) : base(e)
    {
        AddKey("Position");
        AddKey("PolygonalBoundary");
    }
}
#endregion

#endregion

#region IIFCLightSource
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Name |
/// <see cref="IFCColourRgb"/> LightColour |
/// <see cref="float"/> AmbientIntensity |
/// <see cref="float"/> Intensity
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCLightSourceAmbient"/> |
/// <see cref="IFCLightSourceDirectional"/> |
/// <see cref="IFCLightSourceGoniometric"/> |
/// <see cref="IFCLightSourcePositional"/> 
/// 
/// </summary>
public abstract class IIFCLightSource : IIFCGeometricRepresentationItem
{
    protected IIFCLightSource(IFCEntity e) : base(e)
    {
        AddKey("Name");
        AddKey("LightColour");
        AddKey("AmbientIntensity");
        AddKey("Intensity");
    }
}

#region IFCLightSourceAmbient
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCLightSource"/>
/// 
/// </summary>
public class IFCLightSourceAmbient : IIFCLightSource
{
    public IFCLightSourceAmbient(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCLightSourceDirectional
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Orientation
/// 
/// <para/>Parent:
/// <see cref="IIFCLightSource"/>
/// 
/// </summary>
public class IFCLightSourceDirectional : IIFCLightSource
{
    public IFCLightSourceDirectional(IFCEntity e) : base(e)
    {
        AddKey("Orientation");
    }
}
#endregion

#region IFCLightSourceGoniometric
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis2Placement3D"/> Position |
/// <see cref="IFCColourRgb"/> ColourAppearance |
/// <see cref="float"/> ColourTemperature |
/// <see cref="float"/> LuminousFlux |
/// <see cref="IFCLightEmissionSourceEnum"/> LightEmissionSource |
/// <see cref="IFCLightDistributionDataSourceSelect"/> LightDistributionDataSource
/// 
/// <para/>Parent:
/// <see cref="IIFCLightSource"/>
/// 
/// </summary>
public class IFCLightSourceGoniometric : IIFCLightSource
{
    public IFCLightSourceGoniometric(IFCEntity e) : base(e)
    {
        AddKey("Position");
        AddKey("ColourAppearance");
        AddKey("ColourTemperature");
        AddKey("LuminousFlux");
        AddKey("LightEmissionSource");
        AddKey("LightDistributionDataSource");
    }
}
#endregion

#region IFCLightSourcePositional
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/> Position |
/// <see cref="float"/> Radius |
/// <see cref="float"/> ConstantAttenuation |
/// <see cref="float"/> DistanceAttenuation |
/// <see cref="float"/> QuadricAttenuation
/// 
/// <para/>Parent:
/// <see cref="IIFCLightSource"/>
/// 
/// <para/>Children:
/// <see cref="IFCLightSourceSpot"/>
/// 
/// </summary>
public class IFCLightSourcePositional : IIFCLightSource
{
    public IFCLightSourcePositional(IFCEntity e) : base(e)
    {
        AddKey("Position");
        AddKey("Radius");
        AddKey("ConstantAttenuation");
        AddKey("DistanceAttenuation");
        AddKey("QuadricAttenuation");
    }
}

#region IFCLightSourceSpot
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Orientation |
/// <see cref="float"/> ConcentrationExponent |
/// <see cref="float"/> SpreadAngle |
/// <see cref="float"/> BeamWidthAngle
/// 
/// <para/>Parent:
/// <see cref="IFCLightSourcePositional"/>
/// 
/// </summary>
public class IFCLightSourceSpot : IFCLightSourcePositional
{
    public IFCLightSourceSpot(IFCEntity e) : base(e)
    {
        AddKey("Orientation");
        AddKey("ConcentrationExponent");
        AddKey("SpreadAngle");
        AddKey("BeamWidthAngle");
    }
}
#endregion

#endregion

#endregion

#region IIFCPlacement
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/> Location
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCAxis1Placement"/> |
/// <see cref="IFCAxis2Placement2D"/> |
/// <see cref="IFCAxis2Placement3D"/>
/// 
/// </summary>
public abstract class IIFCPlacement : IIFCGeometricRepresentationItem
{
    protected IIFCPlacement(IFCEntity e) : base(e)
    {
        AddKey("Location");
    }
}

#region IFCAxis1Placement
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Axis
/// 
/// <para/>Parent:
/// <see cref="IIFCPlacement"/>
/// 
/// </summary>
public class IFCAxis1Placement : IIFCPlacement
{
    public IFCAxis1Placement(IFCEntity e) : base(e)
    {
        AddKey("Axis");
    }
}
#endregion

#region IIFCAxis2Placement
/// <summary>
/// 
/// Select:
/// <see cref="IFCAxis2Placement2D"/> |
/// <see cref="IFCAxis2Placement3D"/>
/// 
/// </summary>
public interface IIFCAxis2Placement
{
}
#endregion

#region IFCAxis2Placement2D
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> RefDirection
/// 
/// <para/>Parent:
/// <see cref="IIFCPlacement"/>
/// 
/// </summary>
public class IFCAxis2Placement2D : IIFCPlacement, IIFCAxis2Placement
{
    public IFCAxis2Placement2D(IFCEntity e) : base(e)
    {
        AddKey("RefDirection");
    }
}
#endregion

#region IFCAxis2Placement3D
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Axis |
/// <see cref="IFCDirection"/> RefDirection 
/// 
/// <para/>Parent:
/// <see cref="IIFCPlacement"/>
/// 
/// </summary>
public class IFCAxis2Placement3D : IIFCPlacement, IIFCAxis2Placement
{
    public IFCAxis2Placement3D(IFCEntity e) : base(e)
    {
        AddKey("Axis");
        AddKey("RefDirection");
    }
}
#endregion

#endregion

#region IFCPlanarExtent
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> SizeInX |
/// <see cref="float"/> SizeInY
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCPlanarBox"/>
/// 
/// </summary>
public class IFCPlanarExtent : IIFCGeometricRepresentationItem
{
    public IFCPlanarExtent(IFCEntity e) : base(e)
    {
        AddKey("SizeInX");
        AddKey("SizeInY");
    }
}

#region IFCPlanarBox
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCAxis2Placement"/> Placement
/// 
/// <para/>Parent:
/// <see cref="IFCPlanarExtent"/>
/// </summary>
public class IFCPlanarBox : IFCPlanarExtent
{
    public IFCPlanarBox(IFCEntity e) : base(e)
    {
        AddKey("Placement");
    }
}
#endregion

#endregion

#region IFCPoint
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCCartesianPoint"/> |
/// <see cref="IFCPointOnCurve"/> |
/// <see cref="IFCPointOnSurface"/>
/// 
/// </summary>
public abstract class IIFCPoint : IIFCGeometricRepresentationItem
{
    protected IIFCPoint(IFCEntity e) : base(e)
    {
    }
}

#region IFCCartesianPoint
/// <summary>
/// 
/// Properties:
/// <see cref="float"/>[] Coordinates
/// 
/// <para/>Parent:
/// <see cref="IIFCPoint"/>
/// 
/// </summary>
public class IFCCartesianPoint : IIFCPoint
{
    public IFCCartesianPoint(IFCEntity e) : base(e)
    {
        AddKey("Coordinates");
    }
}
#endregion

#region IFCPointOnCurve
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> BasisCurve |
/// <see cref="float"/> ParameterValue
/// 
/// <para/>Parent:
/// <see cref="IIFCPoint"/>
/// 
/// </summary>
public class IFCPointOnCurve : IIFCPoint
{
    public IFCPointOnCurve(IFCEntity e) : base(e)
    {
        AddKey("BasisCurve");
        AddKey("PointParameter");
    }
}
#endregion

#region IFCPointOnSurface
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCSurface"/> BasisSurface |
/// <see cref="float"/> PointParameterU |
/// <see cref="float"/> PointParameterV
/// 
/// <para/>Parent:
/// <see cref="IIFCPoint"/>
/// 
/// </summary>
public class IFCPointOnSurface : IIFCPoint
{
    public IFCPointOnSurface(IFCEntity e) : base(e)
    {
        AddKey("BasisSurface");
        AddKey("PointParameterU");
        AddKey("PointParameterV");
    }
}
#endregion

#endregion

#region IFCSectionedSpine
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCompositeCurve"/> SpineCurve |
/// <see cref="IFCProfileDef"/>[] CrossSections |
/// <see cref="IFCAxis2Placement3D"/>[] CrossSectionPositions
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCSectionedSpine : IIFCGeometricRepresentationItem
{
    public IFCSectionedSpine(IFCEntity e) : base(e)
    {
        AddKey("SpineCurve");
        AddKey("CrossSections");
        AddKey("CrossSectionPositions");
    }
}
#endregion

#region IFCShellBasedSurfaceModel
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCShell"/>[] SbsmBoundary
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCShellBasedSurfaceModel : IIFCGeometricRepresentationItem
{
    public IFCShellBasedSurfaceModel(IFCEntity e) : base(e)
    {
        AddKey("SbsmBoundary");
    }
}
#endregion

#region IIFCSolidModel
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCCsgSolid"/> |
/// <see cref="IIFCManifoldSolidBrep"/> |
/// <see cref="IIFCSweptAreaSolid"/> |
/// <see cref="IFCSweptDiskSolid"/> 
/// 
/// </summary>
public abstract class IIFCSolidModel : IIFCGeometricRepresentationItem
{
    protected IIFCSolidModel(IFCEntity e) : base(e)
    {
    }
}

#region IFCCsgSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCsgSelect"/> TreeRootExpression
/// 
/// <para/>Parent:
/// <see cref="IIFCSolidModel"/>
/// 
/// </summary>
public class IFCCsgSolid : IIFCSolidModel
{
    public IFCCsgSolid(IFCEntity e) : base(e)
    {
        AddKey("TreeRootExpression");
    }
}
#endregion

#region IIFCManifoldSolidBrep
/// <summary>
/// 
/// Properties:
/// <see cref="IFCClosedShell"/> Outer
/// 
/// <para/>Parent:
/// <see cref="IIFCSolidModel"/>
/// 
/// <para/>Children:
/// <see cref="IFCAdvancedBrep"/> |
/// <see cref="IFCFacetedBrep"/> 
/// 
/// </summary>
public abstract class IIFCManifoldSolidBrep : IIFCSolidModel
{
    public IIFCManifoldSolidBrep(IFCEntity e) : base(e)
    {
        AddKey("Outer");
    }
}

#region IFCAdvancedBrep
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCManifoldSolidBrep"/>
/// 
/// <para/>Children:
/// <see cref="IFCAdvancedBrepWithVoids"/>
/// 
/// </summary>
public class IFCAdvancedBrep : IIFCManifoldSolidBrep
{
    public IFCAdvancedBrep(IFCEntity e) : base(e)
    {
    }
}

#region IFCAdvancedBrepWithVoids
/// <summary>
/// 
/// Properties:
/// <see cref="IFCClosedShell"/>[] Voids
/// 
/// <para/>Parent:
/// <see cref="IFCAdvancedBrep"/>
/// 
/// </summary>
public class IFCAdvancedBrepWithVoids : IFCAdvancedBrep
{
    public IFCAdvancedBrepWithVoids(IFCEntity e) : base(e)
    {
        AddKey("Voids");
    }
}
#endregion

#endregion

#region IFCFacetedBrep
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCManifoldSolidBrep"/>
/// 
/// <para/>Children:
/// <see cref="IFCFacetedBrepWithVoids"/>
/// 
/// </summary>
public class IFCFacetedBrep : IIFCManifoldSolidBrep
{
    public IFCFacetedBrep(IFCEntity e) : base(e)
    {
    }
}

#region IFCFacetedBrepWithVoids
/// <summary>
/// 
/// Properties:
/// <see cref="IFCClosedShell"/>[] Voids
/// 
/// <para/>Parent:
/// <see cref="IFCFacetedBrep"/>
/// 
/// </summary>
public class IFCFacetedBrepWithVoids : IFCFacetedBrep
{
    public IFCFacetedBrepWithVoids(IFCEntity e) : base(e)
    {
        AddKey("Voids");
    }
}
#endregion

#endregion

#endregion

#region IIFCSweptAreaSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IFCProfileDef"/> SweptArea |
/// <see cref="IFCAxis2Placement3D"/> Position
/// 
/// <para/>Parent:
/// <see cref="IIFCSolidModel"/>
/// 
/// <para/>Children:
/// <see cref="IFCExtrudedAreaSolid"/> |
/// <see cref="IFCFixedReferenceSweptAreaSolid"/> |
/// <see cref="IFCRevolvedAreaSolid"/> |
/// <see cref="IFCSurfaceCurveSweptAreaSolid"/>
/// </summary>
public abstract class IIFCSweptAreaSolid : IIFCSolidModel
{
    protected IIFCSweptAreaSolid(IFCEntity e) : base(e)
    {
        AddKey("SweptArea");
        AddKey("Position");
    }
}

#region IFCExtrudedAreaSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> ExtrudedDirection |
/// <see cref="float"/> Depth
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptAreaSolid"/>
/// 
/// <para/>Children:
/// <see cref="IFCExtrudedAreaSolidTapered"/>
/// 
/// </summary>
public class IFCExtrudedAreaSolid : IIFCSweptAreaSolid
{
    public IFCExtrudedAreaSolid(IFCEntity e) : base(e)
    {
        AddKey("ExtrudedDirection");
        AddKey("Depth");
    }
}

#region IFCExtrudedAreaSolidTapered
/// <summary>
/// 
/// Properties:
/// <see cref="IFCProfileDef"/> EndSweptArea
/// 
/// <para/>Parent:
/// <see cref="IFCExtrudedAreaSolid"/>
/// 
/// </summary>
public class IFCExtrudedAreaSolidTapered : IFCExtrudedAreaSolid
{
    public IFCExtrudedAreaSolidTapered(IFCEntity e) : base(e)
    {
        AddKey("EndSweptArea");
    }
}
#endregion

#endregion

#region IFCFixedReferenceSweptAreaSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> Directrix |
/// <see cref="float"/> StartParam |
/// <see cref="float"/> EndParam |
/// <see cref="IFCDirection"/> FixedReference
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptAreaSolid"/>
/// 
/// </summary>
public class IFCFixedReferenceSweptAreaSolid : IIFCSweptAreaSolid
{
    public IFCFixedReferenceSweptAreaSolid(IFCEntity e) : base(e)
    {
        AddKey("Directrix");
        AddKey("StartParam");
        AddKey("EndParam");
        AddKey("FixedReference");
    }
}
#endregion

#region IFCRevolvedAreaSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis1Placement"/> Axis |
/// <see cref="float"/> Angle
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptAreaSolid"/>
/// 
/// <para/>Children:
/// <see cref="IFCRevolvedAreaSolidTapered"/>
/// 
/// </summary>
public class IFCRevolvedAreaSolid : IIFCSweptAreaSolid
{
    public IFCRevolvedAreaSolid(IFCEntity e) : base(e)
    {
        AddKey("Axis");
        AddKey("Angle");
    }
}

#region IFCRevolvedAreaSolidTapered
/// <summary>
/// 
/// Properties:
/// <see cref="IFCProfileDef"/> EndSweptArea
/// 
/// <para/>Parent:
/// <see cref="IFCRevolvedAreaSolid"/>
/// 
/// </summary>
public class IFCRevolvedAreaSolidTapered : IFCRevolvedAreaSolid
{
    public IFCRevolvedAreaSolidTapered(IFCEntity e) : base(e)
    {
        AddKey("EndSweptArea");
    }
}
#endregion

#endregion

#region IFCSurfaceCurveSweptAreaSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> Directrix |
/// <see cref="float"/> StartParam |
/// <see cref="float"/> EndParam |
/// <see cref="IIFCSurface"/> ReferenceSurface
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptAreaSolid"/>
/// 
/// </summary>
public class IFCSurfaceCurveSweptAreaSolid : IIFCSweptAreaSolid
{
    public IFCSurfaceCurveSweptAreaSolid(IFCEntity e) : base(e)
    {
        AddKey("Directrix");
        AddKey("StartParam");
        AddKey("EndParam");
        AddKey("ReferenceSurface");
    }
}
#endregion

#endregion

#region IFCSweptDiskSolid
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCCurve"/> Directrix |
/// <see cref="float"/> Radius |
/// <see cref="float"/> InnerRadius |
/// <see cref="float"/> StartParam |
/// <see cref="float"/> EndParam
/// 
/// <para/>Parent:
/// <see cref="IIFCSolidModel"/>
/// 
/// <para/>Children:
/// <see cref="IFCSweptDiskSolidPolygonal"/>
/// 
/// </summary>
public class IFCSweptDiskSolid : IIFCSolidModel
{
    public IFCSweptDiskSolid(IFCEntity e) : base(e)
    {
        AddKey("Directrix");
        AddKey("Radius");
        AddKey("InnerRadius");
        AddKey("StartParam");
        AddKey("EndParam");
    }
}

#region IFCSweptDiskSolidPolygonal
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> FilletRadius
/// 
/// <para/>Parent:
/// <see cref="IFCSweptDiskSolid"/>
/// 
/// </summary>
public class IFCSweptDiskSolidPolygonal : IFCSweptDiskSolid
{
    public IFCSweptDiskSolidPolygonal(IFCEntity e) : base(e)
    {
        AddKey("FilletRadius");
    }
}
#endregion

#endregion

#endregion

#region IIFCSurface
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IIFCBoundedSurface"/> | 
/// <see cref="IIFCElementarySurface"/> |
/// <see cref="IIFCSweptSurface"/>
///  
/// </summary>
public abstract class IIFCSurface : IIFCGeometricRepresentationItem
{
    protected IIFCSurface(IFCEntity e) : base(e)
    {
    }
}

#region IFCBoundedSurface
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCSurface"/>
/// 
/// <para/>Children:
/// <see cref="IIFCBSplineSurface"/> |
/// <see cref="IFCCurvedBoundedPlane"/> |
/// <see cref="IFCCurveBoundedSurface"/> |
/// <see cref="IFCRectangularTrimmedSurface"/>
/// 
/// </summary>
public abstract class IIFCBoundedSurface : IIFCSurface
{
    protected IIFCBoundedSurface(IFCEntity e) : base(e)
    {
    }
}

#region IIFCBSplineSurface
/// <summary>
/// 
/// Properties:
/// <see cref="int"/> UDegree |
/// <see cref="int"/> VDegree |
/// <see cref="IFCCartesianPoint"/>[][] ControlPointsList |
/// <see cref="IFCBSplineSurfaceForm"/> SurfaceForm |
/// <see cref="bool"/> UClosed |
/// <see cref="bool"/> VClosed |
/// <see cref="bool"/> SelfIntersect
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedSurface"/>
/// 
/// <para/>Children:
/// <see cref="IFCBSplineSurfaceWithKnots"/>
/// 
/// </summary>
public abstract class IIFCBSplineSurface : IIFCBoundedSurface
{
    protected IIFCBSplineSurface(IFCEntity e) : base(e)
    {
        AddKey("UDegree");
        AddKey("VDegree");
        AddKey("ControlPointsList");
        AddKey("SurfaceForm");
        AddKey("UClosed");
        AddKey("VClosed");
        AddKey("SelfIntersect");
    }
}

#region IFCBSplineSurfaceWithKnots
/// <summary>
/// 
/// Properties:
/// <see cref="int"/>[] UMultiplicities |
/// <see cref="int"/>[] VMultiplicities |
/// <see cref="float"/>[] UKnots |
/// <see cref="float"/>[] VKnots |
/// <see cref="IFCKnotType"/> KnotSpec
/// 
/// <para/>Parent:
/// <see cref="IIFCBSplineSurface"/>
/// 
/// <para/>Children:
/// <see cref="IFCrationalBSplineSurfaceWithKnots"/>
/// 
/// </summary>
public class IFCBSplineSurfaceWithKnots : IIFCBSplineSurface
{
    public IFCBSplineSurfaceWithKnots(IFCEntity e) : base(e)
    {
        AddKey("UMultiplicities");
        AddKey("VMultiplicities");
        AddKey("UKnots");
        AddKey("VKnots");
        AddKey("KnotSpec");
    }
}

#region IFCrationalBSplineSurfaceWithKnots
/// <summary>
/// 
/// Properties:
/// <see cref="float"/>[][] WeightsData
/// 
/// <para/>Parent:
/// <see cref="IFCBSplineSurfaceWithKnots"/>
/// 
/// </summary>
public class IFCrationalBSplineSurfaceWithKnots : IFCBSplineSurfaceWithKnots
{
    public IFCrationalBSplineSurfaceWithKnots(IFCEntity e) : base(e)
    {
        AddKey("WeightsData");
    }
}
#endregion

#endregion

#endregion

#region IFCCurvedBoundedPlane
/// <summary>
/// 
/// Properties:
/// <see cref="IFCPlane"/> BasisSurface |
/// <see cref="IIFCCurve"/> OuterBoundary |
/// <see cref="IIFCCurve"/>[] InnerBoundaries
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedSurface"/>
/// 
/// </summary>
public class IFCCurvedBoundedPlane : IIFCBoundedSurface
{
    public IFCCurvedBoundedPlane(IFCEntity e) : base(e)
    {
        AddKey("BasisSurface");
        AddKey("OuterBoundary");
        AddKey("InnerBoundaries");
    }
}
#endregion

#region IFCCurveBoundedSurface
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCSurface"/> BasisSurface |
/// <see cref="IFCBoundaryCurve"/>[] Boundaries |
/// <see cref="bool"/> ImplicitOuter
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedSurface"/>
/// 
/// </summary>
public class IFCCurveBoundedSurface : IIFCBoundedSurface
{
    public IFCCurveBoundedSurface(IFCEntity e) : base(e)
    {
        AddKey("BasisSurface");
        AddKey("Boundaries");
        AddKey("ImplicitOuter");
    }
}
#endregion

#region IFCRectangularTrimmedSurface
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCSurface"/> BasisSurface |
/// <see cref="float"/> U1 |
/// <see cref="float"/> V1 |
/// <see cref="float"/> U2 |
/// <see cref="float"/> V2 |
/// <see cref="bool"/> Usense |
/// <see cref="bool"/> Vsense
/// 
/// <para/>Parent:
/// <see cref="IIFCBoundedSurface"/>
/// 
/// </summary>
public class IFCRectangularTrimmedSurface : IIFCBoundedSurface
{
    public IFCRectangularTrimmedSurface(IFCEntity e) : base(e)
    {
        AddKey("BasisSurface");
        AddKey("U1");
        AddKey("V1");
        AddKey("U2");
        AddKey("V2");
        AddKey("Usense");
        AddKey("Vsense");
    }
}
#endregion

#endregion

#region IIFCElementarySurface
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis2Placement3D"/> Position
/// 
/// <para/>Parent:
/// <see cref="IIFCSurface"/>
/// 
/// <para/>Children:
/// <see cref="IFCCylindricalSurface"/> |
/// <see cref="IFCPlane"/>
/// 
/// </summary>
public abstract class IIFCElementarySurface : IIFCSurface
{
    protected IIFCElementarySurface(IFCEntity e) : base(e)
    {
        AddKey("Position");
    }
}

#region IFCCylindricalSurface
/// <summary>
/// 
/// Properties:
/// <see cref="float"/> Radius
/// 
/// <para/>Parent:
/// <see cref="IIFCElementarySurface"/>
/// 
/// </summary>
public class IFCCylindricalSurface : IIFCElementarySurface
{
    public IFCCylindricalSurface(IFCEntity e) : base(e)
    {
        AddKey("Radius");
    }
}
#endregion

#region IFCPlane 
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCElementarySurface"/>
/// 
/// </summary>
public class IFCPlane : IIFCElementarySurface
{
    public IFCPlane(IFCEntity e) : base(e)
    {
    }   
}
#endregion

#endregion

#region IIFCSweptSurface
/// <summary>
/// 
/// Properties:
/// <see cref="IFCProfileDef"/> SweptCurve |
/// <see cref="IFCAxis2Placement3D"/> Position
/// 
/// <para/>Parent:
/// <see cref="IIFCSurface"/>
/// 
/// <para/>Children:
/// <see cref="IFCSurfaceOfLinearExtrusion"/> |
/// <see cref="IFCSurfaceOfRevolution"/>
/// 
/// </summary>
public abstract class IIFCSweptSurface : IIFCSurface
{
    protected IIFCSweptSurface(IFCEntity e) : base(e)
    {
        AddKey("SweptCurve");
        AddKey("Position");
    }
}

#region IFCSurfaceOfLinearExtrusion
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> ExtrudedDirection |
/// <see cref="float"/> Depth
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptSurface"/>
/// 
/// </summary>
public class IFCSurfaceOfLinearExtrusion : IIFCSweptSurface
{
    public IFCSurfaceOfLinearExtrusion(IFCEntity e) : base(e)
    {
        AddKey("ExtrudedDirection");
        AddKey("Depth");
    }
}
#endregion

#region IFCSurfaceOfRevolution
/// <summary>
/// 
/// Properties:
/// <see cref="IFCAxis1Placement"/> AxisPosition
/// 
/// <para/>Parent:
/// <see cref="IIFCSweptSurface"/>
/// 
/// </summary>
public class IFCSurfaceOfRevolution : IIFCSweptSurface
{
    public IFCSurfaceOfRevolution(IFCEntity e) : base(e)
    {
        AddKey("AxisPosition");
    }
}
#endregion

#endregion

#endregion

#region IIFCTessellatedItem
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IIFCTessellatedFaceSet"/>
/// 
/// </summary>
public abstract class IIFCTessellatedItem : IIFCGeometricRepresentationItem
{
    protected IIFCTessellatedItem(IFCEntity e) : base(e)
    {
    }
}

#region IIFCTessellatedFaceSet
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPointList3D"/> Coordinates |
/// <see cref="float"/>[][] Normals |
/// <see cref="bool"/> Closed
/// 
/// <para/>Parent:
/// <see cref="IIFCTessellatedItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCTriangulatedFaceSet"/>
/// 
/// </summary>
public abstract class IIFCTessellatedFaceSet : IIFCTessellatedItem
{
    protected IIFCTessellatedFaceSet(IFCEntity e) : base(e)
    {
        AddKey("Coordinates");
        AddKey("Normals");
        AddKey("Closed");
    }
}

#region IFCTriangulatedFaceSet
/// <summary>
/// 
/// Properties:
/// <see cref="int"/>[][] CoordIndex
/// <see cref="int"/>[][] NormalIndex
/// 
/// <para/>Parent:
/// <see cref="IIFCTessellatedFaceSet"/>
/// 
/// </summary>
public class IFCTriangulatedFaceSet : IIFCTessellatedFaceSet
{
    public IFCTriangulatedFaceSet(IFCEntity e) : base(e)
    {
        AddKey("CoordIndex");
        AddKey("NormalIndex");
    }
}
#endregion

#endregion

#endregion

#region IFCTextLiteral
/// <summary>
/// 
/// Properties:
/// <see cref="string"/> Literal |
/// <see cref="IIFCAxis2Placement"/> Placement |
/// <see cref="IFCTextPath"/> Path
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCTextLiteralWithExtent"/>
/// 
/// </summary>
public class IFCTextLiteral : IIFCGeometricRepresentationItem
{
    public IFCTextLiteral(IFCEntity e) : base(e)
    {
        AddKey("Literal");
        AddKey("Placement");
        AddKey("Path");
    }
}

#region IFCTextLiteralWithExtent
/// <summary>
/// 
/// Properties:
/// <see cref="IFCPlanarExtent"/> Extent |
/// <see cref="string"/> BoxAlignment 
/// 
/// <para/>Parent:
/// <see cref="IFCTextLiteral"/>
/// 
/// </summary>
public class IFCTextLiteralWithExtent : IFCTextLiteral
{
    public IFCTextLiteralWithExtent(IFCEntity e) : base(e)
    {
        AddKey("Extent");
        AddKey("BoxAlignment");
    }
}
#endregion

#endregion

#region IFCVector
/// <summary>
/// 
/// Properties:
/// <see cref="IFCDirection"/> Orientation |
/// <see cref="float"/> Magnitude
/// 
/// <para/>Parent:
/// <see cref="IIFCGeometricRepresentationItem"/>
/// 
/// </summary>
public class IFCVector : IIFCGeometricRepresentationItem
{
    public IFCVector(IFCEntity e) : base(e)
    {
        AddKey("Orientation");
        AddKey("Magnitude");
    }
}
#endregion

#endregion

#region IFCMappedItem
/// <summary>
/// 
/// Properties:
/// <see cref="IFCRepresentationMap"/> MappingSource |
/// <see cref="IIFCCartesianTransformationOperator"/> MappingTarget
/// 
/// <para/>Parent:
/// <see cref="IIFCRepresentationItem"/>
/// 
/// </summary>
public class IFCMappedItem : IIFCRepresentationItem
{
    public IFCMappedItem(IFCEntity e) : base(e)
    {
        AddKey("MappingSource");
        AddKey("MappingTarget");
    }
}
#endregion

#region IFCRepresentationMap
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCAxis2Placement"/> MappingOrigin |
/// <see cref="IIFCRepresentation"/> MappedRepresentation 
/// 
/// <para/>Parent:
/// <see cref="IFCEntity"/>
/// 
/// </summary>
public class IFCRepresentationMap : IFCEntity
{
    public IFCRepresentationMap(IFCEntity e) : base(e)
    {
        AddKey("MappingOrigin");
        AddKey("MappedRepresentation");
    } 
}
#endregion

#region IFCStyledItem
/// <summary>
/// TODO
/// <para/>Parent:
/// <see cref="IIFCRepresentationItem"/>
/// </summary>
public class IFCStyledItem : IIFCRepresentationItem
{
    public IFCStyledItem(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IIFCTopologicalRepresentationItem
/// <summary>
/// 
/// <para/>Parent 
/// <see cref="IIFCRepresentationItem"/>
/// 
/// <para/>Children: 
/// <see cref="IFCConnectedFaceSet"/> |
/// <see cref="IFCEdge"/> |
/// <see cref="IFCFace"/> |
/// <see cref="IFCFaceBound"/> |
/// <see cref="IFCLoop"/> |
/// <see cref="IFCPath"/> |
/// <see cref="IFCVertex"/>
/// 
/// </summary>
public abstract class IIFCTopologicalRepresentationItem : IIFCRepresentationItem
{
    protected IIFCTopologicalRepresentationItem(IFCEntity e) : base(e)
    {
    }
}

#region IFCConnectedFaceSet
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCFace"/>[] CfsFaces
/// 
/// <para/>Parent:
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children: 
/// <see cref="IFCClosedShell "/> | 
/// <see cref="IFCOpenShell "/>
/// 
/// </summary>
public class IFCConnectedFaceSet : IIFCTopologicalRepresentationItem
{
    public IFCConnectedFaceSet(IFCEntity e) : base(e)
    {
        AddKey("CfsFaces");
    }
}

#region IIFCShell
/// <summary>
/// 
/// Select:
/// <see cref="IFCClosedShell"/> |
/// <see cref="IFCOpenShell"/>
/// 
/// </summary>
public interface IIFCShell
{
}
#endregion

#region IFCClosedShell
/// <summary>
/// 
/// <para/>Parent:
/// <see cref="IFCConnectedFaceSet"/>
/// 
/// </summary>
public class IFCClosedShell : IFCConnectedFaceSet, IIFCShell
{
    public IFCClosedShell(IFCEntity e) : base(e)
    {
    }
}
#endregion

#region IFCOpenShell
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCConnectedFaceSet"/>
/// 
/// </summary>
public class IFCOpenShell : IFCConnectedFaceSet, IIFCShell
{
    public IFCOpenShell(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IFCEdge
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCVertex"/> EdgeStart |
/// <see cref="IFCVertex"/> EdgeEnd
/// 
/// <para/>Parent:
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children: 
/// <see cref="IFCEdgeCurve"/> | 
/// <see cref="IFCOrientedEdge"/> | 
/// <see cref="IFCSubedge"/>
/// 
/// </summary>
public class IFCEdge : IIFCTopologicalRepresentationItem
{
    public IFCEdge(IFCEntity e) : base(e)
    {
        AddKey("EdgeStart");
        AddKey("EdgeEnd");
    }
}

#region IFCEdgeCurve
/// <summary>
/// 
/// Properties: 
/// <see cref="IIFCCurve"/> EdgeGeometry |
/// <see cref="bool"/> SameSense
/// 
/// <para/>Parent:
/// <see cref="IFCEdge"/>
/// 
/// </summary>
public class IFCEdgeCurve : IFCEdge
{
    public IFCEdgeCurve(IFCEntity e) : base(e)
    {
        AddKey("EdgeGeometry");
        AddKey("SameSense");
    }
}
#endregion

#region IFCOrientedEdge
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCEdge"/> EdgeElement |
/// <see cref="bool"/> Orientation
/// 
/// <para/>Parent:
/// <see cref="IFCEdge"/>
/// 
/// </summary>
public class IFCOrientedEdge : IFCEdge
{
    public IFCOrientedEdge(IFCEntity e) : base(e)
    {
        AddKey("EdgeElement");
        AddKey("Orientation");
    }
}
#endregion

#region IFCSubedge
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCEdge"/> ParentEdge
/// 
/// <para/>Parent:
/// <see cref="IFCEdge"/>
/// 
/// </summary>
public class IFCSubedge : IFCEdge
{
    public IFCSubedge(IFCEntity e) : base(e)
    {
        AddKey("ParentEdge");
    }
}
#endregion

#endregion

#region IFCFace
/// <summary>
/// 
/// Properties: 
/// <see cref="IFCFaceBound"/>[] Bounds 
/// 
/// <para/>Parent:
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCFaceSurface"/>
/// 
/// </summary>
public class IFCFace : IIFCTopologicalRepresentationItem
{
    public IFCFace(IFCEntity e) : base(e)
    {
        AddKey("Bounds");
    }
}

#region IFCFaceSurface
/// <summary>
/// 
/// Properties:
/// <see cref="IFCSurface"/> FaceSurface |
/// <see cref="bool"/> SameSense 
/// 
/// <para/>Parent:
/// <see cref="IFCFace"/>
/// 
/// <para/>Children:
/// <see cref="IFCAdvancedFace"/>
/// 
/// </summary>
public class IFCFaceSurface : IFCFace
{
    public IFCFaceSurface(IFCEntity e) : base(e)
    {
        AddKey("FaceSurface");
        AddKey("SameSense");
    }
}

#region IFCAdvancedFace
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCFaceSurface"/>
/// 
/// </summary>
public class IFCAdvancedFace : IFCFaceSurface
{
    public IFCAdvancedFace(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#endregion

#region IFCFaceBound
/// <summary>
/// 
/// Properties:
/// <see cref="IFCLoop"/> Bound |
/// <see cref="bool"/> Orientation
/// 
/// <para/>Parent:
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCFaceOuterBound"/>
/// 
/// </summary>
public class IFCFaceBound : IIFCTopologicalRepresentationItem
{
    public IFCFaceBound(IFCEntity e) : base(e)
    {
        AddKey("Bound");
        AddKey("Orientation");
    }
}

#region IFCFaceOuterBound
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IFCFaceBound"/>
/// 
/// </summary>
public class IFCFaceOuterBound : IFCFaceBound
{
    public IFCFaceOuterBound(IFCEntity e) : base(e)
    {
    }
}
#endregion

#endregion

#region IFCLoop
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCEdgeLoop"/> |
/// <see cref="IFCPolyLoop"/> |
/// <see cref="IFCVertexLoop"/>
/// 
/// </summary>
public class IFCLoop : IIFCTopologicalRepresentationItem
{
    public IFCLoop(IFCEntity e) : base(e)
    {
    }
}

#region IFCEdgeLoop
/// <summary>
/// 
/// Properties:
/// <see cref="IFCOrientedEdge"/>[] EdgeList
/// 
/// <para/>Parent:
/// <see cref="IFCLoop"/>
/// 
/// </summary>
public class IFCEdgeLoop : IFCLoop
{
    public IFCEdgeLoop(IFCEntity e) : base(e)
    {
        AddKey("EdgeList");
    }
}
#endregion

#region IFCPolyLoop
/// <summary>
/// 
/// Properties:
/// <see cref="IFCCartesianPoint"/>[] Polygon
/// 
/// <para/>Parent:
/// <see cref="IFCLoop"/>
/// 
/// </summary>
public class IFCPolyLoop : IFCLoop
{
    public IFCPolyLoop(IFCEntity e) : base(e)
    {
        AddKey("Polygon");
    }
}
#endregion

#region IFCVertexLoop
/// <summary>
/// 
/// Properties:
/// <see cref="IFCVertex"/> LoopVertex
/// 
/// <para/>Parent:
/// <see cref="IFCLoop"/>
/// 
/// </summary>
public class IFCVertexLoop : IFCLoop
{
    public IFCVertexLoop(IFCEntity e) : base(e)
    {
        AddKey("LoopVertex");
    } 
}
#endregion

#endregion

#region IFCPath
/// <summary>
/// 
/// Properties:
/// <see cref="IFCOrientedEdge"/>[] EdgeList
/// 
/// <para/>Parent:
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// </summary>
public class IFCPath : IIFCTopologicalRepresentationItem
{
    public IFCPath(IFCEntity e) : base(e)
    {
        AddKey("EdgeList");
    }
}
#endregion

#region IFCVertex
/// <summary>
/// 
/// <para/>Parent
/// <see cref="IIFCTopologicalRepresentationItem"/>
/// 
/// <para/>Children:
/// <see cref="IFCVertexPoint"/>
/// 
/// </summary>
public class IFCVertex : IIFCTopologicalRepresentationItem
{
    public IFCVertex(IFCEntity e) : base(e)
    {
    }
}

#region IFCVertexPoint
/// <summary>
/// 
/// Properties:
/// <see cref="IIFCPoint"/> VertexGeometry
/// 
/// <para/>Parent:
/// <see cref="IFCVertex"/>
/// 
/// </summary>
public class IFCVertexPoint : IFCVertex
{
    public IFCVertexPoint(IFCEntity e) : base(e)
    {
        AddKey("VertexGeometry");
    }
}
#endregion

#endregion

#endregion

#endregion

#endregion
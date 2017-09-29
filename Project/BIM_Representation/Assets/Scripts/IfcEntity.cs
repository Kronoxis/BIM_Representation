using System;
using System.Collections;
using System.Collections.Generic;

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
    IFCOPENSHELL
}

public class IFCGeometricRepresentationItem
{
    public IFCEntityTypes Type;

    public IFCGeometricRepresentationItem()
    {
        
    }
}

#region IFCPOINT
public abstract class IFCPoint : IFCGeometricRepresentationItem
{
    public static readonly IFCEntityTypes[] SubTypes = {IFCEntityTypes.IFCCARTESIANPOINT, IFCEntityTypes.IFCPOINTONCURVE, IFCEntityTypes.IFCPOINTONSURFACE};
}

public class IFCCartesianPoint : IFCPoint
{
    public List<float> Coordinates;
}

public class IFCPointOnCurve : IFCPoint
{
    //public IFCCurve BasisCurve;
    public float PointParameter;
}

public class IFCPointOnSurface : IFCPoint
{
    //public IFCSurface BasisSurface;
    public float PointParameterU;
    public float PointParameterV;
}
#endregion

#region IFCVERTEX
public abstract class IFCVertex : IFCGeometricRepresentationItem
{
    public static readonly IFCEntityTypes[] SubTypes = {IFCEntityTypes.IFCVERTEXPOINT};
}

public class IFCVertexPoint : IFCVertex
{
    public IFCPoint VertexGeometry;
}
#endregion

#region IFCDIRECTION
public class IfcDirection : IFCGeometricRepresentationItem
{
    public static readonly IFCEntityTypes[] SubTypes = { IFCEntityTypes.IFCDIRECTION };
}
#endregion
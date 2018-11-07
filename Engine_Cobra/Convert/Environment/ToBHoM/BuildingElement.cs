﻿using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.Engine.Environment;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Environment.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this Element element, oM.Geometry.ICurve crv, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            BuildingElementProperties aBuildingElementProperties = pullSettings.FindRefObject(aElementType.Id.IntegerValue) as BuildingElementProperties;

            if (aBuildingElementProperties == null)
            {
                BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)aElementType.Category.Id.IntegerValue);
                if (!aBuildingElementType.HasValue)
                    aBuildingElementType = BuildingElementType.Undefined;

                aBuildingElementProperties = Create.BuildingElementProperties(aElementType.Name, aBuildingElementType.Value);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (pullSettings.CopyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, pullSettings.ConvertUnits) as BuildingElementProperties;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElementProperties);
            }

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, crv);

            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (pullSettings.CopyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element, pullSettings.ConvertUnits) as BuildingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aBuildingElement);

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            //Create a BuildingElement from the familyInstance geometry
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementType? aBEType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
            if (!aBEType.HasValue)
                aBEType = BuildingElementType.Undefined;

            BuildingElementProperties properties = Create.BuildingElementProperties(aBEType.Value);

            return Create.BuildingElement(properties, ToBHoMCurve(familyInstance, pullSettings));
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisSurface energyAnalysisSurface, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BH.oM.Geometry.ICurve crv = null;
            BuildingElementProperties properties = null;

            //Get the geometry curve
            if (energyAnalysisSurface != null)
                crv = energyAnalysisSurface.GetPolyloop().ToBHoM(pullSettings);

            //Get the name and element type
            Document aDocument = energyAnalysisSurface.Document;
            Element aElement = Query.Element(aDocument, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
            ElementType aElementType = null;
            if (aElement != null)
            {
                aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                properties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
                pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddRefObject(pullSettings.RefObjects, properties);
            }

            //Create the BuildingElement
            BuildingElement aBuildingElement = Create.BuildingElement(aElement.Name, crv, properties);

            //Set some custom data properties
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;
                double aHeight = energyAnalysisSurface.Height;
                double aWidth = energyAnalysisSurface.Width;
                double aAzimuth = energyAnalysisSurface.Azimuth;
                if (pullSettings.ConvertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Azimuth", aAzimuth) as BuildingElement;
                if (aElementType != null)
                    aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
            }

            return aBuildingElement;
        }

        /***************************************************/

        internal static BuildingElement ToBHoMBuildingElement(this EnergyAnalysisOpening energyAnalysisOpening, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BH.oM.Geometry.ICurve crv = null;
            BuildingElementProperties properties = null;

            Document aDocument = energyAnalysisOpening.Document;
            Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);
            if (aElement == null)
                return null;

            //Set the properties
            ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
            properties = aElementType.ToBHoMBuildingElementProperties(pullSettings);
            pullSettings.RefObjects = BH.Engine.Adapters.Revit.Modify.AddRefObject(pullSettings.RefObjects, properties);

            //Set the curve
            if (energyAnalysisOpening != null)
                crv = energyAnalysisOpening.GetPolyloop().ToBHoM(pullSettings);

            //Create BuildingElement
            BuildingElement aBuildingElement = Create.BuildingElement(aElement.Name, crv, properties);

            //Set custom data on BuildingElement
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
            if (pullSettings.CopyCustomData)
            {
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, pullSettings.ConvertUnits) as BuildingElement;

                double aHeight = energyAnalysisOpening.Height;
                double aWidth = energyAnalysisOpening.Width;
                if (pullSettings.ConvertUnits)
                {
                    aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                    aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                }
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Type", energyAnalysisOpening.OpeningType.ToString()) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Name", energyAnalysisOpening.OpeningName) as BuildingElement;
                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, pullSettings.ConvertUnits) as BuildingElement;
            }

            return aBuildingElement;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Ceiling ceiling, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();

            BuildingElementProperties properties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(pullSettings) as BuildingElementProperties;

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(ceiling, pullSettings);
            if (aPolyCurveList == null)
                return buildingElements;

            foreach(oM.Geometry.PolyCurve aPolyCurve in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, aPolyCurve);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, ceiling) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, ceiling, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }       

            return buildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();
            BuildingElementProperties properties = floor.FloorType.ToBHoMBuildingElementProperties(pullSettings);

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(floor, pullSettings);
            if (aPolyCurveList == null)
                return buildingElements;

            foreach (oM.Geometry.ICurve crv in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, crv);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, floor) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, floor, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }

            return buildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this RoofBase roofBase, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<BuildingElement> buildingElements = new List<BuildingElement>();
            BuildingElementProperties properties = roofBase.RoofType.ToBHoMBuildingElementProperties(pullSettings);

            List<oM.Geometry.PolyCurve> aPolyCurveList = Query.Profiles(roofBase, pullSettings);
            if (aPolyCurveList == null)
                return buildingElements;

            foreach (oM.Geometry.ICurve crv in aPolyCurveList)
            {
                //Create the BuildingElement
                BuildingElement aElement = Create.BuildingElement(properties, crv);

                //Assign custom data
                aElement = Modify.SetIdentifiers(aElement, roofBase) as BuildingElement;
                if (pullSettings.CopyCustomData)
                    aElement = Modify.SetCustomData(aElement, roofBase, pullSettings.ConvertUnits) as BuildingElement;

                buildingElements.Add(aElement);
            }

            return buildingElements;
        }

        /***************************************************/

        internal static List<BuildingElement> ToBHoMBuildingElements(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(pullSettings) as BuildingElementProperties;

            List<BuildingElement> buildingElements = new List<BuildingElement>();

            IList<Reference> aReferences = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            foreach (Reference aReference in aReferences)
            {
                //Element aElement = wall.Document.GetElement(aReference);
                Face aFace = wall.GetGeometryObjectFromReference(aReference) as Face;
                if (aFace == null)
                    continue;

                List<oM.Geometry.PolyCurve> aPolyCurveList = Query.PolyCurves(aFace, pullSettings);
                if (aPolyCurveList == null)
                    continue;

                foreach(oM.Geometry.PolyCurve aPolyCurve in aPolyCurveList)
                {
                    //Create the BuildingElement
                    BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aPolyCurve);

                    //Assign custom data
                    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                    if (pullSettings.CopyCustomData)
                        aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, pullSettings.ConvertUnits) as BuildingElement;

                    buildingElements.Add(aBuildingElement);
                }

            }

            return buildingElements;
        }

        /***************************************************/
    }
}
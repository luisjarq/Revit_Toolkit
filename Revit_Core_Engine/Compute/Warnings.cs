/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static void NotConvertedWarning(this Element element, Discipline discipline)
        {
            string message = String.Format("Revit element conversion to BHoM failed for discipline {0}. The element has been converted into a generic BHoM instance.", discipline);

            if (element != null)
                message += string.Format(" Element Type: {0}, Element Id: {1}, Element Name: {2}", element.GetType(), element.Id.IntegerValue, element.Name);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NotConvertedWarning(this StructuralMaterialType structuralMaterialType)
        {
            BH.Engine.Base.Compute.RecordWarning("Structural material type " + structuralMaterialType + " could not be converted because conversion method does not exist.");
        }

        /***************************************************/

        internal static void NotConvertedWarning(this FamilySymbol symbol)
        {
            BH.Engine.Base.Compute.RecordWarning("Framing profile " + symbol.Name + " could not be converted. ElementId: " + symbol.Id.IntegerValue.ToString());
        }

        /***************************************************/

        internal static void CheckIfNullPush(this Element element, IBHoMObject bhomObject)
        {
            if (element == null)
                BH.Engine.Base.Compute.RecordWarning(string.Format("Revit element has not been created due to BHoM/Revit conversion issues. BHoM element Guid: {0}", bhomObject.BHoM_Guid));
        }

        /***************************************************/

        internal static void NullObjectWarning()
        {
            BH.Engine.Base.Compute.RecordWarning("BHoM object could not be created becasue Revit object is null.");
        }

        /***************************************************/

        internal static void NullRevitElementWarning(this IBHoMObject bhomObject)
        {
            string message = "Referenced Revit element could not be found.";

            if (bhomObject != null)
                message = string.Format("{0} BHoM Guid: {1}", message, bhomObject.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void LogNullProperties(this BHoMObject obj, IEnumerable<string> propertyNames = null)
        {
            //TODO: Move this one to the BHoM_Engine?
            List<string> nullPropertyNames = new List<string>();

            Type type = obj.GetType();
            if (propertyNames == null)
            {
                PropertyInfo[] properties = type.GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    if (pi.GetValue(obj) == null) nullPropertyNames.Add(pi.Name);
                }
            }
            else
            {
                foreach (string propertyName in propertyNames)
                {
                    if (type.GetProperty(propertyName).GetValue(obj) == null) nullPropertyNames.Add(propertyName);
                }
            }

            if (nullPropertyNames.Count > 0)
            {
                string warning = string.Format("The BHoM object if missing following properties: {0}. BHoM_Guid: {1}.", string.Join(", ", nullPropertyNames), obj.BHoM_Guid);

                ElementId revitID = obj.ElementId();
                if (revitID != null) warning += string.Format(" Revit ElementId: {0}.", revitID.IntegerValue);
                BH.Engine.Base.Compute.RecordWarning(warning);
            }
        }

        /***************************************************/

        internal static void UnknownMaterialWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("Revit symbol has been converted to a steel profile with an unknown material. Element Id: {0}, Element Name: {1}", familyInstance.Id.IntegerValue, familyInstance.Name));
        }

        /***************************************************/

        internal static void MaterialNotFoundWarning(this string materialGrade)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("A BHoM equivalent to the Revit material has not been found. Material  grade: {0}", materialGrade));
        }

        /***************************************************/

        internal static void MaterialNotFoundWarning(this oM.Physical.Materials.Material material)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("A Revit equivalent to the BHoM material has not been found. BHoM Guid: {0}", material.BHoM_Guid));
        }

        /***************************************************/

        internal static void MaterialNotStructuralWarning(this oM.Physical.Materials.Material material)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("The material does not contain any structural properties. BHoM Guid: {0}", material.BHoM_Guid));
        }

        /***************************************************/

        internal static void CompositePanelWarning(this HostObjAttributes hostObjAttributes)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("Composite panels are currently not supported in BHoM. A zero thickness panel is created. Element type Id: {0}", hostObjAttributes.Id.IntegerValue));
        }

        /***************************************************/

        internal static void OpeningInPanelWarning(this oM.Structure.Elements.Panel panel)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("In current implementation of BHoM the panels are pushed without openings. {0} openings are skipped for the panel with BHoM_Guid: {1}", panel.Openings.Count, panel.BHoM_Guid));
        }

        /***************************************************/

        internal static void ConvertProfileFailedWarning(this FamilySymbol familySymbol)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("Revit family symbol conversion to BHoM profile failed, zero profile is returned. Family symbol Id: {0}", familySymbol.Id.IntegerValue));
        }

        /***************************************************/

        internal static void NonlinearBarWarning(this FamilyInstance bar)
        {
            string message = "Nonlinear bars are currently not supported in BHoM, the object is returned with empty geometry.";

            if (bar != null)
                message = string.Format("{0} Element Id: {1}", message, bar.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void FramingCurveNotFoundWarning(this FamilyInstance framing)
        {
            string message = "Driving curve of a Revit framing element could not be retrieved.";

            if (framing != null)
                message = string.Format("{0} ElementId: {1}", message, framing.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void FramingCurveNotFoundWarning(this IFramingElement framingElement)
        {
            string message = "Driving curve of a BHoM framing element could not be retrieved.";

            if (framingElement != null)
                message = string.Format("{0} BHoM_Guid: {1}", message, framingElement.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void UnsupportedOutlineCurveWarning(this HostObject hostObject)
        {
            string message = "The panel outline contains a curve that is currently not supported in BHoM, the object is returned with empty geometry.";

            if (hostObject != null)
                message = string.Format("{0} Element Id: {1}", message, hostObject.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NonClosedOutlineWarning(this HostObject hostObject)
        {
            string message = "The panel outline is not closed, the object is returned with empty geometry.";

            if (hostObject != null)
                message = string.Format("{0} Element Id: {1}", message, hostObject.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void ElementCouldNotBeQueriedWarning(this Element element)
        {
            string message = "Revit element could not be queried.";

            if (element != null)
                message = string.Format("{0} Element Id: {1}", message, element.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NullStructuralAssetWarning(this BH.oM.Structure.MaterialFragments.IMaterialFragment material)
        {
            string message = "Could not find Revit Structural Asset for BHoM Object.";

            if (material != null)
                message = string.Format("{0} BHoM Guid: {1}", message, material.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void UnknownStructuralAssetWarning(this BH.oM.Structure.MaterialFragments.IMaterialFragment material)
        {
            string message = "Revit Structural Asset could not be converted into BHoM material properties.";

            if (material != null)
                message = string.Format("{0} BHoM Guid: {1}", message, material.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NullThermalAssetWarning(this oM.Environment.MaterialFragments.IEnvironmentMaterial materialProperties)
        {
            string message = "Could not find Revit Thermal Asset for BHoM Object.";

            if (materialProperties != null)
                message = string.Format("{0} BHoM Guid: {1}", message, materialProperties.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void MaterialTypeNotFoundWarning(this Material material)
        {
            string message = "Matching BHoM ElementType could not be found.";

            if (material != null)
                message = string.Format("{0} Material Element Id: {1}", message, material.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void MaterialTypeNotFoundWarning(this FamilyInstance familyInstance)
        {
            string message = "Matching BHoM ElementType could not be found.";

            if (familyInstance != null)
                message = string.Format("{0} Element Id: {1}", message, familyInstance.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void InvalidDataMaterialWarning(this Element element)
        {
            string message = "Material could not be correctly converted. Some BHoM Material data may not be valid.";

            if (element != null)
                message = string.Format("{0} Element Id: {1}", message, element.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void ElementTypeNotFoundWarning(this IBHoMObject iBHoMObject)
        {
            string message = "Element type has not been found for given BHoM Object. Please make sure the name of the BHoM object (or its type property, e.g. Construction) is matching the Revit family type name.";

            if (iBHoMObject != null)
                message = $"{message}\nObject name: {iBHoMObject.Name}, object type: {iBHoMObject.GetType().FullName}.";

            BH.Engine.Base.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void GeometryConvertFailed(this IBHoMObject iBHoMObject)
        {
            string message = "Conversion of the element geometry failed.";

            if (iBHoMObject != null)
                message = string.Format("{0} BHoM Guid: {1}", message, iBHoMObject.BHoM_Guid);

            BH.Engine.Base.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void ViewTemplateNotExistsWarning(this oM.Adapters.Revit.Elements.ViewPlan viewPlan)
        {
            string message = "View Template has not been found for given BHoM ViewPlan.";

            if (viewPlan != null)
                message = string.Format("{0} BHoM Guid: {1} View Template Name: {2}", message, viewPlan.BHoM_Guid, viewPlan.TemplateName);

            BH.Engine.Base.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void NullObjectPropertiesWarning(this IInstance instance)
        {
            string message = "The instance has no object properties.";

            if (instance != null)
                message = string.Format("{0} BHoM Guid: {1}", message, instance.BHoM_Guid);

            BH.Engine.Base.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void AnalyticalObjectConversionWarning(this IObject iObject, Type type = null)
        {
            string message = "Analytical object cannot be converted to Revit.";

            if (iObject is IBHoMObject)
                message = string.Format("{0} BHoM Guid: {1} Type: {2}.", message, ((IBHoMObject)iObject).BHoM_Guid, iObject.GetType().FullName);
            else
                message = string.Format("{0} Type: {1}.", message, iObject.GetType().FullName);

            if (type != null)
                message = string.Format("{0} Use {1} instead", message, type.FullName);

            BH.Engine.Base.Compute.RecordError(message);
        }

        /***************************************************/

        internal static void AnalyticalPullWarning(this Element element)
        {
            BH.Engine.Base.Compute.RecordWarning(string.Format("Location of element's analytical model has been pulled. Element Id: {0}", element.Id.IntegerValue));
        }

        /***************************************************/

        internal static void CurveToBHoMNotImplemented(this Curve curve)
        {
            BH.Engine.Base.Compute.RecordError(string.Format("Conversion for curve type {0} to BHoM is not implemented.", curve.GetType().ToString().Split('.').Last()));
        }

        /***************************************************/

        internal static void MultiSegmentCurveError()
        {
            BH.Engine.Base.Compute.RecordWarning("Revit does not suppport conversion of multi-segment BHoM curves (Polyline, PolyCurve). Please consider exploding the curve into its SubParts.");
        }

        /***************************************************/

        internal static void NonPlanarCurveError(this IBHoMObject iBHoMObject)
        {
            string message = "Revit accepts curve-based ModelInstances only when the curve is planar.";
            if (iBHoMObject != null)
                message = string.Format("{0} BHoM Guid: {1}", message, iBHoMObject.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void ClosedNurbsCurveError(this IBHoMObject iBHoMObject)
        {
            string message = "Revit does not support closed nurbs curves.";
            if (iBHoMObject != null)
                message = string.Format("{0} BHoM Guid: {1}", message, iBHoMObject.BHoM_Guid);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NonLinearFramingOffsetWarning(this FamilyInstance familyInstance)
        {
            string message = "Handling of offset/justification is currently not supported for nonlinear framing. Revit justification and offset has been ignored.";
            if (familyInstance != null)
                message += string.Format(" Revit ElementId: {0}", familyInstance.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void NoProfileWarning(this FamilyInstance familyInstance)
        {
            string message = "Profile of the framing element could not be retrieved. Revit justification and offset has been ignored.";
            if (familyInstance != null)
                message += string.Format(" Revit ElementId: {0}", familyInstance.Id.IntegerValue);

            BH.Engine.Base.Compute.RecordWarning(message);
        }

        /***************************************************/

        internal static void ProjectedOnXYWarning(this FamilyInstance familyInstance)
        {
            FamilyPlacementType fpt = ((FamilySymbol)familyInstance.Document.GetElement(familyInstance.GetTypeId())).Family.FamilyPlacementType;
            BH.Engine.Base.Compute.RecordWarning($"Family with placement type {fpt} needs to be placed vertically, therefore transform out of XY plane has been ignored. ElementId: {familyInstance?.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void NotPerpendicularOrientationWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"Family instance needs to lie in the plane perpendicular to the MEPCurve direction, therefore transform out of this plane has been ignored. ElementId: {familyInstance?.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void ProjectedOnViewPlaneWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"Drafting family needs to have orientation perpendicular to the view plane, therefore transform out of view plane has been ignored. BHoM Guid: ElementId: {familyInstance?.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void HostIgnoredWarning(this FamilyInstance familyInstance)
        {
            FamilyPlacementType fpt = ((FamilySymbol)familyInstance.Document.GetElement(familyInstance.GetTypeId())).Family.FamilyPlacementType;
            BH.Engine.Base.Compute.RecordWarning($"Family with placement type {fpt} does not need a host element, therefore the input host has been ignored. ElementId: {familyInstance?.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void CurveBasedNonHostedWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"A curve-based non-structural element has been created without a host. There is a high chance it got created in a wrong location due to Revit limitations, therefore it is recommended to inspect the element. ElementId: {familyInstance.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void TwoLevelBasedByPointWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"A point-based, two level based element has been created. There is a high chance it got created in a wrong location due to Revit limitations, therefore it is recommended to inspect the element. ElementId: {familyInstance.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void CurveBasedHostedWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"A curve-based non-structural element has been created with a host. That functionality had not been tested, therefore it is recommended to inspect the element. ElementId: {familyInstance.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void ReferencePlaneCreatedWarning(this FamilyInstance familyInstance)
        {
            BH.Engine.Base.Compute.RecordWarning($"A dedicated reference plane has been created in order to place the family instance in the correct location. ElementId: {familyInstance.Id.IntegerValue}");
        }

        /***************************************************/

        internal static void TransformNotImplementedWarning(this Element element)
        {
            BH.Engine.Base.Compute.RecordWarning($"Transform method has not been implemented in convert from {element.GetType()} to type BHoM.\n" +
                                                        $"All elements of type {element.GetType()} coming from link {element.Document.Title} will be be pulled in coordinate system of the link, not the host model.");
        }

        /***************************************************/
    }
}




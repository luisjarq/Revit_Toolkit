/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Environment.Fragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Physical.IPhysical> ToBHoMIPhysicals(this HostObject hostObject, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Physical.IPhysical> aPhysicalList = pullSettings.FindRefObjects<oM.Physical.IPhysical>(hostObject.Id.IntegerValue);
            if (aPhysicalList != null && aPhysicalList.Count > 0)
                return aPhysicalList;

            //TODO: check if the attributes != null
            HostObjAttributes hostObjAttributes = hostObject.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
            oM.Physical.Constructions.Construction aConstruction = ToBHoMConstruction(hostObjAttributes, pullSettings);
            string materialGrade = hostObject.MaterialGrade();
            aConstruction.UpdateMaterialProperties(hostObjAttributes, materialGrade, pullSettings);

            IEnumerable<PlanarSurface> aPlanarSurfaces = Query.PlanarSurfaces(hostObject, pullSettings);
            if (aPlanarSurfaces == null)
                return null;

            aPhysicalList = new List<oM.Physical.IPhysical>();

            foreach(PlanarSurface aPlanarSurface in aPlanarSurfaces)
            {

                oM.Physical.IPhysical aPhysical = null;

                if (hostObject is Wall)
                {
                    aPhysical = BH.Engine.Physical.Create.Wall(aPlanarSurface, aConstruction);

                    Wall aWall = (Wall)hostObject;
                    CurtainGrid aCurtainGrid = aWall.CurtainGrid;
                    if (aCurtainGrid != null)
                    {
                        foreach (ElementId aElementId in aCurtainGrid.GetPanelIds())
                        {
                            Panel aPanel = aWall.Document.GetElement(aElementId) as Panel;
                            if (aPanel == null)
                                continue;
                        }
                    }

                }
                else if (hostObject is Floor)
                    aPhysical = BH.Engine.Physical.Create.Floor(aPlanarSurface, aConstruction);
                else if (hostObject is RoofBase)
                    aPhysical = BH.Engine.Physical.Create.Roof(aPlanarSurface, aConstruction);

                if (aPhysical == null)
                    continue;

                aPhysical.Name = Query.FamilyTypeFullName(hostObject);

                ElementType aElementType = hostObject.Document.GetElement(hostObject.GetTypeId()) as ElementType;
                //Set ExtendedProperties
                OriginContextFragment aOriginContextFragment = new OriginContextFragment();
                aOriginContextFragment.ElementID = hostObject.Id.IntegerValue.ToString();
                aOriginContextFragment.TypeName = Query.FamilyTypeFullName(hostObject);
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, hostObject) as OriginContextFragment;
                aOriginContextFragment = aOriginContextFragment.UpdateValues(pullSettings, aElementType) as OriginContextFragment;
                aPhysical.Fragments.Add(aOriginContextFragment);

                aPhysical = Modify.SetIdentifiers(aPhysical, hostObject) as oM.Physical.IPhysical;
                if (pullSettings.CopyCustomData)
                    aPhysical = Modify.SetCustomData(aPhysical, hostObject, pullSettings.ConvertUnits) as oM.Physical.IPhysical;

                aPhysical = aPhysical.UpdateValues(pullSettings, hostObject) as oM.Physical.IPhysical;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aPhysical);

                aPhysicalList.Add(aPhysical);
            }

            return aPhysicalList;
        }

        /***************************************************/
    }
}
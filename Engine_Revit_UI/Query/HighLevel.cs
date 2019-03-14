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
using System.Linq;

using Autodesk.Revit.DB;
using BH.oM.Common.Interface;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Level HighLevel(this Document document, double elevation, bool convertUnits = true)
        {
            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count == 0)
                return null;

            aLevelList.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            double aElevation = aLevelList.First().Elevation;
            if (convertUnits)
                aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

            if (Math.Abs(elevation - aElevation) < oM.Geometry.Tolerance.MacroDistance)
                return aLevelList.First();

            for (int i = 1; i < aLevelList.Count; i++)
            {
                aElevation = aLevelList[i].Elevation;
                if (convertUnits)
                    aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);

                //if (Elevation) <= Math.Round(aElevation, 3, MidpointRounding.AwayFromZero))
                if (Math.Round(elevation, 3, MidpointRounding.AwayFromZero) <= Math.Round(aElevation, 3, MidpointRounding.AwayFromZero))
                    return aLevelList[i];
            }


            return aLevelList.Last();
        }

        /***************************************************/

        static public Level HighLevel(this Document document, oM.Geometry.ICurve curve, bool convertUnits = true)
        {
            double aElevation = HighElevation(curve);

            return HighLevel(document, aElevation, convertUnits);
        }

        /***************************************************/

        static public Level HighLevel(this Document document, IObject2D object2D, bool convertUnits = true)
        {
            double aElevation = HighElevation(object2D);

            return HighLevel(document, aElevation, convertUnits);
        }

        /***************************************************/
    }
}
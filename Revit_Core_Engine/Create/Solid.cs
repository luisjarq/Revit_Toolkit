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
using BH.oM.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Create solid object from filled region limited by bottom and top elevation.")]
        [Input("filledRegion", "Filled region that is extruded to the solid.")]
        [Input("bottomElevation", "Bottom elevation of the solid (Revit unit system).")]
        [Input("topElevation", "Top elevation of the solid (Revit unit system).")]
        [Output("solid", "Solid representation of extruded filled region.")]
        public static Solid Solid(FilledRegion filledRegion, double bottomElevation, double topElevation)
        {
            if (filledRegion == null)
            {
                BH.Engine.Base.Compute.RecordError($"Filled region cannot be null.");
                return null;
            }
            else if (topElevation - bottomElevation < Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError($"Top elevation value must be greater than bottom elevation.");
                return null;
            }
            
            IList<CurveLoop> boundaries = filledRegion.GetBoundaries();

            return Solid(boundaries, bottomElevation, topElevation);
        }

        /***************************************************/

        [Description("Create solid object from boundaries limited by bottom and top elevation.")]
        [Input("boundaries", "Boundaries list that is extruded to the solid.")]
        [Input("bottomElevation", "Bottom elevation of the solid (Revit unit system).")]
        [Input("topElevation", "Top elevation of the solid (Revit unit system).")]
        [Output("solid", "Solid representation of extruded boundaries.")]
        public static Solid Solid(IList<CurveLoop> boundaries, double bottomElevation, double topElevation)
        {
            if (boundaries == null || boundaries.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Boundaries cannot be null or empty.");
                return null;
            }
            else if (topElevation - bottomElevation < Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError($"Top elevation value must be greater than bottom elevation.");
                return null;
            }

            double elev = boundaries[0].GetPlane().Origin.Z;
            XYZ dir = XYZ.BasisZ;
            double height = topElevation - bottomElevation;
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(boundaries, dir, height);

            if (Math.Abs(elev - bottomElevation) > Tolerance.Distance)
            {
                XYZ translation = new XYZ(0, 0, bottomElevation - elev);
                Transform transform = Transform.CreateTranslation(translation);
                solid = SolidUtils.CreateTransformed(solid, transform);
            }

            return solid;
        }

        /***************************************************/
    }
}

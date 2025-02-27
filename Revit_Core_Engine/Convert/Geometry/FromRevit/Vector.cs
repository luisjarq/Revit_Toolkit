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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit XYZ to BH.oM.Geometry.Vector.")]
        [Input("xyz", "Revit XYZ to be converted.")]
        [Output("vector", "BH.oM.Geometry.Vector resulting from converting the input Revit XYZ.")]
        public static oM.Geometry.Vector VectorFromRevit(this XYZ xyz)
        {
            if (xyz == null)
                return null;

            return new Vector { X = xyz.X.ToSI(SpecTypeId.Length), Y = xyz.Y.ToSI(SpecTypeId.Length), Z = xyz.Z.ToSI(SpecTypeId.Length) };
        }

        /***************************************************/
    }
}


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

using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Physical.Elements;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Queries the material take off information extracted from a Revit element on Pull and stored in RevitMaterialTakeOff fragment attached to a given BHoMObject.")]
        [Input("bHoMObject", "BHoMObject to be queried for the material take off information extracted from a Revit element on Pull.")]
        [Output("takeOff", "Material take off information in a form of an ExplicitBulk.")]
        public static ExplicitBulk MaterialTakeoff(this IBHoMObject bHoMObject)
        {
            RevitMaterialTakeOff takeOff = bHoMObject?.Fragments?.FirstOrDefault(x => x is RevitMaterialTakeOff) as RevitMaterialTakeOff;
            if (takeOff == null)
                return null;

            return new ExplicitBulk { MaterialComposition = takeOff.MaterialComposition, Volume = takeOff.TotalVolume };
        }

        /***************************************************/
    }
}



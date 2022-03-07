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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a RevitComparisonConfig with specific inputs assigned.")]
        [Input("parametersToConsider", "Names of the Revit Parameters that will be considered for the comparison." +
            "By default, this list is empty, so all parameters are considered (except possibly those included in the other property `ParametersExceptions`).")]
        [Input("propertiesToConsider", "If one or more entries are specified here, only objects/properties that match them will be considered." +
           "\nE.g. Given input objects BH.oM.Structure.Elements.Bar, specifying `StartNode` will only check that property of the Bar." +
           "\nSupports * wildcard." +
           "\nNote that using this will incur in a general slowdown because it is computationally heavy. See the wiki for more details.")]
        public static RevitComparisonConfig RevitComparisonConfig(List<string> parametersToConsider, List<string> propertiesToConsider = null)
        {
            RevitComparisonConfig rcc = new RevitComparisonConfig()
            {
                PropertiesToConsider = propertiesToConsider,
                ParametersToConsider = parametersToConsider
            };

            return rcc;
        }

        /***************************************************/

        [Description("Creates a RevitComparisonConfig with specific inputs assigned.")]
        [Input("parametersToConsider", "Names of the Revit Parameters that will be considered for the comparison." +
            "By default, this list is empty, so all parameters are considered (except possibly those included in the other property `ParametersExceptions`).")]
        [Input("considerOnlyParameterDifferences", "If `true`, objects will be considered 'Modified' only if their RevitParameter changed, and only RevitParameterDifferences will be returned.'")]
        public static RevitComparisonConfig RevitComparisonConfig(List<string> parametersToConsider, bool considerOnlyParameterDifferences = false)
        {
            RevitComparisonConfig rcc = new RevitComparisonConfig()
            {
                ParametersToConsider = parametersToConsider,
                PropertiesToConsider = considerOnlyParameterDifferences ? new List<string>() { "Considering only Revit Parameter Differences" } : new List<string>() // using a very improbable PropertyToConsider name to exclude all differences that are not Revit Parameter differences.
            };

            return rcc;
        }

        /***************************************************/
    }
}

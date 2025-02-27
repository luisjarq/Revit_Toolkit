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

using BH.oM.Adapters.Revit.Requests;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Data.Requests;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;
using BH.Engine.Data;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the discipline to which a given BHoM type belongs. The result is based on the namespace in which the type is declared, e.g. BH.oM.Structure.Elements.Bar will return oM.Adapters.Revit.Enums.Discipline.Structural.")]
        [Input("type", "BHoM type to be queried.")]
        [Output("discipline")]
        public static Discipline Discipline(this Type type)
        {
            if (type == null)
                return oM.Adapters.Revit.Enums.Discipline.Undefined;

            if(type.Namespace.StartsWith("BH.oM.Structure"))
                return oM.Adapters.Revit.Enums.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environment"))
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            if (type.Namespace.StartsWith("BH.oM.Architecture"))
                return oM.Adapters.Revit.Enums.Discipline.Architecture;

            if (type.Namespace.StartsWith("BH.oM.Physical"))
                return oM.Adapters.Revit.Enums.Discipline.Physical;

            if (type.Namespace.StartsWith("BH.oM.Facade"))
                return oM.Adapters.Revit.Enums.Discipline.Facade;

            return oM.Adapters.Revit.Enums.Discipline.Undefined;
        }

        /***************************************************/

        [Description("Gets discipline enforced by the Request. If the result is different than defaultDiscipline and neither of two is Undefined, null is returned (the result discipline is conflicting with defaultDiscipline).")]
        [Input("request", "BHoM Request to be queried.")]
        [Input("defaultDiscipline", "Default discipline set in adapter's ActionConfig (RevitPullConfig).")]
        [Output("discipline")]
        public static Discipline? Discipline(this IRequest request, Discipline? defaultDiscipline = oM.Adapters.Revit.Enums.Discipline.Undefined)
        {
            Discipline? discipline = defaultDiscipline;
            if (request is ILogicalRequest)
            {
                foreach (IRequest subRequest in (request as ILogicalRequest).IRequests())
                {
                    discipline = subRequest.Discipline(discipline);
                    if (discipline == null)
                        return null;
                }
            }
            else if (request is FilterRequest)
            {
                Discipline requestDiscipline = (request as FilterRequest).Type.Discipline();

                if (discipline == oM.Adapters.Revit.Enums.Discipline.Undefined)
                    discipline = requestDiscipline;
                else if (discipline != requestDiscipline)
                    return null;
            }
            else if (request is EnergyAnalysisModelRequest)
            {
                if (discipline == oM.Adapters.Revit.Enums.Discipline.Undefined)
                    discipline = oM.Adapters.Revit.Enums.Discipline.Environmental;
                else if (discipline != oM.Adapters.Revit.Enums.Discipline.Environmental)
                    return null;
            }

            return discipline;
        }

        /***************************************************/
    }
}



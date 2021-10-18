﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Diffing;
using BH.Engine.Diffing;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            return Diffing(pastObjects, followingObjects, "UniqueId", propertiesOrParamsToConsider, null);
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            return Diffing(pastObjects, followingObjects, revitIdName, propertiesOrParamsToConsider, null);
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null, DiffingConfig diffConfig = null)
        {
            return Diffing(pastObjects, followingObjects, revitIdName, propertiesOrParamsToConsider, diffConfig);
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
        "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        private static Diff Diffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null, DiffingConfig diffConfig = null)
        {
            // Set configurations if diffConfig is null. Clone it for immutability in the UI.
            DiffingConfig diffConfigClone = diffConfig == null ? new DiffingConfig() { IncludeUnchangedObjects = true } : diffConfig.DeepClone();

            if (pastObjects == null)
                pastObjects = new List<object>();

            if (followingObjects == null)
                followingObjects = new List<object>();

            // // - Now do the necessary configurations for the Revit-specific diffing.

            // Checks and setup of revitIdName.
            if (revitIdName == "UniqueId" || revitIdName == nameof(RevitIdentifiers.PersistentId))
                revitIdName = nameof(RevitIdentifiers.PersistentId);
            else if (revitIdName != "ElementId")
            {
                BH.Engine.Reflection.Compute.RecordError($"The input parameter {nameof(revitIdName)} can only be 'ElementId', 'UniqueId' or '{nameof(RevitIdentifiers.PersistentId)}' (BHoM's equivalent of Revit's UniqueId), but '{revitIdName}' was specified.");
                return null;
            }

            // Set up the diffingConfig so it deals with Revit objects correctly.
            if (propertiesOrParamsToConsider != null)
                diffConfigClone.ComparisonConfig.PropertiesToConsider.AddRange(propertiesOrParamsToConsider);
            diffConfigClone.ComparisonConfig.TypeExceptions.Add(typeof(IRevitParameterFragment));


            // Check if we already specified some ComparisonFunctions or not.
            if (diffConfigClone.ComparisonConfig.ComparisonFunctions == null)
                diffConfigClone.ComparisonConfig.ComparisonFunctions = new ComparisonFunctions();

            // set a PropertyFullNameModifier, so that Revit Parameters are considered as object declared properties.
            diffConfigClone.ComparisonConfig.ComparisonFunctions.PropertyFullNameModifier = diffConfigClone.ComparisonConfig.ComparisonFunctions.PropertyFullNameModifier ?? PropertyFullNameModifier_RevitParameterNameAsPropertyName;
            diffConfigClone.ComparisonConfig.ComparisonFunctions.PropertyDisplayNameModifier = diffConfigClone.ComparisonConfig.ComparisonFunctions.PropertyDisplayNameModifier ?? PropertyDisplayNameModifier;

            // Compute the diffing through DiffWithFragmentId() with revitDiffingConfig.
            BH.Engine.Reflection.Compute.RecordNote("Computing the revit-specific Diffing.");
            Diff revitDiff = BH.Engine.Diffing.Compute.DiffWithFragmentId(pastObjects.OfType<IBHoMObject>(), followingObjects.OfType<IBHoMObject>(), typeof(RevitIdentifiers), revitIdName, diffConfigClone);

            // Combine and return the Diffs.
            return revitDiff;
        }


        /***************************************************/

        // This method is to be passed to the DiffingConfig.ComparisonConfig.ComparisonFunctions (as Func delegate).
        // It will be called when needed by the base Diffing methods.
        // The method ensures that Revit Parameters are treated as main properties of an object,
        // e.g. we can specify as exception to the diffing `someRevitObj.SomeParameter`, as like `SomeParameter` was a property of the object (when instead it's stored on a RevitParameter fragment).
        private static string PropertyFullNameModifier_RevitParameterNameAsPropertyName(string propertyFullName, object propertyValue)
        {
            // If the object is not a RevitParameter, just return the propertyFullName as-is.
            RevitParameter revitParameter = propertyValue as RevitParameter;
            if (revitParameter == null)
                return propertyFullName;

            // Get the parameter name, and modify the input propertyFullName as if the parameter was a declared property of the object.
            string parameterName = revitParameter.Name;
            string modifiedPropertyFullName = propertyFullName.Split(new string[] { "Fragments" }, StringSplitOptions.None).First() + parameterName;

            return modifiedPropertyFullName;
        }

        /***************************************************/

        // This method is to be passed to the DiffingConfig.ComparisonConfig.ComparisonFunctions (as Func delegate).
        // It will be called when needed by the base Diffing methods.
        // The method displays the property difference for Revit parameters using the parameter name.
        // e.g. we can specify as exception to the diffing `someRevitObj.SomeParameter`, as like `SomeParameter` was a property of the object (when instead it's stored on a RevitParameter fragment).
        private static string PropertyDisplayNameModifier(string propertyFullName, object propertyValue)
        {
            // If the object is not a RevitParameter, just return the propertyFullName as-is.
            RevitParameter revitParameter = propertyValue as RevitParameter;
            if (revitParameter == null)
                return propertyFullName;

            // Get the parameter name, and modify the input propertyFullName as if the parameter was a declared property of the object.
            string parameterName = revitParameter.Name;
            string modifiedPropertyFullName = parameterName + " (RevitParameter)";

            return modifiedPropertyFullName;
        }
    }
}

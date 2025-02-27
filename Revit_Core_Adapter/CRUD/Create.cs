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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.Revit.Engine.Core;
using System;
using System.Collections.Generic;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public List<IBHoMObject> Create(IEnumerable<IBHoMObject> bHoMObjects, RevitPushConfig pushConfig)
        {
            Document document = this.Document;
            RevitSettings settings = this.RevitSettings.DefaultIfNull();

            Dictionary<Guid, List<int>> refObjects = new Dictionary<Guid, List<int>>();
            List<IBHoMObject> created = new List<IBHoMObject>();
            foreach (IBHoMObject obj in bHoMObjects)
            {
                if (Create(obj, document, settings, refObjects) != null)
                    created.Add(obj);
            }

            return created;
        }

        /***************************************************/

        public static Element Create(IBHoMObject bHoMObject, Document document, RevitSettings settings, Dictionary<Guid, List<int>> refObjects)
        {
            if (bHoMObject == null)
            {
                NullObjectCreateError(typeof(IBHoMObject));
                return null;
            }

            try
            {
                Element element = bHoMObject.IToRevit(document, settings, refObjects);
                bHoMObject.SetIdentifiers(element);
                
                //Assign Tags
                string tagsParameterName = null;
                if (settings != null)
                    tagsParameterName = settings.MappingSettings?.TagsParameter;
                
                if (!string.IsNullOrEmpty(tagsParameterName))
                    element.SetTags(bHoMObject, tagsParameterName);

                return element;
            }
            catch (Exception ex)
            {
                ObjectNotCreatedError(bHoMObject, ex);
                return null;
            }
        }

        /***************************************************/
    }
}


/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using BH.UI.Revit.Engine;

using BH.oM.Base;
using BH.oM.Structure.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Properties;

namespace BH.UI.Revit.Adapter
{
    public partial class RevitUIAdapter : BH.Adapter.Revit.InternalRevitAdapter
    {
        /***************************************************/
        /****             Protected Methods             ****/
        /***************************************************/

        protected override bool Create<T>(IEnumerable<T> objects)
        {
            if (Document == null)
            {
                NullDocumentCreateError();
                return false;
            }

            if (objects == null)
            {
                NullObjectsCreateError();
                return false;
            }

            if (objects.Count() < 1)
                return false;

            Document document = Document;

            bool result = false;
            if (!document.IsModifiable && !document.IsReadOnly)
            {
                //Transaction has to be opened
                using (Transaction transaction = new Transaction(document, "Create"))
                {
                    transaction.Start();
                    result = Create(objects, UIControlledApplication, document, RevitSettings);
                    transaction.Commit();
                }
            }
            else
            {
                //Transaction is already opened
                result = Create(objects, UIControlledApplication, document, RevitSettings);
            }

            return result; ;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool Create<T>(IEnumerable<T> objects, UIControlledApplication UIContralledApplication, Document document, RevitSettings revitSettings) where T : IObject
        {
            string tagsParameterName = revitSettings.GeneralSettings.TagsParameterName;

            if (UIContralledApplication != null && revitSettings.GeneralSettings.SuppressFailureMessages)
                UIContralledApplication.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;

            PushSettings pushSettings = new PushSettings()
            {
                AdapterMode = revitSettings.GeneralSettings.AdapterMode,
                CopyCustomData = true,
                FamilyLoadSettings = revitSettings.FamilyLoadSettings

            };

            for (int i = 0; i < objects.Count(); i++)
            {
                IBHoMObject bhomObject = objects.ElementAt<T>(i) as IBHoMObject;

                if (bhomObject == null)
                {
                    NullObjectCreateError(typeof(IBHoMObject));
                    continue;
                }

                if (bhomObject is Bar)
                {
                    ConvertBeforePushError(bhomObject, typeof(BH.oM.Physical.Elements.IFramingElement));
                    continue;
                }
                else if (bhomObject is BH.oM.Structure.Elements.Panel || bhomObject is BH.oM.Environment.Elements.Panel)
                {
                    ConvertBeforePushError(bhomObject, typeof(BH.oM.Physical.Elements.ISurface));
                    continue;
                }

                Element element = null;

                try
                {
                    if (bhomObject is oM.Adapters.Revit.Generic.RevitFilePreview)
                    {
                        oM.Adapters.Revit.Generic.RevitFilePreview revitFilePreview = (oM.Adapters.Revit.Generic.RevitFilePreview)bhomObject;

                        Family family = null;

                        if(revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                        {
                            IEnumerable<FamilySymbol> familySymbols = Query.FamilySymbols(revitFilePreview, document);
                            if (familySymbols != null)
                            {
                                if (familySymbols.Count() > 0)
                                    family = familySymbols.First().Family;

                                foreach (FamilySymbol familySymbol in familySymbols)
                                    document.Delete(familySymbol.Id);
                            }

                            SetIdentifiers(bhomObject, family);

                            IEnumerable<ElementId> elementIDs = family.GetFamilySymbolIds();
                            if (elementIDs == null || elementIDs.Count() == 0)
                                document.Delete(family.Id);
                        }
                        else
                        {
                            FamilyLoadOptions familyLoadOptions = new FamilyLoadOptions(revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Update);
                            if (document.LoadFamily(revitFilePreview.Path, out family))
                            {
                                SetIdentifiers(bhomObject, family);
                                element = family;
                            }
                        }
                    }
                    else
                    {
                        string uniqueID = BH.Engine.Adapters.Revit.Query.UniqueId(bhomObject);
                        if (!string.IsNullOrEmpty(uniqueID))
                            element = document.GetElement(uniqueID);

                        if (element == null)
                        {
                            int id = BH.Engine.Adapters.Revit.Query.ElementId(bhomObject);
                            if (id != -1)
                                element = document.GetElement(new ElementId(id));
                        }

                        if (element != null)
                        {
                            if (revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Replace || revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                            {
                                if (element.Pinned)
                                {
                                    DeletePinnedElementError(element);
                                    continue;
                                }

                                document.Delete(element.Id);
                                element = null;
                            }
                        }

                        if (revitSettings.GeneralSettings.AdapterMode == oM.Adapters.Revit.Enums.AdapterMode.Delete)
                            continue;

                        if (element == null)
                        {
                            Type type = bhomObject.GetType();

                            if (type != typeof(BHoMObject))
                            {
                                element = BH.UI.Revit.Engine.Convert.ToRevit(bhomObject as dynamic, document, pushSettings);
                                SetIdentifiers(bhomObject, element);
                            }

                        }
                        else
                        {
                            element = Modify.SetParameters(element, bhomObject);
                            if (element != null && element.Location != null)
                            {
                                try
                                {
                                    Location location = Modify.Move(element, bhomObject, pushSettings);
                                }
                                catch
                                {
                                    ObjectNotMovedWarning(bhomObject);
                                }

                            }

                            if (bhomObject is IView || bhomObject is oM.Adapters.Revit.Elements.Family || bhomObject is InstanceProperties)
                                element.Name = bhomObject.Name;
                        }
                    }
                }
                catch
                {
                    ObjectNotCreatedCreateError(bhomObject);
                    element = null;
                }

                //Assign Tags
                if (element != null && !string.IsNullOrEmpty(tagsParameterName))
                    Modify.SetTags(element, bhomObject, tagsParameterName);
            }

            if (UIContralledApplication != null)
                UIContralledApplication.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;

            return true;
        }

        /***************************************************/

        private static void ControlledApplication_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            bool hasFailure = false;
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            List<FailureMessageAccessor> failureMessageAccessorsList = failuresAccessor.GetFailureMessages().ToList();
            List<ElementId> elementsToDelete = new List<ElementId>();
            foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessorsList)
            {
                try
                {
                    if (failureMessageAccessor.GetSeverity() == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failureMessageAccessor);
                        continue;
                    }
                    else
                    {
                        failuresAccessor.ResolveFailure(failureMessageAccessor);
                        hasFailure = true;
                        continue;
                    }

                }
                catch
                {
                }
            }

            if (elementsToDelete.Count > 0)
                failuresAccessor.DeleteElements(elementsToDelete);

            if (hasFailure)
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);

            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        /***************************************************/

        private static void SetIdentifiers(IBHoMObject bHoMObject, Element element)
        {
            if (bHoMObject == null || element == null)
                return;

            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.ElementId, element.Id.IntegerValue);
            SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.AdapterId, element.UniqueId);

            if (element is Family)
            {
                Family family = (Family)element;

                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyPlacementTypeName, Query.FamilyPlacementTypeName(family));
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, family.Name);
                if (family.FamilyCategory != null)
                    SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, family.FamilyCategory.Name);
            }
            else
            {
                int worksetID = WorksetId.InvalidWorksetId.IntegerValue;
                if (element.Document != null && element.Document.IsWorkshared)
                {
                    WorksetId revitWorksetID = element.WorksetId;
                    if (revitWorksetID != null)
                        worksetID = revitWorksetID.IntegerValue;
                }
                SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.WorksetId, worksetID);

                Parameter parameter = null;

                parameter = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyName, value);
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.FamilyTypeName, value);
                }


                parameter = element.get_Parameter(BuiltInParameter.ELEM_CATEGORY_PARAM);
                if (parameter != null)
                {
                    string value = parameter.AsValueString();
                    if (!string.IsNullOrEmpty(value))
                        SetCustomData(bHoMObject, BH.Engine.Adapters.Revit.Convert.CategoryName, value);
                }
            }

        }

        /***************************************************/

        private static void SetCustomData(IBHoMObject bHoMObject, string customDataName, object value)
        {
            if (bHoMObject == null || string.IsNullOrEmpty(customDataName))
                return;

            bHoMObject.CustomData[customDataName] = value;
        }

        /***************************************************/
        /****              Private Classes              ****/
        /***************************************************/

        private class FamilyLoadOptions : IFamilyLoadOptions
        {
            private bool m_Update;

            public FamilyLoadOptions(bool update)
            {
                this.m_Update = update;
            }

            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    return false;

                }

                overwriteParameterValues = true;
                return true;
            }

            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                if (m_Update)
                {
                    overwriteParameterValues = false;
                    source = FamilySource.Project;
                    return false;

                }

                overwriteParameterValues = true;
                source = FamilySource.Family;
                return true;
            }
        }

        /***************************************************/
    }
}
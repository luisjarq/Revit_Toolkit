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

using BH.oM.Geometry;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Structure.Elements;
using System;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public class WarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
            {
                IList<FailureMessageAccessor> failures = a.GetFailureMessages();

                foreach (FailureMessageAccessor f in failures)
                {
                    a.DeleteWarning(f);
                }
                return FailureProcessingResult.Continue;
            }
        }

        internal static AssemblyInstance ToLydiaTruss(this CustomObject truss, Document document)
        {
            PushSettings pushSettings = null;
            pushSettings.DefaultIfNull();
            
            Level level = new FilteredElementCollector(document)
                    .WherePasses(new ElementClassFilter(typeof(Level), false))
                    .Cast<Level>()
                    .Where(e => e.Name == "Level 1").First() as Level;

            IEnumerable<FramingElement> topChords = (truss.CustomData["TopChords"] as List<object>).Cast<FramingElement>();
            IEnumerable<FramingElement> bottomChords = (truss.CustomData["BottomChords"] as List<object>).Cast<FramingElement>();
            IEnumerable<FramingElement> diagonals = (truss.CustomData["Diagonals"] as List<object>).Cast<FramingElement>();
            IEnumerable<FramingElement> verticals = (truss.CustomData["Verticals"] as List<object>).Cast<FramingElement>();
            List<Vector> translations = (truss.CustomData["CopyTranslations"] as List<object>).Cast<Vector>().ToList();
            List<double> rotations = (truss.CustomData["CopyRotations"] as List<object>).Cast<double>().ToList();

            List<FamilyInstance> bars = new List<FamilyInstance>();
            using (Transaction aTransaction = new Transaction(document, "Create bars"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                foreach (FramingElement bar in topChords)
                {
                    FamilyInstance revitBar = bar.ToLydiaBar(document, level,Autodesk.Revit.DB.Structure.ZJustification.Center, pushSettings);
                    bars.Add(revitBar);
                }
                foreach (FramingElement bar in bottomChords)
                {
                    FamilyInstance revitBar = bar.ToLydiaBar(document, level, Autodesk.Revit.DB.Structure.ZJustification.Center, pushSettings);
                    bars.Add(revitBar);
                }
                foreach (FramingElement bar in diagonals)
                {
                    FamilyInstance revitBar = bar.ToLydiaBar(document, level, Autodesk.Revit.DB.Structure.ZJustification.Center, pushSettings);
                    bars.Add(revitBar);
                }
                foreach (FramingElement bar in verticals)
                {
                    FamilyInstance revitBar = bar.ToLydiaBar(document, level, Autodesk.Revit.DB.Structure.ZJustification.Center, pushSettings);
                    bars.Add(revitBar);
                }

                aTransaction.Commit();
            }

            using (Transaction aTransaction = new Transaction(document, "Disable analytical model"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                foreach (FamilyInstance bar in bars)
                {
                    Parameter param = bar.get_Parameter(BuiltInParameter.STRUCTURAL_ANALYTICAL_MODEL);
                    param.Set(0);
                }

                aTransaction.Commit();
            }

            AssemblyInstance originalTruss;
            using (Transaction aTransaction = new Transaction(document, "Create original assembly"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                originalTruss = AssemblyInstance.Create(document, bars.Select(x => x.Id).ToList(), document.GetElement(bars[0].Id).Category.Id);

                aTransaction.Commit();
            }

            using (Transaction aTransaction = new Transaction(document, "Modify original assembly"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                char suffix = 'A';

                bool accepted = false;
                while (!accepted)
                {
                    try
                    {
                        originalTruss.AssemblyTypeName = truss.Name + " Type " + suffix;
                        accepted = true;
                    }
                    catch
                    {
                        if (suffix == 'Z')
                            break;

                        suffix++;
                    }
                }

                aTransaction.Commit();
            }

            using (Transaction aTransaction = new Transaction(document, "Copy assembly"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                XYZ originalStartPoint = (topChords.First().LocationCurve.ToRevitCurve() as Autodesk.Revit.DB.Line).GetEndPoint(0);
                for (int i = 0; i < translations.Count; i++)
                {
                    XYZ translation = translations[i].ToRevit();
                    ElementId newTruss = ElementTransformUtils.CopyElement(document, originalTruss.Id, translation).First();
                    ElementTransformUtils.RotateElement(document, newTruss, Autodesk.Revit.DB.Line.CreateUnbound(originalStartPoint + translation, new XYZ(0, 0, 1)), rotations[i]);
                }

                aTransaction.Commit();
            }
            
            using (Transaction aTransaction = new Transaction(document, "Delete original truss"))
            {
                aTransaction.Start();
                FailureHandlingOptions options = aTransaction.GetFailureHandlingOptions();
                WarningSwallower preproccessor = new WarningSwallower();
                options.SetFailuresPreprocessor(preproccessor);
                aTransaction.SetFailureHandlingOptions(options);

                document.Delete(originalTruss.Id);

                aTransaction.Commit();
            }


            return null;
        }

        /***************************************************/

        internal static FamilyInstance ToLydiaBar(this FramingElement framingElement, Document document, Level level, Autodesk.Revit.DB.Structure.ZJustification justification, PushSettings pushSettings)
        {
            Curve aCurve = framingElement.LocationCurve.ToRevitCurve(pushSettings);
            FamilySymbol aFamilySymbol = new FilteredElementCollector(document).WhereElementIsElementType().WherePasses(new ElementClassFilter(typeof(FamilySymbol), false)).Where(x => x.Name == framingElement.Property.Name).First() as FamilySymbol;
            if (!aFamilySymbol.IsActive)
                aFamilySymbol.Activate();

            FamilyInstance aFamilyInstance = document.Create.NewFamilyInstance(aCurve, aFamilySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            //set rotation or all 0? remember about radians
            Parameter parameter = aFamilyInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
            parameter.Set((int)justification);

            Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(aFamilyInstance, 0);
            Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(aFamilyInstance, 1);

            aFamilyInstance.CheckIfNullPush(framingElement);
            if (aFamilyInstance == null)
                return null;
            
            return aFamilyInstance;
        }

        /***************************************************/
    }
}
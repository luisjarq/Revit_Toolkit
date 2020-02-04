/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using Autodesk.Revit.UI;

using BH.oM.Adapters.Revit;
using BH.Adapter.Revit;
using BH.Adapter.Socket;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Settings;
using BH.UI.Revit.Adapter;


namespace BH.UI.Revit
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class RevitListener : IExternalApplication
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        public List<IObject> LatestPackage { get; set; }

        /***************************************************/

        public IRequest LatestRequest { get; set; }

        /***************************************************/

        public RevitConfig LatestConfig { get; set; } = null;

        /***************************************************/

        public string LatestTag { get; set; }

        /***************************************************/

        public UIControlledApplication UIControlledApplication = null;

        /***************************************************/

        public KeyValuePair<string, object> LatestKeyValuePair { get; set; }

        /***************************************************/

        public static RevitListener Listener { get; private set; } = null;

        /***************************************************/

        public static RevitSettings AdapterSettings { get; set; } = null;

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public RevitUIAdapter GetAdapter(Document document)
        {
            RevitUIAdapter adapter;

            if (!m_Adapters.TryGetValue(document, out adapter))
            {
                adapter = new RevitUIAdapter(UIControlledApplication, document);
                m_Adapters[document] = adapter;
            }

            if (AdapterSettings != null)
                adapter.RevitSettings = AdapterSettings;

            return adapter;
        }

        /***************************************************/

        public void ReturnData(IEnumerable<object> objs)
        {

            oM.Socket.DataPackage package = new oM.Socket.DataPackage
            {
                Data = objs.ToList(),
                Events = BH.Engine.Reflection.Query.CurrentEvents(),
                Tag = ""
            };

            m_LinkOut.SendData(package);
        }

        /***************************************************/

        public void SetPorts(int inputPort, int outputPort)
        {
            //Not sure if needed
            m_LinkIn.DataObservers -= M_linkIn_DataObservers;

            m_LinkIn = new SocketLink_Tcp(inputPort);
            m_LinkIn.DataObservers += M_linkIn_DataObservers;

            m_LinkOut = new SocketLink_Tcp(outputPort);
        }


        /***************************************************/
        /****           Revit addin methods             ****/
        /***************************************************/

        public Result OnShutdown(UIControlledApplication application)
        {
            //Not sure if needed
            m_LinkIn.DataObservers -= M_linkIn_DataObservers;

            return Result.Succeeded;
        }

        /***************************************************/

        public Result OnStartup(UIControlledApplication uIControlledApplication)
        {
            UIControlledApplication = uIControlledApplication;

            //Make sure all BHoM assemblies are loaded
            BH.Engine.Reflection.Compute.LoadAllAssemblies();

            //Add button to set socket ports
            AddRibbonItems(uIControlledApplication);

            //Define static instance of the listener
            Listener = this;

            //Define push and pull events
            PushEvent pushEvent = new PushEvent();
            m_PushEvent = ExternalEvent.Create(pushEvent);

            PullEvent pullEvent = new PullEvent();
            m_PullEvent = ExternalEvent.Create(pullEvent);

            //empty list for package holding
            LatestPackage = new List<IObject>();

            //Socket link for listening and sending data
            m_LinkIn = new SocketLink_Tcp(14128);
            m_LinkIn.DataObservers += M_linkIn_DataObservers;

            m_LinkOut = new SocketLink_Tcp(14129);

            uIControlledApplication.ViewActivated += Application_ViewActivated;

            return Result.Succeeded;
        }

        private void Application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            RevitAdapter.InternalAdapter = GetAdapter(e.Document);
        }

        /***************************************************/

        private void AddRibbonItems(UIControlledApplication application)
        {
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("RevitListen");

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData("Update Ports", "Update Ports", System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(RevitListener).Namespace + ".UpdatePorts")) as PushButton;
            //aPushButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(TestResource.Test.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            pushButton.ToolTip = "Update the ports that revit is listening on for information from external softwares sending BHoM information";
        }


        /***************************************************/
        /****           Data observer method            ****/
        /***************************************************/

        private void M_linkIn_DataObservers(oM.Socket.DataPackage package)
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            ExternalEvent eve = null;

            lock (m_PackageLock)
            {
                if (package.Data.Count < 1)
                {
                    ReturnData(new List<string> { "Cant handle empty package" });
                    return;
                }

                PackageType packageType = PackageType.ConnectionCheck;

                try
                {
                    packageType = (PackageType)package.Data[0];
                }
                catch
                {
                    ReturnData(new List<string> { "Unrecognized package type" });
                    return;
                }

                switch (packageType)
                {
                    case PackageType.ConnectionCheck:
                        ReturnData(new List<object>());
                        return;
                    case PackageType.Push:
                        if (!CheckPackageSize(package)) return;
                        eve = m_PushEvent;  //Set the event to raise
                        LatestPackage = new List<IObject>();    //Clear the previous package list
                        //Add all objects to the list
                        foreach (object obj in package.Data[1] as IEnumerable<object>)
                        {
                            if (obj is IObject)
                                LatestPackage.Add(obj as IObject);
                        }
                        break;
                    case PackageType.Pull:
                        if (!CheckPackageSize(package)) return;
                        eve = m_PullEvent;
                        LatestRequest = package.Data[1] as IRequest;
                        break;
                    case PackageType.UpdateTags:
                        if (!CheckPackageSize(package)) return;
                        eve = m_UpdateTagsEvent;
                        var tuple = package.Data[1] as Tuple<IRequest, string, object>;
                        LatestRequest = tuple.Item1;
                        LatestKeyValuePair = new KeyValuePair<string, object>(tuple.Item2, tuple.Item3);
                        break;
                    default:
                        ReturnData(new List<string> { "Unrecognized package type" });
                        return;
                }
                
                RevitConfig revitConfig = package.Data[2] as RevitConfig;
                LatestConfig = revitConfig;
                
                AdapterSettings = package.Data[3] as RevitSettings;

                LatestTag = package.Tag;
            }

            eve.Raise();
        }

        /***************************************************/

        private bool CheckPackageSize(oM.Socket.DataPackage package)
        {
            if (package.Data.Count < 4)
            {
                ReturnData(new List<string> { "Invalid Package" });
                return false;
            }
            return true;
        }


        /***************************************************/
        /****             Private fields                ****/
        /***************************************************/

        private SocketLink_Tcp m_LinkIn;
        private SocketLink_Tcp m_LinkOut;
        private ExternalEvent m_PushEvent;
        private ExternalEvent m_PullEvent;
        private ExternalEvent m_UpdateTagsEvent;
        private Dictionary<Document, RevitUIAdapter> m_Adapters = new Dictionary<Document, RevitUIAdapter>();
        
        public object m_PackageLock = new object();

        /***************************************************/
    }
}
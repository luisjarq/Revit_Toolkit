﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using BH.oM.Base;
using BH.UI.Revit.Adapter;

namespace BH.UI.Revit
{
    public class PushEvent : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            lock (RevitListener.Listener.m_packageLock)
            {
                try
                {
                    //Clear the event log
                    Engine.Reflection.Compute.ClearCurrentEvents();

                    //Get instance of listener
                    RevitListener listener = RevitListener.Listener;

                    //Get the revit adapter
                    RevitInternalAdapter adapter = listener.GetAdapter(app.ActiveUIDocument.Document);

                    //Push the data
                    List<IObject> objs = adapter.Push(listener.LatestPackage, listener.LatestTag, listener.LatestConfig);

                    //Clear the lastest package list
                    listener.LatestPackage.Clear();
                    listener.LatestConfig = null;
                    listener.LatestTag = "";

                    //Return the pushed objects
                    listener.ReturnData(objs);
                }
                catch (Exception e)
                {
                    RevitListener.Listener.ReturnData(new List<string> { "Failed to push. Exception from the adapter: " + e.Message });
                }
            }
        }

        public string GetName()
        {
            return "Push event";
        }
    }
}

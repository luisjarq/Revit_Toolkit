﻿using BH.oM.Adapters.Revit.Enums;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static FilterQuery SetPullShell(this FilterQuery filterQuery, bool PullShell)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullShell))
                aFilterQuery.Equalities[Convert.FilterQuery.PullShell] = PullShell;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.PullShell, PullShell);

            return aFilterQuery;
        }

        /***************************************************/
    }
}

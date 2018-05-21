﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow
{
    /// <summary>
    /// MEF Container class for WorkflowAction Componenets
    /// </summary>
    [Obsolete( "Use Rock.Cache.CacheWorkflowTriggers instead" )]
    public class TriggerCache
    {

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="TriggerCache" /> class.
        /// </summary>
        static TriggerCache()
        {
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        [Obsolete( "Use Rock.Cache.CacheWorkflowTriggers.Refresh() method instead" )]
        public static void Refresh()
        {
            CacheWorkflowTriggers.Refresh();
        }

        /// <summary>
        /// Gets a collection of Workflow Triggers for the specified criteria.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Cache.CacheWorkflowTriggers.Triggers() method instead" )]
        public static List<WorkflowTrigger> Triggers( string entityTypeName )
        {
            return CacheWorkflowTriggers.Triggers( entityTypeName );
        }

        /// <summary>
        /// Gets a collection of Workflow Triggers for the specified criteria.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Cache.CacheWorkflowTriggers.Triggers() method instead" )]
        public static List<WorkflowTrigger> Triggers( string entityTypeName, WorkflowTriggerType triggerType )
        {
            return CacheWorkflowTriggers.Triggers( entityTypeName, triggerType );
        }

        #endregion

    }
}
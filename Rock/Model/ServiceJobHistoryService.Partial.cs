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
using System.Linq;
using System.Web.Compilation;

using Quartz;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.ServiceJobHistory"/> entity objects.
    /// </summary>
    public partial class ServiceJobHistoryService
    {
        /// <summary>
        /// Returns a queryable collection of all <see cref="Rock.Model.ServiceJobHistory">jobs history</see>
        /// </summary>
        /// <param name="serviceJobId">The service job identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="stopDateTime">The stop date time.</param>
        /// <returns>A queryable collection of all <see cref="Rock.Model.ServiceJobHistory"/>jobs history</returns>
        public List<ServiceJobHistory> GetServiceJobHistory( int? serviceJobId, DateTime? startDateTime, DateTime? stopDateTime )
        {
            var ServiceJobHistoryQuery = this.AsNoFilter();

            if ( serviceJobId.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.ServiceJobId == serviceJobId );
            }

            if ( startDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StartDateTime >= startDateTime.Value );
            }

            if ( stopDateTime.HasValue )
            {
                ServiceJobHistoryQuery = ServiceJobHistoryQuery.Where( a => a.StopDateTime < stopDateTime.Value );
            }

            return ServiceJobHistoryQuery.OrderBy( a => a.ServiceJobId ).ThenByDescending( a => a.StartDateTime ).ToList();
        }

        public void DeleteMoreThanMax( int serviceJobId )
        {

            int historyCount;
            ServiceJobService serviceJobService = new ServiceJobService( (RockContext)this.Context );
            ServiceJob serviceJob = serviceJobService.Get( serviceJobId );
            historyCount = serviceJob.HistoryCount;

            historyCount = historyCount <= 0 ? historyCount = 100 : historyCount;
            var matchingServiceJobs = this.AsNoFilter().Where( a => a.ServiceJobId == serviceJobId ).OrderByDescending( a => a.StartDateTime );
            var serviceJobsMoreThanMax = matchingServiceJobs.Skip( historyCount );
            foreach ( var job in serviceJobsMoreThanMax )
            {
                this.Delete( job );
            }
        }

        public override void Add( ServiceJobHistory item )
        {
            base.Add( item );
            DeleteMoreThanMax( item.ServiceJobId );
        }
    }
}

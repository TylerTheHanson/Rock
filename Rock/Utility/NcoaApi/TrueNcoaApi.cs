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
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Rock.Utility.NcoaApi
{
    /// <summary>
    /// TrueNCOA API calls
    /// </summary>
    public class TrueNcoaApi
    {
        private string TRUE_NCOA_SERVER = "https://app.testing.truencoa.com"; // "https://app.truencoa.com/api/";
        private int _batchsize = 150;
        private string _username;
        private string _password;
        private RestClient _client = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrueNcoaApi"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public TrueNcoaApi( UsernamePassword usernamePassword )
        {
            _username = usernamePassword.UserName;
            _password = usernamePassword.Password;
            CreateRestClient();
        }

        /// <summary>
        /// Creates the rest client.
        /// </summary>
        private void CreateRestClient()
        {
            _client = new RestClient( TRUE_NCOA_SERVER );
            _client.AddDefaultHeader( "user_name", _username );
            _client.AddDefaultHeader( "password", _password );
            _client.AddDefaultHeader( "Content-Type", "application/x-www-form-urlencoded" );
        }


        public void CreateFile( string fileName, string companyName, out string id )
        {
            id = null;
            try
            {
                // submit for exporting
                var request = new RestRequest( $"api/files/{fileName}/index", Method.POST );
                request.AddParameter( "application/x-www-form-urlencoded", $"caption={companyName}", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    TrueNcoaResponse file = JsonConvert.DeserializeObject<TrueNcoaResponse>( response.Content );
                    id = file.Id;
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Error creating TrueNCOA file", ex );
            }
        }

        /// <summary>
        /// Uploads the addresses.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="id">Name of the file.</param>
        public void UploadAddresses( Dictionary<int, PersonAddressItem> addresses, string id )
        {
            try
            {
                PersonAddressItem[] addressArray = addresses.Values.ToArray();
                StringBuilder data = new StringBuilder();
                for ( int i = 1; i <= addressArray.Length; i++ )
                {
                    PersonAddressItem personAddressItem = addressArray[i - 1];
                    data.AppendFormat( "{0}={1}&", "individual_id", $"{personAddressItem.PersonId}_{personAddressItem.PersonAliasId}_{personAddressItem.FamilyId}" );
                    data.AppendFormat( "{0}={1}&", "individual_first_name", personAddressItem.FirstName );
                    data.AppendFormat( "{0}={1}&", "individual_last_name", personAddressItem.LastName );
                    data.AppendFormat( "{0}={1}&", "address_line_1", personAddressItem.Street1 );
                    data.AppendFormat( "{0}={1}&", "address_line_2", personAddressItem.Street2 );
                    data.AppendFormat( "{0}={1}&", "address_city_name", personAddressItem.City );
                    data.AppendFormat( "{0}={1}&", "address_state_code", personAddressItem.State );
                    data.AppendFormat( "{0}={1}&", "address_postal_code", personAddressItem.PostalCode );
                    data.AppendFormat( "{0}={1}&", "address_country_code", personAddressItem.Country );

                    if ( i % _batchsize == 0 || i == addressArray.Length )
                    {
                        var request = new RestRequest( $"api/files/{id}/records", Method.POST );
                        request.AddParameter( "application/x-www-form-urlencoded", data.ToString().TrimEnd( '&' ), ParameterType.RequestBody );
                        IRestResponse response = _client.Execute( request );
                        if ( response.StatusCode != HttpStatusCode.OK )
                        {
                            throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                            {
                                Content = new StringContent( response.Content )
                            } );
                        }

                        data = new StringBuilder();
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Could not upload address to TrueNCOA", ex );
            }

            try
            {
                var request = new RestRequest( $"api/files/{id}/index", Method.GET );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                TrueNcoaResponse file;
                try
                {
                    file = JsonConvert.DeserializeObject<TrueNcoaResponse>( response.Content );
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                }

                if ( file.Status != "Mapped" )
                {
                    throw new Exception( $"TrueNCOA is not in the correct state: {file.Status}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Could not upload address to TrueNCOA", ex );
            }
        }

        /// <summary>
        /// Creates the report.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        public void CreateReport( string id )
        {
            try
            {
                // submit for processing
                var request = new RestRequest( $"api/files/{id}/index", Method.PATCH );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Could create report on TrueNCOA", ex );
            }
        }

        /// <summary>
        /// Determines whether the report is created.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if the report is created; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReportCreated( string id )
        {
            try
            {
                var request = new RestRequest( $"api/files/{id}/index", Method.GET );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                TrueNcoaResponse file;
                try
                {
                    file = JsonConvert.DeserializeObject<TrueNcoaResponse>( response.Content );
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                }

                if ( file.Status == "Errored" )
                {
                    throw new Exception( "TrueNCOA returned an error creating the report" );
                }

                bool processing = ( file.Status == "Import" || file.Status == "Importing" || file.Status == "Parse" || file.Status == "Parsing" || file.Status == "Report" || file.Status == "Reporting" || file.Status == "Process" || file.Status == "Processing" );
                return !processing;
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Error checking if report is created by TrueNCOA", ex );
            }
        }

        /// <summary>
        /// Creates the report export.
        /// </summary>
        /// <param name="id">Name of the file.</param>
        /// <param name="exportfileid">The export file ID.</param>
        public void CreateReportExport( string id, out string exportfileid )
        {
            exportfileid = null;
            try
            {
                // submit for exporting
                var request = new RestRequest( $"api/files/{id}/index", Method.PATCH );
                request.AddParameter( "application/x-www-form-urlencoded", "status=export", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    TrueNcoaResponse file = JsonConvert.DeserializeObject<TrueNcoaResponse>( response.Content );
                    exportfileid = file.Id;
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Error creating TrueNCOA report", ex );
            }
        }

        /// <summary>
        /// Determines whether the report export is created.
        /// </summary>
        /// <param name="exportfileid">The export file ID.</param>
        /// <returns>
        ///   <c>true</c> if the report export is created; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReportExportCreated( string exportfileid )
        {
            try
            {
                var request = new RestRequest( $"api/files/{exportfileid}/index", Method.GET );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                try
                {
                    TrueNcoaResponse file = JsonConvert.DeserializeObject<TrueNcoaResponse>( response.Content );
                    bool exporting = ( file.Status == "Export" || file.Status == "Exporting" );
                    return !exporting;
                }
                catch
                {
                    throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Error creating TrueNCOA report", ex );
            }
        }

        /// <summary>
        /// Downloads the export.
        /// </summary>
        /// <param name="exportfileid">The export file ID.</param>
        /// <param name="records">The records.</param>
        public void DownloadExport( string exportfileid, out List<TrueNcoaReturnRecord> records )
        {
            records = null;

            try
            {
                var request = new RestRequest( $"api/files/{exportfileid}/records", Method.GET );
                request.AddParameter( "application/x-www-form-urlencoded", "status=submit", ParameterType.RequestBody );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }

                Dictionary<string, object> obj = null;
                try
                {
                    obj = JObject.Parse( response.Content ).ToObject<Dictionary<string, object>>();

                    var recordsjson = (string)obj["Records"].ToString();
                    records = JsonConvert.DeserializeObject<List<TrueNcoaReturnRecord>>( recordsjson );
                    DateTime dt = DateTime.Now;
                    records.ForEach( r => r.NcoaRunDateTime = dt );
                }
                catch
                {
                    if ( obj != null && obj.ContainsKey( "error" ) )
                    {
                        throw new Exception( $"TrueNCOA error response: {obj["error"]}" );
                    }
                    else
                    {
                        throw new Exception( $"Failed to deserialize TrueNCOA response: {response.Content}" );
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Error creating TrueNCOA report", ex );
            }
        }

        /// <summary>
        /// Saves the records.
        /// </summary>
        /// <param name="records">The records.</param>
        /// <param name="fileName">Name of the file.</param>
        public void SaveRecords( List<TrueNcoaReturnRecord> records, string fileName )
        {
            DataTable dtRecords = null;
            string recordsjson = JsonConvert.SerializeObject( records );
            dtRecords = (DataTable)JsonConvert.DeserializeObject( recordsjson, ( typeof( DataTable ) ) );
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dtRecords.Columns.Cast<DataColumn>().Select( column => column.ColumnName );
            sb.AppendLine( string.Join( ",", columnNames ) );
            foreach ( DataRow row in dtRecords.Rows )
            {
                IEnumerable<string> fields = row.ItemArray.Select( field => string.Concat( "\"", field.ToString().Replace( "\"", "\"\"" ), "\"" ) );
                sb.AppendLine( string.Join( ",", fields ) );
            }

            if ( System.IO.File.Exists( fileName ) )
            {
                System.IO.File.Delete( fileName );
            }

            System.IO.File.WriteAllText( fileName, sb.ToString() );
        }
    }
}
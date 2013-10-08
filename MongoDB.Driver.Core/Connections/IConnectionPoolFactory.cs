﻿/* Copyright 2010-2013 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Net;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Creates a <see cref="IConnectionPool"/>.
    /// </summary>
    internal interface IConnectionPoolFactory
    {
        /// <summary>
        /// Creates a connection pool for the specified address.
        /// </summary>
        /// <param name="dnsEndPoint">The DNS end point.</param>
        /// <returns>A connection pool.</returns>
        IConnectionPool Create(DnsEndPoint dnsEndPoint);
    }
}
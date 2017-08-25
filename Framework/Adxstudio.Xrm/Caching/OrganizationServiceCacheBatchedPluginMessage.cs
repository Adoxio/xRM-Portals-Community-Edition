/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Caching
{
    using System.Collections.Generic;
    using Microsoft.Xrm.Client.Services.Messages;

    /// <summary>
    /// Extension of OrganizationServiceCachePluginMessage message
    /// </summary>
    public class OrganizationServiceCacheBatchedPluginMessage : OrganizationServiceCachePluginMessage
    {
        /// <summary>
        /// Batched Messages
        /// </summary>
        public List<OrganizationServiceCachePluginMessage> BatchedPluginMessage;
    }
}

/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Chat.Models.LivePerson
{
    using Newtonsoft.Json;

    /// <summary>
    /// LivePerson extended service information
    /// </summary>
    public class LivePersonService
    {
        /// <summary>
        /// Service Topic
        /// </summary>
        [JsonProperty("topic")]
        public string Topic { get; set; }

        /// <summary>
        /// Service Status
        /// </summary>
        [JsonProperty("status")]
        public int Status { get; set; }

        /// <summary>
        /// Service Category
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>IdService Id</summary>
        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }
    }
}

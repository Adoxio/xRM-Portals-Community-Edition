/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Chat.Models.LivePerson
{
    using Newtonsoft.Json;

    /// <summary>
    /// Live Person Customer Info wrapper
    /// </summary>
    public class LivePersonSdesCustomerInfo
    {
        /// <summary>
        /// Event type
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type = "ctmrinfo";

        /// <summary>
        /// The <see cref="LivePersonCustomerInfo"/>
        /// </summary>
        [JsonProperty("info")]
        public LivePersonCustomerInfo CustomerInfo { get; set; }
    }
}

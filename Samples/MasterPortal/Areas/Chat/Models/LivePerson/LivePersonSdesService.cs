/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Chat.Models.LivePerson
{
    using Newtonsoft.Json;

    /// <summary>
    /// Live Person Service Info wrapper
    /// </summary>
    public class LivePersonSdesService
    {
        /// <summary>
        /// Event type
        /// </summary>
        [JsonProperty("type")]
        public readonly string Type = "service";

        /// <summary>
        /// The <see cref="LivePersonService"/>
        /// </summary>
        [JsonProperty("service")]
        public LivePersonService Service { get; set; }
    }
}

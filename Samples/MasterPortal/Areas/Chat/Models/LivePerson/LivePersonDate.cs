/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Chat.Models.LivePerson
{
    using Newtonsoft.Json;

    /// <summary>
    /// Used to convert date to format expected by LivePerson
    /// </summary>
    public struct LivePersonDate
    {
        /// <summary>
        /// The day of month
        /// </summary>
        [JsonProperty("day")]
        public int Day { get; set; }

        /// <summary>
        /// The month
        /// </summary>
        [JsonProperty("month")]
        public int Month { get; set; }

        /// <summary>
        /// The year
        /// </summary>
        [JsonProperty("year")]
        public int Year { get; set; }
    }
}

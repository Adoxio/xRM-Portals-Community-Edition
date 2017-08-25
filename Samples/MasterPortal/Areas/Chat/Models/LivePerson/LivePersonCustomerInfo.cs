/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Site.Areas.Chat.Models.LivePerson
{
    using Newtonsoft.Json;

    /// <summary>
    /// LivePerson extended customer information
    /// </summary>
    public class LivePersonCustomerInfo
    {
        /// <summary>
        /// Customer status
        /// </summary>
        [JsonProperty("cstatus")]
        public string Status { get; set; }

        /// <summary>
        /// Customer Type
        /// </summary>
        [JsonProperty("ctype")]
        public string Type { get; set; }

        /// <summary>
        /// Customer Id
        /// </summary>
        [JsonProperty("customerId")]
        public string Id { get; set; }

        /// <summary>
        /// Customer's financial balnce
        /// </summary>
        [JsonProperty("balance")]
        public decimal? Balance { get; set; }

        /// <summary>
        /// Social media identifier
        /// </summary>
        [JsonProperty("socialId")]
        public string SocialId { get; set; }

        /// <summary>
        /// Unique device/phone identifier
        /// </summary>
        [JsonProperty("imei")]
        public string Imei { get; set; }

        /// <summary>
        /// Customer's user name
        /// </summary>
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Number of company employees
        /// </summary>
        [JsonProperty("companySize")]
        public int? CompanySize { get; set; }

        /// <summary>
        /// Customer's company name
        /// </summary>
        [JsonProperty("accountName")]
        public string AccountName { get; set; }

        /// <summary>
        /// Customer's title
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// Last payment date
        /// </summary>
        [JsonProperty("lastPaymentDate")]
        public LivePersonDate LastPaymentDate { get; set; }

        /// <summary>
        /// Registration date
        /// </summary>
        [JsonProperty("registrationDate")]
        public LivePersonDate RegistrationDate { get; set; }
    }
}

/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Services
{
	using System;
	using System.Runtime.Serialization;
	using Microsoft.Xrm.Sdk;
	using Microsoft.Xrm.Sdk.Messages;
	using Microsoft.Xrm.Sdk.Query;
	using Adxstudio.Xrm.Services.Query;
	using Newtonsoft.Json;

	/// <summary>
	/// A customized <see cref="RetrieveSingleRequest"/> wrapper for storing the original <see cref="Fetch"/> query object.
	/// </summary>
	[Serializable]
	[DataContract(Name = "RetrieveMultipleRequest", Namespace = "http://schemas.microsoft.com/xrm/2011/Contracts")]
	[KnownType(typeof(FetchMultipleRequest))]
	internal sealed class FetchMultipleRequest : OrganizationRequest
	{
		/// <summary>
		/// The wrapped request.
		/// </summary>
		[DataMember]
		public RetrieveMultipleRequest Request { get; private set; }

		/// <summary>
		/// The fetch query.
		/// </summary>
		[DataMember]
		public Fetch Fetch { get; private set; }

		/// <summary>
		/// The query.
		/// </summary>
		public QueryBase Query
		{
			get { return this.Request.Query; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchMultipleRequest" /> class.
		/// </summary>
		/// <param name="fetch">The fetch query.</param>
		public FetchMultipleRequest(Fetch fetch)
			: this(fetch.ToFetchExpression())
		{
			this.Fetch = fetch;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchMultipleRequest" /> class.
		/// </summary>
		/// <param name="query">The query.</param>
		private FetchMultipleRequest(QueryBase query)
			: this(new RetrieveMultipleRequest { Query = query })
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FetchMultipleRequest" /> class.
		/// </summary>
		/// <param name="request">The wrapped request.</param>
		private FetchMultipleRequest(RetrieveMultipleRequest request)
		{
			this.Request = request;
			this.ExtensionData = request.ExtensionData;
			this.Parameters = request.Parameters;
			this.RequestId = request.RequestId;
			this.RequestName = request.RequestName;
		}

		/// <summary>
		/// Prevents a default instance of the <see cref="FetchMultipleRequest" /> class from being created.
		/// </summary>
		/// <remarks>
		/// Required for json deserialization.
		/// </remarks>
		[JsonConstructor]
		private FetchMultipleRequest()
		{
		}
	}
}

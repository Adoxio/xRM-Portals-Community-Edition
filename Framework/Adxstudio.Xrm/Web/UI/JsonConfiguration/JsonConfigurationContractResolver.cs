/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Serialization;

namespace Adxstudio.Xrm.Web.UI.JsonConfiguration
{
	/// <summary>
	/// Custom <see cref="IContractResolver"/> for serialization and deserialization of JSON configuration.
	/// </summary>
	public class JsonConfigurationContractResolver : DefaultContractResolver
	{
		private static readonly IContractResolver _instance = new JsonConfigurationContractResolver();

		public static IContractResolver Instance
		{
			get { return _instance; }
		}

		protected override JsonObjectContract CreateObjectContract(Type objectType)
		{
			return objectType == typeof(EntityReference)
				? CreateEntityReferenceObjectContract(objectType)
				: base.CreateObjectContract(objectType);
		}

		private static readonly string[] EntityReferencePropertyWhitelist = new string[] {
			"Id",
			"LogicalName",
			"Name"
		};

		private JsonObjectContract CreateEntityReferenceObjectContract(Type objectType)
		{
			var contract = base.CreateObjectContract(objectType);

			var propertiesToRemove = contract.Properties
				.Where(e => !EntityReferencePropertyWhitelist.Contains(e.PropertyName, StringComparer.Ordinal))
				.ToArray();

			foreach (var property in propertiesToRemove)
			{
				contract.Properties.Remove(property);
			}

			return contract;
		}
	}
}

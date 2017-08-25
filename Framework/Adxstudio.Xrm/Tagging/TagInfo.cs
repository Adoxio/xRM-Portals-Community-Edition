/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;

namespace Adxstudio.Xrm.Tagging
{
	/// <summary>
	/// Represents the base class of entity wrappers of tagging values.
	/// </summary>
	public abstract class TagInfo : ITagInfo
	{
		public static readonly IEqualityComparer<string> TagComparer = StringComparer.CurrentCultureIgnoreCase;

		private readonly Entity _crmEntity;
		private readonly string _tagNamePropertyName;
		private readonly string _taggedItemAssociationSetName;

		protected TagInfo(string tagNamePropertyName, string taggedItemAssociationSetName, Entity crmEntity)
		{
			_crmEntity = crmEntity;
			_tagNamePropertyName = tagNamePropertyName;
			_taggedItemAssociationSetName = taggedItemAssociationSetName;
		}

		/// <summary>
		/// Retrieves the underlying entity.
		/// </summary>
		/// <returns></returns>
		public Entity ToCrmEntity()
		{
			return _crmEntity;
		}

		/// <summary>
		/// Gets the name of this tag.
		/// </summary>
		public string Name
		{
			get { return _crmEntity.GetAttributeValue<string>(_tagNamePropertyName); }
		}

		/// <summary>
		/// Gets the number of items that are associated with this tag.
		/// </summary>
		public int TaggedItemCount
		{
			get
			{
				var taggedItems = _crmEntity.GetRelatedEntities(PortalContext.Current.ServiceContext, _taggedItemAssociationSetName);
				return taggedItems.Count();
			}
		}
	}
}

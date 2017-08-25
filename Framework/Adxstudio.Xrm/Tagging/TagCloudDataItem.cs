/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Tagging
{
	public class TagCloudDataItem : ITagInfo
	{
		/// <summary>
		/// Gets or sets a (generally weight-based) CSS class for this item.
		/// </summary>
		public string CssClass { get; set; }

		/// <summary>
		/// Gets or sets the name of this tag.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the number of items that are associated with this tag.
		/// </summary>
		public int TaggedItemCount { get; set; }

		/// <summary>
		/// Gets or sets the assigned weight of this data item in its tag cloud.
		/// </summary>
		/// <see cref="TagCloudData"/>
		public int Weight { get; set; }
	}
}

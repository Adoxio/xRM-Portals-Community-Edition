/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Tagging
{
	/// <summary>
	/// Minimal information about a tag, providing the its name, and the count of items
	/// associated with it.
	/// </summary>
	public interface ITagInfo
	{
		/// <summary>
		/// Gets the name of this tag.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the number of items that are associated with this tag.
		/// </summary>
		int TaggedItemCount { get; }
	}
}

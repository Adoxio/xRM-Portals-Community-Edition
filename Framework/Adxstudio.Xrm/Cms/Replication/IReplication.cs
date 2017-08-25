/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Replication Interface
	/// </summary>
	public interface IReplication
	{
		/// <summary>
		/// Created Event
		/// </summary>
		void Created();

		/// <summary>
		/// Deleted Event
		/// </summary>
		void Deleted();

		/// <summary>
		/// Updated Event
		/// </summary>
		void Updated();
	}
}

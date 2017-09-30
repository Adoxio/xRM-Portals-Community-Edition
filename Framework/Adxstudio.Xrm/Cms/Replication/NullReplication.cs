/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Cms.Replication
{
	/// <summary>
	/// Null entity replication
	/// </summary>
	public class NullReplication : IReplication
	{
		/// <summary>
		/// Created Event
		/// </summary>
		public void Created() { }

		/// <summary>
		/// Deleted Event
		/// </summary>
		public void Deleted() { }

		/// <summary>
		/// Updated Event
		/// </summary>
		public void Updated() { }
	}
}

/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Web
{
	/// <summary>
	/// Url mapping result.
	/// </summary>
	/// <typeparam name="T">Mapping type.</typeparam>
	public sealed class UrlMappingResult<T>
		where T : class
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UrlMappingResult{T}"/> class.
		/// </summary>
		/// <param name="node">Found node or null.</param>
		/// <param name="isUnique">Unique state.</param>
		private UrlMappingResult(T node, bool isUnique)
		{
			this.Node = node;
			this.IsUnique = isUnique;
		}

		/// <summary>
		/// Found node.
		/// </summary>
		public T Node { get; private set; }
	
		/// <summary>
		/// Specifies that result is unique.
		/// </summary>
		public bool IsUnique { get; private set; }

		/// <summary>
		/// Create result with duplicate state.
		/// </summary>
		/// <param name="node">Found node.</param>
		/// <returns>Duplicate mapping result.</returns>
		public static UrlMappingResult<T> DuplicateResult(T node)
		{
			return new UrlMappingResult<T>(node, false);
		}

		/// <summary>
		/// Creates unique result.
		/// </summary>
		/// <param name="node">Found node or null.</param>
		/// <returns>Unique mapping result.</returns>
		public static UrlMappingResult<T> MatchResult(T node)
		{
			return new UrlMappingResult<T>(node, true);
		}
	}
}

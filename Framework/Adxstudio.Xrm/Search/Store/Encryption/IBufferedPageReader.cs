/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;

	/// <summary>
	/// The BufferedPageReader interface.
	/// </summary>
	public interface IBufferedPageReader : IDisposable, ICloneable
	{
		/// <summary>
		/// Gets the length.
		/// </summary>
		long Length { get; }

		/// <summary>
		/// Gets the position.
		/// </summary>
		long Position { get; }

		/// <summary>
		/// The read byte.
		/// </summary>
		/// <returns>
		/// The <see cref="byte"/>.
		/// </returns>
		byte ReadByte();

		/// <summary>
		/// The read bytes.
		/// </summary>
		/// <param name="destination">
		/// The destination.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		void ReadBytes(byte[] destination, int offset, int length);

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void Seek(long position);
	}
}

/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;

	/// <summary>
	/// The BufferedPageWriter interface.
	/// </summary>
	public interface IBufferedPageWriter : IDisposable
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
		/// The flush.
		/// </summary>
		void Flush();

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void Seek(long position);

		/// <summary>
		/// The write byte.
		/// </summary>
		/// <param name="byte">
		/// The byte.
		/// </param>
		void WriteByte(byte @byte);

		/// <summary>
		/// The write bytes.
		/// </summary>
		/// <param name="source">
		/// The source.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		void WriteBytes(byte[] source, int offset, int length);
	}
}

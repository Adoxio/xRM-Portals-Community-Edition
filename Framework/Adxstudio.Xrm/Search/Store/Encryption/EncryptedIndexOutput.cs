/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using Lucene.Net.Store;

	/// <summary>
	/// The encrypted index output.
	/// </summary>
	public class EncryptedIndexOutput : IndexOutput
	{
		/// <summary>
		/// The writer.
		/// </summary>
		private readonly IBufferedPageWriter writer;

		/// <summary>
		/// Initializes a new instance of the <see cref="EncryptedIndexOutput"/> class.
		/// </summary>
		/// <param name="writer">
		/// The writer.
		/// </param>
		public EncryptedIndexOutput(IBufferedPageWriter writer)
		{
			this.writer = writer;
		}

		/// <summary>
		/// Gets the file pointer.
		/// </summary>
		public override long FilePointer
		{
			get
			{
				return this.writer.Position;
			}
		}

		/// <summary>
		/// Gets the length.
		/// </summary>
		public override long Length
		{
			get
			{
				return this.writer.Length;
			}
		}

		/// <summary>
		/// The flush.
		/// </summary>
		public override void Flush()
		{
			this.writer.Flush();
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="pos">
		/// The pos.
		/// </param>
		public override void Seek(long pos)
		{
			this.writer.Seek(pos);
		}

		/// <summary>
		/// The write byte.
		/// </summary>
		/// <param name="b">
		/// The b.
		/// </param>
		public override void WriteByte(byte b)
		{
			this.writer.WriteByte(b);
		}

		/// <summary>
		/// The write bytes.
		/// </summary>
		/// <param name="b">
		/// The b.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="length">
		/// The length.
		/// </param>
		public override void WriteBytes(byte[] b, int offset, int length)
		{
			this.writer.WriteBytes(b, offset, length);
		}

		/// <summary>
		/// The dispose.
		/// </summary>
		/// <param name="disposing">
		/// The disposing.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			this.writer.Dispose();
		}
	}
}

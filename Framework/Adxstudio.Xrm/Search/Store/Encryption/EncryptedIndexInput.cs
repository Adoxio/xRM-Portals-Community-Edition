/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using Lucene.Net.Store;

	/// <summary>
	/// The encrypted index input.
	/// </summary>
	public class EncryptedIndexInput : IndexInput
	{
		/// <summary>
		/// The reader.
		/// </summary>
		private readonly IBufferedPageReader reader;

		/// <summary>
		/// Initializes a new instance of the <see cref="EncryptedIndexInput"/> class.
		/// </summary>
		/// <param name="reader">
		/// The reader.
		/// </param>
		public EncryptedIndexInput(IBufferedPageReader reader)
		{
			this.reader = reader;
		}

		/// <summary>
		/// Gets the file pointer.
		/// </summary>
		public override long FilePointer
		{
			get
			{
				return this.reader.Position;
			}
		}

		/// <summary>
		/// The length.
		/// </summary>
		/// <returns>
		/// The <see cref="long"/>.
		/// </returns>
		public override long Length()
		{
			return this.reader.Length;
		}

		/// <summary>
		/// The read byte.
		/// </summary>
		/// <returns>
		/// The <see cref="byte"/>.
		/// </returns>
		public override byte ReadByte()
		{
			return this.reader.ReadByte();
		}

		/// <summary>
		/// The read bytes.
		/// </summary>
		/// <param name="b">
		/// The b.
		/// </param>
		/// <param name="offset">
		/// The offset.
		/// </param>
		/// <param name="len">
		/// The len.
		/// </param>
		public override void ReadBytes(byte[] b, int offset, int len)
		{
			this.reader.ReadBytes(b, offset, len);
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="pos">
		/// The pos.
		/// </param>
		public override void Seek(long pos)
		{
			this.reader.Seek(pos);
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

			this.reader.Dispose();
		}

		/// <summary>
		/// Clones instance
		/// </summary>
		/// <returns>copy of instance</returns>
		public override object Clone()
		{
			var newReader = (IBufferedPageReader)this.reader.Clone();

			return new EncryptedIndexInput(newReader);
		}
	}
}

/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.Linq;

	/// <summary>
	/// The buffered page reader writer.
	/// </summary>
	public abstract class BufferedPageReaderWriter : IBufferedPageReader, IBufferedPageWriter
	{
		/// <summary>
		/// The buffer pointer.
		/// </summary>
		protected int bufferPointer;

		/// <summary>
		/// The file pointer.
		/// </summary>
		protected long filePointer;

		/// <summary>
		/// The file size.
		/// </summary>
		protected long fileSize;

		/// <summary>
		/// The page buffer.
		/// </summary>
		protected byte[] pageBuffer;

		/// <summary>
		/// The page is changed.
		/// </summary>
		protected bool pageIsChanged;

		/// <summary>
		/// The page number.
		/// </summary>
		protected long pageNumber;

		/// <summary>
		/// Gets the length.
		/// </summary>
		public abstract long Length { get; }

		/// <summary>
		/// Gets the page size.
		/// </summary>
		public abstract int PageSize { get; }

		/// <summary>
		/// Clones the object.
		/// </summary>
		/// <returns>copy of instance</returns>
		public abstract object Clone();

		/// <summary>
		/// Gets the length.
		/// </summary>
		long IBufferedPageReader.Length
		{
			get
			{
				return this.Length;
			}
		}

		/// <summary>
		/// Gets the position.
		/// </summary>
		long IBufferedPageReader.Position
		{
			get
			{
				return this.filePointer;
			}
		}

		/// <summary>
		/// Gets the length.
		/// </summary>
		long IBufferedPageWriter.Length
		{
			get
			{
				return this.Length;
			}
		}

		/// <summary>
		/// Gets the position.
		/// </summary>
		long IBufferedPageWriter.Position
		{
			get
			{
				return this.filePointer;
			}
		}

		/// <summary>
		/// The dispose.
		/// </summary>
		public virtual void Dispose()
		{
			this.Flush();
		}

		/// <summary>
		/// The flush.
		/// </summary>
		public void Flush()
		{
			this.WriteCurrentPage();
		}

		/// <summary>
		/// The read byte.
		/// </summary>
		/// <returns>
		/// The <see cref="byte"/>.
		/// </returns>
		public byte ReadByte()
		{
			if (this.bufferPointer == this.PageSize)
			{
				this.pageNumber++;
				this.bufferPointer = 0;

				this.ReadCurrentPage();
			}

			var @byte = this.pageBuffer[this.bufferPointer++];
			this.filePointer++;

			return @byte;
		}

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
		public void ReadBytes(byte[] destination, int offset, int length)
		{
			// TODO: Optimize with Buffer.BlockCopy
			for (var i = 0; i < length; i++)
			{
				var @byte = this.ReadByte();

				destination[offset + i] = @byte;
			}
		}

		/// <summary>
		/// The read page.
		/// </summary>
		/// <param name="pageNumber">
		/// The page number.
		/// </param>
		/// <param name="destination">
		/// The destination.
		/// </param>
		public abstract void ReadPage(long pageNumber, byte[] destination);

		/// <summary>
		/// The write byte.
		/// </summary>
		/// <param name="byte">
		/// The byte.
		/// </param>
		public void WriteByte(byte @byte)
		{
			if (this.bufferPointer == this.PageSize)
			{
				this.WriteCurrentPage();
				this.bufferPointer = 0;
				this.pageNumber++;
				this.ReadCurrentPage();
			}

			this.pageBuffer[this.bufferPointer++] = @byte;
			this.pageIsChanged = true;

			this.filePointer++;

			if (this.filePointer > this.fileSize)
			{
				this.fileSize = this.filePointer;
			}
		}

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
		public void WriteBytes(byte[] source, int offset, int length)
		{
			var range = source.Skip(offset).Take(length).ToArray();

			foreach (byte @byte in range)
			{
				this.WriteByte(@byte);
			}
		}

		/// <summary>
		/// The write page.
		/// </summary>
		/// <param name="pageNumber">
		/// The page number.
		/// </param>
		/// <param name="source">
		/// The source.
		/// </param>
		public abstract void WritePage(long pageNumber, byte[] source);

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void IBufferedPageReader.Seek(long position)
		{
			this.Seek(position);
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		void IBufferedPageWriter.Seek(long position)
		{
			this.Seek(position);
		}

		/// <summary>
		/// The initialize.
		/// </summary>
		protected void Initialize()
		{
			this.pageBuffer = new byte[this.PageSize];
			this.pageNumber = -1;
			this.Seek(0);
		}

		/// <summary>
		/// The read current page.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Invalid Operation Exception
		/// </exception>
		protected void ReadCurrentPage()
		{
			if (this.pageIsChanged)
			{
				throw new InvalidOperationException("Save page before read");
			}

			if (this.pageNumber * this.PageSize >= this.Length)
			{
				// There are no more data. Fill next page with zeros
				Array.Clear(this.pageBuffer, 0, this.PageSize);
			}
			else
			{
				this.ReadPage(this.pageNumber, this.pageBuffer);
			}

			this.pageIsChanged = false;
		}

		/// <summary>
		/// The seek.
		/// </summary>
		/// <param name="position">
		/// The position.
		/// </param>
		protected void Seek(long position)
		{
			this.Flush();

			var pageNumber = (int)Math.Floor((double)position / this.PageSize);

			this.filePointer = position;
			this.bufferPointer = (int)(position - (pageNumber * this.PageSize));

			if (pageNumber == this.pageNumber)
			{
				return;
			}

			this.pageNumber = pageNumber;

			this.ReadCurrentPage();
		}

		/// <summary>
		/// The write current page.
		/// </summary>
		protected void WriteCurrentPage()
		{
			if (!this.pageIsChanged)
			{
				return;
			}

			this.WritePage(this.pageNumber, this.pageBuffer);

			this.pageIsChanged = false;
		}
	}
}

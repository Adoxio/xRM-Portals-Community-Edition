/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography.Pkcs;
	using System.Security.Cryptography.X509Certificates;
	using System.Text;

	/// <summary>
	/// The fs buffered reader writer.
	/// </summary>
	public class FsBufferedReaderWriter : BufferedPageReaderWriter
	{
		/// <summary>
		/// File header marker for file validation and in case of futher file structure changes
		/// </summary>
		private readonly byte[] headerMarker = Encoding.ASCII.GetBytes("ADX");

		/// <summary>
		/// The file.
		/// </summary>
		protected readonly FileInfo File;

		/// <summary>
		/// The file stream.
		/// </summary>
		private readonly Stream fileStream;

		/// <summary>
		/// Is read only mode for file
		/// </summary>
		private readonly bool readOnly;

		/// <summary>
		/// Key header size
		/// </summary>
		private long lengthOffset;

		/// <summary>
		/// Data offset
		/// </summary>
		private long dataOffset;

		/// <summary>
		/// The commited file size.
		/// </summary>
		private long commitedFileSize;

		/// <summary>
		/// The certificate
		/// </summary>
		private X509Certificate2 certificate;

		/// <summary>
		/// Cloned instance indicator
		/// </summary>
		internal bool IsClone;

		/// <summary>
		/// Initializes a new instance of the <see cref="FsBufferedReaderWriter"/> class.
		/// </summary>
		/// <param name="file">
		/// The file.
		/// </param>
		/// <param name="certificate">
		/// The certificate.
		/// </param>
		/// <param name="readOnly">
		/// Ts read only
		/// </param>
		public FsBufferedReaderWriter(FileInfo file, X509Certificate2 certificate, bool readOnly)
		{
			this.File = file;
			this.fileStream = this.File.Open(this.readOnly ? FileMode.Open : FileMode.OpenOrCreate, this.readOnly ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite);

			this.certificate = certificate;
			this.readOnly = readOnly;

			if (this.fileStream.Length <= this.dataOffset && !this.readOnly)
			{
				this.WriteHeader();
			}
			else
			{
				this.ReadHeader();
			}
			
			this.Initialize();
		}

		/// <summary>
		/// Gets the length.
		/// </summary>
		public override long Length
		{
			get
			{
				return this.ReadLength();
			}
		}

		/// <summary>
		/// Gets or sets the page size.
		/// </summary>
		public override int PageSize
		{
			get
			{
				return EncryptedDirectory.PageSize;
			}
		}

		/// <summary>
		/// The dispose.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!this.IsClone && this.fileStream != null)
			{
				this.fileStream.Dispose();
			}
		}

		/// <summary>
		/// Clones instance
		/// </summary>
		/// <returns>copy of instance</returns>
		public override object Clone()
		{
			// Lucene does input reader clone. So we need to handle it properly

			var copy = (FsBufferedReaderWriter)MemberwiseClone();
			copy.IsClone = true;

			copy.pageBuffer = new byte[this.PageSize];
			Array.Copy(this.pageBuffer, copy.pageBuffer, this.PageSize);

			return copy;
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
		public override void ReadPage(long pageNumber, byte[] destination)
		{
			this.fileStream.Seek(this.dataOffset + (pageNumber * (this.PageSize + 500)), SeekOrigin.Begin);
			var readInt = this.ReadInt();

			var encrypted = new byte[readInt];

			this.fileStream.Read(encrypted, 0, readInt);

			this.DecryptData(encrypted, destination);
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
		public override void WritePage(long pageNumber, byte[] source)
		{
			var encrypted = this.EncryptData(source);

			this.fileStream.Seek(this.dataOffset + (pageNumber * (this.PageSize + 500)), SeekOrigin.Begin);

			this.WriteInt(encrypted.Length);

			this.fileStream.Write(encrypted, 0, encrypted.Length);

			// We need to track actual data size as our file has some overheap and also data could be padded to block size
			if (this.fileSize > this.Length)
			{
				this.WriteLength(this.fileSize);
			}
		}

		/// <summary>
		/// The decript data.
		/// </summary>
		/// <param name="source">
		/// The source.
		/// </param>
		/// <param name="destination">
		/// The destination.
		/// </param>
		protected void DecryptData(byte[] source, byte[] destination)
		{
			var envelopedCms = new EnvelopedCms();

			envelopedCms.Decode(source);
			envelopedCms.Decrypt(new X509Certificate2Collection(this.certificate));
			Buffer.BlockCopy(envelopedCms.ContentInfo.Content, 0, destination, 0, envelopedCms.ContentInfo.Content.Length);
		}

		/// <summary>
		/// Encrypts the data.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <returns>encrypted data</returns>
		protected byte[] EncryptData(byte[] source)
		{
			var contentInfo = new ContentInfo(source);
			var envelopedCms = new EnvelopedCms(contentInfo);

			envelopedCms.Encrypt(new CmsRecipient(this.certificate));
			return envelopedCms.Encode();
		}

		/// <summary>
		/// Writes encryption token to file
		/// </summary>
		protected void WriteHeader()
		{
			var fs = this.fileStream;

			// Write header marker
			fs.Seek(0, SeekOrigin.Begin);
			fs.Write(this.headerMarker, 0, this.headerMarker.Length);

			// Write version
			fs.WriteByte(1);

			this.lengthOffset = fs.Position;
			this.dataOffset = this.lengthOffset + sizeof(long);
		}

		/// <summary>
		/// Reads encryption tokens from file 
		/// </summary>
		protected void ReadHeader()
		{
			try
			{
				var fs = this.fileStream;

				// Check header
				byte[] fileMarker = new byte[this.headerMarker.Length];

				fs.Seek(0, SeekOrigin.Begin);
				fs.Read(fileMarker, 0, fileMarker.Length);

				if (!fileMarker.SequenceEqual(this.headerMarker))
				{
					throw new InvalidOperationException(
						string.Format("File {0} does not looks like search index",
						this.File.FullName));
				}

				// Read version
				var version = fs.ReadByte(); // Not in use right now. Just in case for required next changes

				this.lengthOffset = fs.Position;
				this.dataOffset = this.lengthOffset + sizeof(long);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException("Not able to read keys from file", e);
			}
		}

		/// <summary>
		/// The read length.
		/// </summary>
		/// <returns>
		/// The <see cref="long"/>.
		/// </returns>
		protected long ReadLength()
		{
			if (this.commitedFileSize > 0)
			{
				return this.commitedFileSize;
			}

			try
			{
				var raw = new byte[sizeof(long)];

				this.fileStream.Seek(this.lengthOffset, SeekOrigin.Begin);
				this.fileStream.Read(raw, 0, raw.Length);
				var value = BitConverter.ToInt64(raw, 0);

				return value;
			}
			catch (IOException)
			{
				return 0;
			}
		}

		/// <summary>
		/// The write length.
		/// </summary>
		/// <param name="value">
		/// The value.
		/// </param>
		protected void WriteLength(long value)
		{
			var raw = BitConverter.GetBytes(this.fileSize);

			this.fileStream.Seek(this.lengthOffset, SeekOrigin.Begin);
			this.fileStream.Write(raw, 0, raw.Length);

			this.commitedFileSize = this.fileSize;
		}

		/// <summary>
		/// Writes integer value to stream
		/// </summary>
		/// <param name="value">int value</param>
		protected void WriteInt(int value)
		{
			var raw = BitConverter.GetBytes(value);
			this.fileStream.Write(raw, 0, raw.Length);
		}

		/// <summary>
		/// Reads integer value from stream
		/// </summary>
		/// <returns>int value</returns>
		protected int ReadInt()
		{
			var raw = new byte[sizeof(int)];
			this.fileStream.Read(raw, 0, raw.Length);

			var value = BitConverter.ToInt32(raw, 0);

			return value;
		}
	}
}

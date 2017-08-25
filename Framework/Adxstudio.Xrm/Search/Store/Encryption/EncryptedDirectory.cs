/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

namespace Adxstudio.Xrm.Search.Store.Encryption
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography.X509Certificates;

	using Lucene.Net.Store;

	using Directory = Lucene.Net.Store.Directory;

	/// <summary>
	/// The encrypted directory.
	/// </summary>
	public class EncryptedDirectory : Directory
	{
		/// <summary>
		/// Page size for underlying encrypted file content
		/// </summary>
		public const int PageSize = 4096;

		/// <summary>
		/// The directory.
		/// </summary>
		private readonly DirectoryInfo directory;

		/// <summary>
		/// The certificate
		/// </summary>
		private X509Certificate2 certificate;

		/// <summary>
		/// Initializes a new instance of the <see cref="EncryptedDirectory"/> class.
		/// </summary>
		/// <param name="path">
		/// The path.
		/// </param>
		/// <param name="certificate">
		/// The certificate.
		/// </param>
		public EncryptedDirectory(string path, X509Certificate2 certificate)
		{
			this.directory = new DirectoryInfo(path);

			if (!this.directory.Exists)
			{
				this.directory.Create();
			}

			this.interalLockFactory = new NativeFSLockFactory(this.directory.FullName);

			this.certificate = certificate;
		}

		/// <summary>
		/// The create output.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="IndexOutput"/>.
		/// </returns>
		public override IndexOutput CreateOutput(string name)
		{
			var record = this.GetRecord(name);

			if (record.Exists)
			{
				try
				{
					this.DeleteFile(name);
				}
				catch (Exception)
				{
					throw new IOException("Cannot overwrite: " + record);
				}
			}

			IBufferedPageWriter writer = this.GetReaderWriter(record, false);

			return new EncryptedIndexOutput(writer);
		}

		/// <summary>
		/// The delete file.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		public override void DeleteFile(string name)
		{
			var file = this.GetRecord(name);
			try
			{
				file.Delete();
			}
			catch (Exception)
			{
				throw new IOException("Cannot delete " + file);
			}
		}

		/// <summary>
		/// The file exists.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="bool"/>.
		/// </returns>
		public override bool FileExists(string name)
		{
			var record = this.GetRecord(name);

			return record.Exists;
		}

		/// <summary>
		/// The file length.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="long"/>.
		/// </returns>
		public override long FileLength(string name)
		{
			var record = this.GetRecord(name);

			if (!record.Exists)
			{
				return 0;
			}

			using (var reader = this.GetReaderWriter(record, true))
			{
				return reader.Length;
			}
		}

		/// <summary>
		/// The file modified.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="long"/>.
		/// </returns>
		public override long FileModified(string name)
		{
			var record = this.GetRecord(name);

			if (!record.Exists)
			{
				return 0;
			}

			return record.LastWriteTimeUtc.ToFileTimeUtc();
		}

		/// <summary>
		/// The list all.
		/// </summary>
		/// <returns>
		/// The <see cref="string[]"/>.
		/// </returns>
		public override string[] ListAll()
		{
			return
				this.directory.EnumerateFiles()
					.Select(file => file.Name)
					.ToArray();
		}

		/// <summary>
		/// The open input.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="IndexInput"/>.
		/// </returns>
		/// <exception cref="FileNotFoundException">
		/// File Not Found Exception
		/// </exception>
		public override IndexInput OpenInput(string name)
		{
			var record = this.GetRecord(name);

			if (!record.Exists)
			{
				// Lucene does not check whether file exists. It expects FileNotFoundException
				throw new FileNotFoundException("File not found", name);
			}

			IBufferedPageReader reader = this.GetReaderWriter(record, true);

			return new EncryptedIndexInput(reader);
		}

		/// <summary>
		/// The touch file.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		public override void TouchFile(string name)
		{
			var record = this.GetRecord(name);

			if (!record.Exists)
			{
				record.Create();
			}
		}

		/// <summary>
		/// The dispose.
		/// </summary>
		/// <param name="disposing">
		/// The disposing.
		/// </param>
		protected override void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// The get reader writer.
		/// </summary>
		/// <param name="record">
		/// The record.
		/// </param>
		/// <param name="readOnly">
		/// Read only file access or not
		/// </param>
		/// <returns>
		/// The <see cref="FsBufferedReaderWriter"/>.
		/// </returns>
		protected virtual FsBufferedReaderWriter GetReaderWriter(FileInfo record, bool readOnly)
		{
			return new FsBufferedReaderWriter(record, this.certificate, readOnly);
		}

		/// <summary>
		/// The get record.
		/// </summary>
		/// <param name="name">
		/// The name.
		/// </param>
		/// <returns>
		/// The <see cref="FileInfo"/>.
		/// </returns>
		private FileInfo GetRecord(string name)
		{
			return new FileInfo(Path.Combine(this.directory.FullName, name));
		}
	}
}

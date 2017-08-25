/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Threading;
using System.Web;

namespace Adxstudio.Xrm.Notes
{
	internal class CrmAnnotationFile : AnnotationFile, ICrmAnnotationFile
	{
		private Lazy<byte[]> _document;

		public CrmAnnotationFile()
		{
			_document = new Lazy<byte[]>(() => null, LazyThreadSafetyMode.None);
		}

		public CrmAnnotationFile(HttpPostedFileBase file) : base(file)
		{
			_document = new Lazy<byte[]>(() =>
			{
				var fileContent = new byte[file.ContentLength];
				file.InputStream.Read(fileContent, 0, fileContent.Length);
				return fileContent;
			}, LazyThreadSafetyMode.None);
		}

		public CrmAnnotationFile(string fileName, string contentType, byte[] fileContent)
			: base(fileName, contentType, fileContent)
		{
			_document = new Lazy<byte[]>(() => fileContent, LazyThreadSafetyMode.None);
		}

		public byte[] Document
		{
			get { return _document.Value; }
			set { _document = new Lazy<byte[]>(() => value, LazyThreadSafetyMode.None); }
		}

		public void SetDocument(Func<byte[]> func)
		{
			_document = new Lazy<byte[]>(func, LazyThreadSafetyMode.None);
		}
	}
}

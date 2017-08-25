/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;

namespace Adxstudio.Xrm.Search.Index
{
	public class EventedIndexBuilder : ICrmEntityIndexBuilder
	{
		private readonly ICrmEntityIndexBuilder _builder;
		private readonly Action _onBuild;

		public EventedIndexBuilder(ICrmEntityIndexBuilder builder, Action onBuild)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}

			if (onBuild == null)
			{
				throw new ArgumentNullException("onBuild");
			}

			_builder = builder;
			_onBuild = onBuild;
		}

		public void Dispose()
		{
			_builder.Dispose();
		}

		public void BuildIndex()
		{
			_builder.BuildIndex();

			_onBuild();
		}
	}
}

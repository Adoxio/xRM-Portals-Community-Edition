/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Web;
using System.Web.UI;
using Microsoft.Xrm.Portal.Runtime;

namespace Microsoft.Xrm.Portal.Web.UI
{
	public interface IAsyncDataSource : IDataSource
	{
		bool IsAsync { get; }
	}

	public abstract class AsyncDataSourceControl : DataSourceControl, IAsyncDataSource
	{
		private bool _performAsyncDataAccess;

		protected AsyncDataSourceControl()
		{
			_performAsyncDataAccess = true;
		}

		[Category("Behavior")]
		[DefaultValue(true)]
		public virtual bool PerformAsyncDataAccess
		{
			get { return _performAsyncDataAccess; }
			set { _performAsyncDataAccess = value; }
		}

		bool IAsyncDataSource.IsAsync
		{
			get { return _performAsyncDataAccess && Page.IsAsync; }
		}
	}

	public abstract class AsyncDataSourceView : DataSourceView
	{
		private IAsyncDataSource _owner;

		protected AsyncDataSourceView(IAsyncDataSource owner, string viewName)
			: base(owner, viewName)
		{
			_owner = owner;
		}

		protected abstract IAsyncResult BeginExecuteSelect(DataSourceSelectArguments arguments, AsyncCallback asyncCallback, object asyncState);

		protected abstract IEnumerable EndExecuteSelect(IAsyncResult asyncResult);

		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			IAsyncResult ar = BeginExecuteSelect(arguments, null, null);
			IEnumerable data = EndExecuteSelect(ar);

			return data;
		}

		private IAsyncResult OnBeginSelect(object sender, EventArgs e, AsyncCallback asyncCallback, object extraData)
		{
			object[] data = (object[])extraData;
			DataSourceSelectArguments arguments = (DataSourceSelectArguments)data[0];
			DataSourceViewSelectCallback callback = (DataSourceViewSelectCallback)data[1];

			return BeginExecuteSelect(arguments, asyncCallback, callback);
		}

		private void OnEndSelect(IAsyncResult asyncResult)
		{
			IEnumerable data = EndExecuteSelect(asyncResult);
			DataSourceViewSelectCallback callback = (DataSourceViewSelectCallback)asyncResult.AsyncState;

			callback(data);
		}

		public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
		{
			if (_owner.IsAsync)
			{
				System.Web.UI.Page page = ((Control)_owner).Page;
				PageAsyncTask task = new PageAsyncTask(
					new BeginEventHandler(OnBeginSelect),
					new EndEventHandler(OnEndSelect),
					null,
					new object[] { arguments, callback });

				page.RegisterAsyncTask(task);
			}
			else
			{
				base.Select(arguments, callback);
			}
		}

		protected sealed class SynchronousAsyncSelectResult : IAsyncResult
		{
			private object _state;

			private IEnumerable _selectResult;
			private Exception _selectException;

			public SynchronousAsyncSelectResult(IEnumerable selectResult, AsyncCallback asyncCallback, object asyncState)
			{
				_state = asyncState;
				_selectResult = selectResult;

				if (asyncCallback != null)
				{
					asyncCallback(this);
				}
			}

			public SynchronousAsyncSelectResult(Exception selectException, AsyncCallback asyncCallback, object asyncState)
			{
				selectException.ThrowOnNull("selectException");

				_state = asyncState;
				_selectException = selectException;

				if (asyncCallback != null)
				{
					asyncCallback(this);
				}
			}

			public IEnumerable SelectResult
			{
				get
				{
					if (_selectException != null)
					{
						throw _selectException;
					}

					return _selectResult;
				}
			}

			#region Implementation of IAsyncResult

			object IAsyncResult.AsyncState
			{
				get
				{
					return _state;
				}
			}

			WaitHandle IAsyncResult.AsyncWaitHandle
			{
				get
				{
					return null;
				}
			}

			bool IAsyncResult.CompletedSynchronously
			{
				get
				{
					return true;
				}
			}

			bool IAsyncResult.IsCompleted
			{
				get
				{
					return true;
				}
			}

			#endregion
		}
	}
}

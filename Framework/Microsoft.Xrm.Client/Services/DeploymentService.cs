/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Specialized;
using System.ServiceModel.Security;
using System.Threading;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Deployment.Proxy;

namespace Microsoft.Xrm.Client.Services
{
	/// <summary>
	/// An <see cref="IDeploymentService"/> that is constructed from a <see cref="CrmConnection"/>.
	/// </summary>
	public class DeploymentService : IDisposable, IDeploymentService, IInitializable
	{
		private static readonly string _servicePath = "/XRMDeployment/2011/Deployment.svc";
		private readonly InnerDeploymentService _service;

		/// <summary>
		/// The nested proxy service.
		/// </summary>
		public IDeploymentService InnerService
		{
			get { return _service.Value; }
		}

		public DeploymentService(string connectionStringName)
			: this(new CrmConnection(connectionStringName))
		{
		}

		public DeploymentService(CrmConnection connection)
		{
			_service = new InnerDeploymentService(() => ToDeploymentService(connection), connection.Timeout);
		}

		public DeploymentService(IDeploymentService service)
		{
			_service = new InnerDeploymentService(() => service, null);
		}

		/// <summary>
		/// Initializes custom settings.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public virtual void Initialize(string name, NameValueCollection config)
		{
		}

		protected virtual IDeploymentService ToDeploymentService(CrmConnection connection)
		{
			var service = ToDeploymentServiceClient(connection);
			return service;
		}

		protected virtual DeploymentServiceClient ToDeploymentServiceClient(CrmConnection connection)
		{
			var uri = connection.ServiceUri;
			var fullServiceUri = uri.AbsolutePath.EndsWith(_servicePath, StringComparison.OrdinalIgnoreCase)
				? uri
				: new Uri(uri, uri.AbsolutePath.TrimEnd('/') + _servicePath);

			var service = connection.Timeout != null
				? ProxyClientHelper.CreateClient(fullServiceUri, connection.Timeout.Value)
				: ProxyClientHelper.CreateClient(fullServiceUri);

			var clientCredentials = connection.ClientCredentials;

			if (clientCredentials != null)
			{
				service.ClientCredentials.Windows.ClientCredential = clientCredentials.Windows.ClientCredential;
				service.ClientCredentials.UserName.UserName = clientCredentials.UserName.UserName;
				service.ClientCredentials.UserName.Password = clientCredentials.UserName.Password;
			}

			return service;
		}

		#region IDeploymentService Members

		public DeploymentObject Retrieve(DeploymentEntityType entityType, EntityInstanceId id)
		{
			return _service.UsingService(s => s.Retrieve(entityType, id));
		}

		public void Update(DeploymentObject deploymentObject)
		{
			_service.UsingService(s => s.Update(deploymentObject));
		}

		public void Delete(DeploymentEntityType entityType, EntityInstanceId id)
		{
			_service.UsingService(s => s.Delete(entityType, id));
		}

		public DeploymentServiceResponse Execute(DeploymentServiceRequest request)
		{
			return _service.UsingService(s => s.Execute(request));
		}

		public DataCollection<EntityInstanceId> RetrieveAll(DeploymentEntityType entityType)
		{
			return _service.UsingService(s => s.RetrieveAll(entityType));
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			var service = _service.Value as IDisposable;

			if (service != null)
			{
				service.Dispose();
			}
		}

		#endregion

		private class InnerDeploymentService
		{
			private readonly Func<IDeploymentService> _serviceFactory;
			private readonly ReaderWriterLockSlim _serviceLock = new ReaderWriterLockSlim();
			private readonly TimeSpan _serviceLockTimeout;

			private Lazy<IDeploymentService> _service;

			public InnerDeploymentService(Func<IDeploymentService> serviceFactory, TimeSpan? serviceLockTimeout)
			{
				if (serviceFactory == null)
				{
					throw new ArgumentNullException("serviceFactory");
				}

				_serviceFactory = serviceFactory;
				_service = new Lazy<IDeploymentService>(_serviceFactory);
				_serviceLockTimeout = serviceLockTimeout ?? TimeSpan.FromSeconds(30);
			}

			public IDeploymentService Value
			{
				get
				{
					if (!_serviceLock.TryEnterReadLock(_serviceLockTimeout))
					{
						throw new TimeoutException("Failed to acquire read lock on inner service.");
					}

					try
					{
						return _service.Value;
					}
					finally
					{
						_serviceLock.ExitReadLock();
					}
				}
			}

			public void UsingService(Action<IDeploymentService> action)
			{
				if (!_serviceLock.TryEnterReadLock(_serviceLockTimeout))
				{
					throw new TimeoutException("Failed to acquire read lock on inner service.");
				}

				try
				{
					try
					{
						action(_service.Value);
					}
					finally
					{
						_serviceLock.ExitReadLock();
					}
				}
				catch (MessageSecurityException)
				{
					ResetService();

					if (!_serviceLock.TryEnterReadLock(_serviceLockTimeout))
					{
						throw new TimeoutException("Failed to acquire read lock on inner service.");
					}

					try
					{
						action(_service.Value);
					}
					finally
					{
						_serviceLock.ExitReadLock();
					}
				}
			}

			public TResult UsingService<TResult>(Func<IDeploymentService, TResult> action)
			{
				if (!_serviceLock.TryEnterReadLock(_serviceLockTimeout))
				{
					throw new TimeoutException("Failed to acquire read lock on inner service.");
				}

				try
				{
					try
					{
						return action(_service.Value);
					}
					finally
					{
						_serviceLock.ExitReadLock();
					}
				}
				catch (MessageSecurityException)
				{
					ResetService();

					if (!_serviceLock.TryEnterReadLock(_serviceLockTimeout))
					{
						throw new TimeoutException("Failed to acquire read lock on inner service.");
					}

					try
					{
						return action(_service.Value);
					}
					finally
					{
						_serviceLock.ExitReadLock();
					}
				}
			}

			private void ResetService()
			{
				if (!_serviceLock.TryEnterWriteLock(_serviceLockTimeout))
				{
					throw new TimeoutException("Failed to acquire write lock on inner service.");
				}

				try
				{
					_service = new Lazy<IDeploymentService>(_serviceFactory);
				}
				finally
				{
					_serviceLock.ExitWriteLock();
				}
			}
		}
	}
}

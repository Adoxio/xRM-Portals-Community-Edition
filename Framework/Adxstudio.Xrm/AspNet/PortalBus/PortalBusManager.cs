/*
  Copyright (c) Microsoft Corporation. All rights reserved.
  Licensed under the MIT License. See License.txt in the project root for license information.
*/

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Owin;

namespace Adxstudio.Xrm.AspNet.PortalBus
{
	/// <summary>
	/// Manages portal bus subscriptions and sending messages to the portal bus.
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	public static class PortalBusManager<TMessage>
	{
		private static readonly BufferBlock<Tuple<IOwinContext, TMessage>> _buffer;
		private static readonly ConcurrentBag<IPortalBusProvider<TMessage>> _providers;

		static PortalBusManager()
		{
			_buffer = new BufferBlock<Tuple<IOwinContext, TMessage>>();
			_providers = new ConcurrentBag<IPortalBusProvider<TMessage>>();
		}

		public static Task PostAsync(IOwinContext context, TMessage message)
		{
			_buffer.Post(new Tuple<IOwinContext, TMessage>(context, message));
			return Task.FromResult(0);
		}

		public static async Task SendAsync(IOwinContext context, TMessage message)
		{
			foreach (var provider in _providers)
			{
				await provider.SendAsync(context, message).WithCurrentCulture();
			}
		}

		public static void Subscribe(IPortalBusProvider<TMessage> provider)
		{
			var action = new ActionBlock<Tuple<IOwinContext, TMessage>>(pair => provider.SendAsync(pair.Item1, pair.Item2)).AsObserver();
			_buffer.AsObservable().Subscribe(action);
			_providers.Add(provider);
		}
	}
}

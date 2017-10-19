/**
 * Copyright 2017 Plexus Interop Deutsche Bank AG
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Plexus.Interop.Internal.Calls
{
    using Plexus.Interop.Internal.ClientProtocol.Invocations;
    using Plexus.Interop.Transport;
    using System;
    using System.Threading.Tasks;

    internal sealed class ServerStreamingMethodCallHandler<TRequest, TResponse> : IMethodCallHandler
    {
        private readonly ServerStreamingMethodHandler<TRequest, TResponse> _handler;
        private readonly IIncomingInvocationFactory _incomingInvocationFactory;

        public ServerStreamingMethodCallHandler(
            ServerStreamingMethodHandler<TRequest, TResponse> handler, 
            IIncomingInvocationFactory incomingInvocationFactory)
        {
            _handler = handler;
            _incomingInvocationFactory = incomingInvocationFactory;
        }

        public async Task HandleAsync(IncomingInvocationDescriptor info, ITransportChannel channel)
        {
            var invocation = _incomingInvocationFactory.CreateAsync<TRequest, TResponse>(info, channel);
            try
            {
                TRequest request = default;
                while (await invocation.In.WaitReadAvailableAsync().ConfigureAwait(false))
                {
                    while (invocation.In.TryRead(out var item))
                    {
                        request = item;
                    }
                }
                var context = new MethodCallContext(info.Source.ApplicationId, info.Source.ConnectionId);
                await _handler(request, invocation.Out, context).ConfigureAwait(false);
                invocation.Out.TryCompleteWriting();
            }
            catch (Exception ex)
            {
                invocation.Out.TryTerminateWriting(ex);
                throw;
            }
            finally
            {
                await invocation.Completion.ConfigureAwait(false);
            }
        }
    }
}

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
import { ClientsSetup } from "../common/ClientsSetup";
import { ConnectionProvider } from "../common/ConnectionProvider";
import { UnaryServiceHandler } from "./UnaryServiceHandler";
import { BaseEchoTest } from "./BaseEchoTest";
import * as plexus from "../../src/echo/gen/plexus-messages";
import { ClientError } from "@plexus-interop/protocol";
import { expect } from "chai";
import { ServerStreamingHandler } from "./ServerStreamingHandler";

export class ClientConnectivityTests extends BaseEchoTest {

    public constructor(
        private connectionProvider: ConnectionProvider,
        private clientsSetup: ClientsSetup = new ClientsSetup()) {
            super();
    }

    public testAllInvocationClientsReceiveErrorOnClientDisconnect(): Promise<void> {
        const echoRequest = this.clientsSetup.createRequestDto();
        return new Promise<void>(async (resolve, reject) => {
            const handler = new ServerStreamingHandler(async (context, request, hostClient) => {
                try {
                    await this.assertEqual(request, echoRequest);
                    hostClient.next(echoRequest);
                    hostClient.next(echoRequest);
                    hostClient.next(echoRequest);
                    hostClient.complete();
                } catch (error) {
                    console.error("Failed", error);
                    reject(error);
                }
            });
            const [client, server] = await this.clientsSetup.createEchoClients(this.connectionProvider, handler);
            const responses: plexus.plexus.interop.testing.IEchoRequest[] = [];
            
            client.getEchoServiceProxy().serverStreaming(echoRequest, {
                next: (response) => {
                    console.log("Received");
                    responses.push(response);
                },
                complete: async () => {
                    expect(responses.length).is.eq(3);
                    responses.forEach(r => this.assertEqual(r, echoRequest));
                    await this.clientsSetup.disconnect(client, server);
                    resolve();
                },
                error: (e) => {
                    reject(e);
                }
            });
        });
    }
}
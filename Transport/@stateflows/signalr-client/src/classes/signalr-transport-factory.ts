import { IStateflowsClientTransport, IStateflowsClientTransportFactory } from "@stateflows/common";
import { SignalRTransport } from "./signalr-transport";
import { HubConnectionBuilderAction } from "../types/hub-connection-builder-action";

export class SignalRTransportFactory implements IStateflowsClientTransportFactory {
    constructor(
        private url: string,
        private builderAction: HubConnectionBuilderAction
    ) {}

    getTransport(): Promise<IStateflowsClientTransport> {
        return Promise.resolve(new SignalRTransport(this.url, this.builderAction));
    }
}

export function UseSignalR(url: string, builderAction: HubConnectionBuilderAction = null): IStateflowsClientTransportFactory {
    if (builderAction !== null) {
        builderAction = b => b;
    }
    return new SignalRTransportFactory(url, builderAction);
}
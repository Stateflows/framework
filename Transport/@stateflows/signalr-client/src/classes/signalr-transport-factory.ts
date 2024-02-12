import { IStateflowsClientTransport, IStateflowsClientTransportFactory } from "@stateflows/common";
import { SignalRTransport } from "./signalr-transport";

export class SignalRTransportFactory implements IStateflowsClientTransportFactory {
    constructor(private url: string) {}

    getTransport(): Promise<IStateflowsClientTransport> {
        return Promise.resolve(new SignalRTransport(this.url));
    }
}

export function UseSignalR(url: string): IStateflowsClientTransportFactory {
    return new SignalRTransportFactory(url);
}
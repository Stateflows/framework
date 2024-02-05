import { IStateflowsClientTransport, IStateflowsClientTransportFactory } from "@stateflows/client-abstractions";
import { SignalRTransport } from "./signalr-transport";

export class SignalRTransportFactory implements IStateflowsClientTransportFactory {
    constructor(private url: string) {}

    getTransport(): Promise<IStateflowsClientTransport> {
        return Promise.resolve(new SignalRTransport(this.url));
    }
}

export function SignalR(url: string): IStateflowsClientTransportFactory {
    return new SignalRTransportFactory(url);
}
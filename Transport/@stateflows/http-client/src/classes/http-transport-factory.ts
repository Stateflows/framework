import { IStateflowsClientTransport, IStateflowsClientTransportFactory } from "@stateflows/common";
import { HttpTransport } from "./http-transport";

export class HttpTransportFactory implements IStateflowsClientTransportFactory {
    constructor(private url: string) {}

    getTransport(): Promise<IStateflowsClientTransport> {
        return Promise.resolve(new HttpTransport(this.url));
    }
}

export function UseHttp(url: string): IStateflowsClientTransportFactory {
    return new HttpTransportFactory(url);
}
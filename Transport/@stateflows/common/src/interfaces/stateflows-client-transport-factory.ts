import { IStateflowsClientTransport } from "./stateflows-client-transport";

export interface IStateflowsClientTransportFactory {
    getTransport(): Promise<IStateflowsClientTransport>;
}
import { HubConnection } from "@microsoft/signalr";
import { Request } from "../events/request";
import { IBehavior } from "../interfaces/behavior";

export class Behavior implements IBehavior {
    private hubPromise: Promise<HubConnection>;
    
    constructor(hubPromiseOrBehavior: Promise<HubConnection> | Behavior) {
        this.hubPromise = hubPromiseOrBehavior instanceof Behavior
            ? hubPromiseOrBehavior.hubPromise
            : this.hubPromise = hubPromiseOrBehavior;
    }

    send(event: Event): Promise<boolean> {
        throw new Error("Method not implemented.");
    }

    request<TResponse>(request: Request<TResponse>): Promise<TResponse> {
        throw new Error("Method not implemented.");
    }

    initialize(): Promise<boolean> {
        throw new Error("Method not implemented.");
    }

    isInitialized(): Promise<boolean> {
        throw new Error("Method not implemented.");
    }
}
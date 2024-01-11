import { HubConnection } from "@microsoft/signalr";
import { Request } from "../events/request";
import { IBehavior } from "../interfaces/behavior";
import { SendResult } from "../classes/send-result";
import { RequestResult } from "../classes/request-result";
import { InitializationResponse } from "../events/initialization.response";
import { BehaviorStatusResponse } from "../events/behavior-status.response";
import { BehaviorId } from "../ids/behavior.id";
import { Response } from "../events/response";
import { InitializationRequest } from "../events/initialization.request";
import { Event } from "../events/event";
import { BehaviorStatusRequest } from "../events/behavior-status.request";

export class Behavior implements IBehavior {
    private hubPromise: Promise<HubConnection>;
    
    constructor(hubPromiseOrBehavior: Promise<HubConnection> | Behavior, public behaviorId: BehaviorId) {
        this.hubPromise = hubPromiseOrBehavior instanceof Behavior
            ? hubPromiseOrBehavior.hubPromise
            : this.hubPromise = hubPromiseOrBehavior;
    }

    send(event: Event): Promise<SendResult> {
        return new Promise<SendResult>(async (resolve, reject) => {
            let hub = await this.hubPromise;
            let result = await hub.invoke("Send", this.behaviorId, JSON.stringify(event));
            resolve(JSON.parse(result));
        });
    }

    request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>> {
        return new Promise<RequestResult<TResponse>>(async (resolve, reject) => {
            let result = await this.send(request);
            resolve(new RequestResult<TResponse>((result as any).Response as TResponse, result.Event, result.Status, result.Validation));
        });
    }

    initialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>> {
        if (typeof initializationRequest === "undefined") {
            initializationRequest = new InitializationRequest();
        }

        return this.request(initializationRequest);
    }

    getStatus(): Promise<RequestResult<BehaviorStatusResponse>> {
        return this.request(new BehaviorStatusRequest());
    }
}
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
import { IStateflowsClientTransport } from "../interfaces/stateflows-client-transport";

export class Behavior implements IBehavior {
    #transportPromise: Promise<IStateflowsClientTransport>

    constructor(transportPromiseOrBehavior: Promise<IStateflowsClientTransport> | Behavior, public behaviorId: BehaviorId) {
        this.#transportPromise = transportPromiseOrBehavior instanceof Behavior
            ? transportPromiseOrBehavior.#transportPromise
            : this.#transportPromise = transportPromiseOrBehavior;
    }

    send(event: Event): Promise<SendResult> {
        return new Promise<SendResult>(async (resolve, reject) => {
            let hub = await this.#transportPromise;
            let result = await hub.send(this.behaviorId, event);
            resolve(result);
        });
    }

    request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>> {
        return new Promise<RequestResult<TResponse>>(async (resolve, reject) => {
            let sendResult = await this.send(request);
            request.Response = (sendResult.Event as any).Response;
            let result = new RequestResult<TResponse>(request.Response, request, sendResult.Status, sendResult.Validation);
            resolve(result);
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
import { RequestResult } from "../classes/request-result";
import { SendResult } from "../classes/send-result";
import { BehaviorStatusResponse } from "../events/behavior-status.response";
import { InitializationResponse } from "../events/initialization.response";
import { Event } from "../events/event";
import { Request } from "../events/request";
import { Response } from "../events/response";
import { InitializationRequest } from "../events/initialization.request";

export interface IBehavior {
    send(event: Event): Promise<SendResult>;
    request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>>;
    initialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>>;
    watch<TNotification>(handler: Func<TNotification>);
    unwatch<TNotification>();

    getStatus(): Promise<RequestResult<BehaviorStatusResponse>>;
    watchStatus(handler: Func<BehaviorStatusNotification>);
    unwatchStatus();
}
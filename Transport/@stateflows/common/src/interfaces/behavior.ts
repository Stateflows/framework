import { RequestResult } from "../classes/request-result";
import { SendResult } from "../classes/send-result";
import { BehaviorStatusResponse } from "../events/behavior-status.response";
import { BehaviorStatusNotification } from "../events/behavior-status.notification";
import { InitializationResponse } from "../events/initialization.response";
import { FinalizationResponse } from "../events/finalization.response";
import { ResetResponse } from "../events/reset.response";
import { CompoundResponse } from "../events/compound.response";
import { Event } from "../events/event";
import { Request } from "../events/request";
import { Response } from "../events/response";
import { Notification } from "../events/notification";
import { InitializationRequest } from "../events/initialization.request";
import { NotificationHandler } from "../utils/notification-handler";
import { BehaviorId } from "../ids/behavior.id";

export interface IBehavior {
    id: BehaviorId;
    
    send(event: Event): Promise<SendResult>;
    sendCompound(...events: Event[]): Promise<RequestResult<CompoundResponse>>;
    request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>>;
    
    initialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>>;
    finalize(): Promise<RequestResult<FinalizationResponse>>;
    reset(keepVersion?: boolean): Promise<RequestResult<ResetResponse>>;    
    reinitialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>>;
    
    watch<TNotification extends Notification>(notificationName: string, handler: NotificationHandler<TNotification>): Promise<void>;
    unwatch(notificationName: string): Promise<void>;

    getStatus(): Promise<RequestResult<BehaviorStatusResponse>>;
    watchStatus(handler: NotificationHandler<BehaviorStatusNotification>): Promise<void>;
    unwatchStatus(): Promise<void>;
}
import { RequestResult } from "../classes/request-result";
import { SendResult } from "../classes/send-result";
import { CompoundResponse } from "../events/compound.response";
import { NotificationHandler } from "../utils/notification-handler";
import { BehaviorId } from "../ids/behavior.id";
import { ResetMode } from "../enums/reset-mode";
import { Request } from "../events/request";
import { BehaviorInfo } from "../events/behavior-info";
import { EventHeader } from "../classes/event-header";
import { Event } from "../events/event";

export interface IBehavior {
    id: BehaviorId;
    
    send(event: Event, headers?: EventHeader[] | null): Promise<SendResult>;
    sendCompound(...events: Event[]): Promise<RequestResult<CompoundResponse>>;
    request<TResponse>(request: Request<TResponse>, headers?: EventHeader[] | null): Promise<RequestResult<TResponse>>;
    
    finalize(): Promise<SendResult>;
    reset(resetMode?: ResetMode): Promise<SendResult>;
    
    watch<TNotification extends Event>(notificationName: string, handler: NotificationHandler<TNotification>): Promise<void>;
    unwatch(notificationName: string): Promise<void>;

    getStatus(): Promise<RequestResult<BehaviorInfo>>;
    watchStatus(handler: NotificationHandler<BehaviorInfo>): Promise<void>;
    unwatchStatus(): Promise<void>;
}
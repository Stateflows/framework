import { Request } from "../events/request";
import { IBehavior } from "../interfaces/behavior";
import { SendResult } from "../classes/send-result";
import { RequestResult } from "../classes/request-result";
import { BehaviorInfo } from "../events/behavior-info";
import { BehaviorId } from "../ids/behavior.id";
import { CompoundRequest } from "../events/compound.request";
import { CompoundResponse } from "../events/compound.response";
import { BehaviorInfoRequest } from "../events/behavior-info.request";
import { IStateflowsClientTransport } from "../interfaces/stateflows-client-transport";
import { Finalize } from "../events/finalize";
import { Reset } from "../events/reset";
import { IWatcher } from "../interfaces/watcher";
import { NotificationHandler } from "../utils/notification-handler";
import { ResetMode } from "../enums/reset-mode";
import { EventHolder } from "../classes/event-holder";
import { EventHeader } from "../classes/event-header";
import { Event } from "../events/event";

export class Behavior implements IBehavior, IWatcher {
    #transportPromise: Promise<IStateflowsClientTransport>;
    #notificationHandlers: Map<string, Array<NotificationHandler<any>>> = new Map<string, Array<NotificationHandler<any>>>();

    constructor(transportPromiseOrBehavior: Promise<IStateflowsClientTransport> | Behavior, public id: BehaviorId) {
        this.#transportPromise = transportPromiseOrBehavior instanceof Behavior
            ? transportPromiseOrBehavior.#transportPromise
            : this.#transportPromise = transportPromiseOrBehavior;
    }

    async send(event: Event, headers: EventHeader[] = []): Promise<SendResult> {
        let transport = await this.#transportPromise;
        let result = await transport.send(this.id, new EventHolder(event, headers));
        result.event = result.event.payload;
        return result;
    }

    sendCompound(...events: Event[]): Promise<RequestResult<CompoundResponse>> {
        return this.request(new CompoundRequest(events))
    }
    
    async request<TResponse>(request: Request<TResponse>, headers: EventHeader[] = []): Promise<RequestResult<TResponse>> {
        let sendResult = await this.send(request, headers);
        request.response = sendResult.event.response;
        let result = new RequestResult<TResponse>(request.response, request, sendResult.status, sendResult.validation);
        return result;
    }

    finalize(): Promise<SendResult> {
        return this.send(new Finalize());
    }

    reset(resetMode?: ResetMode): Promise<SendResult> {
        return this.send(new Reset(resetMode ?? ResetMode.Full));
    }

    notify(notification: EventHolder): void {
        if (this.#notificationHandlers.has(notification.name))
        {
            notification.payload.eventName = notification.name;
            notification.payload.$type = notification.$type;
            let handlers = this.#notificationHandlers.get(notification.name) as Array<NotificationHandler<any>>;
            handlers.forEach(handler => handler(notification.payload));
        }
    }
    
    async watch<TNotification extends Event>(notificationName: string, handler: NotificationHandler<TNotification>): Promise<void> {
        let transport = await this.#transportPromise;
        await transport.watch(this, notificationName);

        let handlers: Array<NotificationHandler<any>> = this.#notificationHandlers.has(notificationName)
            ? this.#notificationHandlers.get(notificationName) as Array<NotificationHandler<any>>
            : [];

        handlers.push(n => handler(n as TNotification));

        this.#notificationHandlers.set(notificationName, handlers);
    }

    async unwatch(notificationName: string): Promise<void> {
        let transport = await this.#transportPromise;
        await transport.unwatch(this, notificationName);

        this.#notificationHandlers.delete(notificationName);
    }

    getStatus(): Promise<RequestResult<BehaviorInfo>> {
        return this.request(new BehaviorInfoRequest());
    }

    watchStatus(handler: NotificationHandler<BehaviorInfo>): Promise<void> {
        return this.watch<BehaviorInfo>(BehaviorInfo.eventName, handler);
    }

    unwatchStatus(): Promise<void> {
        return this.unwatch(BehaviorInfo.eventName);
    }
}
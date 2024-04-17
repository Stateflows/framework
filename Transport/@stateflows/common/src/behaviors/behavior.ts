import { Request } from "../events/request";
import { IBehavior } from "../interfaces/behavior";
import { SendResult } from "../classes/send-result";
import { RequestResult } from "../classes/request-result";
import { InitializationResponse } from "../events/initialization.response";
import { BehaviorStatusResponse } from "../events/behavior-status.response";
import { BehaviorId } from "../ids/behavior.id";
import { Response } from "../events/response";
import { CompoundRequest } from "../events/compound.request";
import { CompoundResponse } from "../events/compound.response";
import { InitializationRequest } from "../events/initialization.request";
import { Event } from "../events/event";
import { BehaviorStatusRequest } from "../events/behavior-status.request";
import { IStateflowsClientTransport } from "../interfaces/stateflows-client-transport";
import { FinalizationResponse } from "../events/finalization.response";
import { ResetResponse } from "../events/reset.response";
import { FinalizationRequest } from "../events/finalization.request";
import { ResetRequest } from "../events/reset.request";
import { BehaviorStatusNotification } from "../events/behavior-status.notification";
import { Notification } from "../events/notification";
import { IWatcher } from "../interfaces/watcher";
import { NotificationHandler } from "../utils/notification-handler";

export class Behavior implements IBehavior, IWatcher {
    #transportPromise: Promise<IStateflowsClientTransport>;
    #notificationHandlers: Map<string, Array<NotificationHandler<Notification>>> = new Map<string, Array<NotificationHandler<Notification>>>();

    constructor(transportPromiseOrBehavior: Promise<IStateflowsClientTransport> | Behavior, public id: BehaviorId) {
        this.#transportPromise = transportPromiseOrBehavior instanceof Behavior
            ? transportPromiseOrBehavior.#transportPromise
            : this.#transportPromise = transportPromiseOrBehavior;
    }

    async send(event: Event): Promise<SendResult> {
        let transport = await this.#transportPromise;
        let result = await transport.send(this.id, event);
        return result;
    }

    sendCompound(...events: Event[]): Promise<RequestResult<CompoundResponse>> {
        return this.request(new CompoundRequest(events))
    }
    
    async request<TResponse extends Response>(request: Request<TResponse>): Promise<RequestResult<TResponse>> {
        let sendResult = await this.send(request);
        request.response = (sendResult.event as any).response;
        let result = new RequestResult<TResponse>(request.response, request, sendResult.status, sendResult.validation);
        return result;
    }

    initialize(initializationRequest?: InitializationRequest): Promise<RequestResult<InitializationResponse>> {
        if (typeof initializationRequest === "undefined") {
            initializationRequest = new InitializationRequest();
        }

        return this.request(initializationRequest);
    }

    finalize(): Promise<RequestResult<FinalizationResponse>> {
        return this.request(new FinalizationRequest());
    }

    reset(keepVersion?: boolean): Promise<RequestResult<ResetResponse>> {
        return this.request(new ResetRequest(keepVersion ?? false));
    }

    async reinitialize(initializationRequest?: InitializationRequest, keepVersion: boolean = true): Promise<RequestResult<InitializationResponse>> {
        if (typeof initializationRequest === "undefined") {
            initializationRequest = new InitializationRequest();
        }
        
        let result = await this.sendCompound(
            new ResetRequest(keepVersion ?? true),
            initializationRequest
        );

        let initializationResult = result.response.results.slice(-1)[0];

        return new RequestResult<InitializationResponse>(
            initializationResult.response as InitializationResponse,
            initializationRequest,
            initializationResult.status,
            initializationResult.validation
        );
    }

    notify(notification: Notification): void {
        if (this.#notificationHandlers.has(notification.name))
        {
            let handlers = this.#notificationHandlers.get(notification.name) as Array<NotificationHandler<Notification>>;
            handlers.forEach(handler => handler(notification));
        }
    }
    
    async watch<TNotification extends Notification>(notificationName: string, handler: NotificationHandler<TNotification>): Promise<void> {
        let transport = await this.#transportPromise;
        await transport.watch(this, notificationName);

        let handlers: Array<NotificationHandler<Notification>> = this.#notificationHandlers.has(notificationName)
            ? this.#notificationHandlers.get(notificationName) as Array<NotificationHandler<Notification>>
            : [];

        handlers.push(n => handler(n as TNotification));

        this.#notificationHandlers.set(notificationName, handlers);
    }

    async unwatch(notificationName: string): Promise<void> {
        let transport = await this.#transportPromise;
        await transport.unwatch(this, notificationName);

        this.#notificationHandlers.delete(notificationName);
    }

    getStatus(): Promise<RequestResult<BehaviorStatusResponse>> {
        return this.request(new BehaviorStatusRequest());
    }

    watchStatus(handler: NotificationHandler<BehaviorStatusNotification>): Promise<void> {
        return this.watch<BehaviorStatusNotification>(BehaviorStatusNotification.notificationName, handler);
    }

    unwatchStatus(): Promise<void> {
        return this.unwatch(BehaviorStatusNotification.notificationName);
    }
}
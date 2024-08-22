import { RequestResult } from "../classes/request-result";
import { CurrentStateNotification } from "../events/current-state.notification";
import { CurrentStateRequest } from "../events/current-state.request";
import { CurrentStateResponse } from "../events/current-state.response";
import { IStateMachineBehavior } from "../interfaces/state-machine.behavior";
import { NotificationHandler } from "../utils/notification-handler";
import { Behavior } from "./behavior";

export class StateMachine extends Behavior implements IStateMachineBehavior {
    constructor(behavior: Behavior) {
        super(behavior, behavior.id);
    }

    getCurrentState(): Promise<RequestResult<CurrentStateResponse>> {
        return this.request(new CurrentStateRequest());
    }

    watchCurrentState(handler: NotificationHandler<CurrentStateNotification>): Promise<void> {
        return this.watch<CurrentStateNotification>(CurrentStateNotification.notificationName, handler);
    }

    unwatchCurrentState(): Promise<void> {
        return this.unwatch(CurrentStateNotification.notificationName);
    }
}
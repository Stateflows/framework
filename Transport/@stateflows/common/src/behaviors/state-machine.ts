import { RequestResult } from "../classes/request-result";
import { StateMachineInfo } from "../events/state-machine-info";
import { StateMachineInfoRequest } from "../events/state-machine-info.request";
import { IStateMachineBehavior } from "../interfaces/state-machine.behavior";
import { NotificationHandler } from "../utils/notification-handler";
import { Behavior } from "./behavior";

export class StateMachine extends Behavior implements IStateMachineBehavior {
    constructor(behavior: Behavior) {
        super(behavior, behavior.id);
    }

    async requestAndWatchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void> {
        let promise = await this.watch<StateMachineInfo>(StateMachineInfo.eventName, handler);
        let result = await this.request(new StateMachineInfoRequest());
        handler(result.response);
        return promise;
    }

    getCurrentState(): Promise<RequestResult<StateMachineInfo>> {
        return this.request(new StateMachineInfoRequest());
    }

    watchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void> {
        return this.watch<StateMachineInfo>(StateMachineInfo.eventName, handler);
    }

    unwatchCurrentState(): Promise<void> {
        return this.unwatch(StateMachineInfo.eventName);
    }
}
import { RequestResult } from "../classes/request-result";
import { StateMachineInfo } from "../events/state-machine-info";
import { NotificationHandler } from "../utils/notification-handler";
import { IBehavior } from "./behavior";
import { IWatcher } from "./watcher";

export interface IStateMachineBehavior extends IBehavior {
    getCurrentState(): Promise<RequestResult<StateMachineInfo>>;
    watchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void>;
    requestAndWatchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void>;
    unwatchCurrentState(): Promise<void>;

    // watchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<IWatcher>;
    // requestAndWatchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<IWatcher>;
}
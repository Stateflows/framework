import { RequestResult } from "../classes/request-result";
import { StateMachineInfo } from "../events/state-machine-info";
import { NotificationHandler } from "../utils/notification-handler";
import { IBehavior } from "./behavior";

export interface IStateMachineBehavior extends IBehavior {
    getCurrentState(): Promise<RequestResult<StateMachineInfo>>;
    watchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void>;
    unwatchCurrentState(): Promise<void>;
}
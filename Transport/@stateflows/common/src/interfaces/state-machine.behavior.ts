import { RequestResult } from "../classes/request-result";
import { CurrentStateNotification } from "../events/current-state.notification";
import { CurrentStateResponse } from "../events/current-state.response";
import { NotificationHandler } from "../utils/notification-handler";
import { IBehavior } from "./behavior";

export interface IStateMachineBehavior extends IBehavior {
    getCurrentState(): Promise<RequestResult<CurrentStateResponse>>;
    watchCurrentState(handler: NotificationHandler<CurrentStateNotification>): Promise<void>;
    unwatchCurrentState(): Promise<void>;
}
import { RequestResult } from "../classes/request-result";
import { CurrentStateResponse } from "../events/current-state.response";
import { IBehavior } from "./behavior";

export interface IStateMachine extends IBehavior {
    getCurrentState(): Promise<RequestResult<CurrentStateResponse>>;
}
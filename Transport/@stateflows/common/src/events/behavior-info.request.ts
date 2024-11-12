import { Request } from "./request";
import { BehaviorInfo } from "./behavior-info";
import { StateflowsEvent } from "../decorators/stateflows-event";

@StateflowsEvent("Stateflows.Common.BehaviorInfoRequest, Stateflows.Common")
export class BehaviorInfoRequest extends Request<BehaviorInfo> { }
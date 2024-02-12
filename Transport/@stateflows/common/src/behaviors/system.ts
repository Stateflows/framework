import { ISystem } from "../interfaces/system";
import { IBehavior } from "../interfaces/behavior";
import { RequestResult } from "../classes/request-result";
import { AvailableBehaviorClassesRequest } from "../events/available-behavior-classes.request";
import { AvailableBehaviorClassesResponse } from "../events/available-behavior-classes.response";
import { BehaviorInstancesRequest } from "../events/behavior-instances.request";
import { BehaviorInstancesResponse } from "../events/behavior-instances.response";

export class System implements ISystem {
    constructor(private behavior: IBehavior) {}

    getAvailableBehaviorClasses(): Promise<RequestResult<AvailableBehaviorClassesResponse>> {
        return this.behavior.request(new AvailableBehaviorClassesRequest());
    }

    getBehaviorInstances(): Promise<RequestResult<BehaviorInstancesResponse>> {
        return this.behavior.request(new BehaviorInstancesRequest());
    }
}
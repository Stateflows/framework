import { RequestResult } from "../classes/request-result";
import { AvailableBehaviorClassesResponse } from "../events/available-behavior-classes.response";
import { BehaviorInstancesResponse } from "../events/behavior-instances.response";

export interface ISystem {
    getAvailableBehaviorClasses(): Promise<RequestResult<AvailableBehaviorClassesResponse>>;
    getBehaviorInstances(): Promise<RequestResult<BehaviorInstancesResponse>>;
}
import { Response } from "./response";
import { BehaviorId } from "../ids/behavior.id";
import { BehaviorStatus } from "../enums/behavior-status";

export class BehaviorDescriptor
{
    Id: BehaviorId;
    Status: BehaviorStatus;
}

export class BehaviorInstancesResponse extends Response {
    constructor(
        public Behaviors: Array<BehaviorDescriptor>,
    ) {
        super();
    }
}
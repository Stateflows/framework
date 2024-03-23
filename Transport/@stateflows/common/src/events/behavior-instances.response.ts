import { Response } from "./response";
import { BehaviorId } from "../ids/behavior.id";
import { BehaviorStatus } from "../enums/behavior-status";

export class BehaviorDescriptor
{
    id: BehaviorId;
    status: BehaviorStatus;
}

export class BehaviorInstancesResponse extends Response {
    constructor(
        public behaviors: Array<BehaviorDescriptor>,
    ) {
        super();
    }
}
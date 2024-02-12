import { BehaviorStatus } from "../enums/behavior-status";
import { Response } from "./response";

export class BehaviorStatusResponse extends Response {
    constructor(
        public BehaviorStatus: BehaviorStatus,
    ) {
        super();
    }
}
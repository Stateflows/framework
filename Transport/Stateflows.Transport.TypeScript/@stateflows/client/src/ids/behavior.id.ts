import { BehaviorClass } from "./behavior.class";

export class BehaviorId {
    constructor(
        public behaviorClass: BehaviorClass,
        public instance: string
    ) {}
}
import { BehaviorId } from "../ids/behavior.id";
import { IBehavior } from "./behavior";

export interface IBehaviorLocator {
    locateBehavior(id: BehaviorId): Promise<IBehavior>;
}
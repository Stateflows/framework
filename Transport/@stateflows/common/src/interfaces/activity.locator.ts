import { ActivityId } from "../ids/activity.id";
import { IActivityBehavior } from "./activity.behavior";

export interface IActivityLocator {
    locateActivity(id: ActivityId): Promise<IActivityBehavior>;
}
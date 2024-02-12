import { ActivityId } from "../ids/activity.id";
import { IActivity } from "./activity";

export interface IActivityLocator {
    locateActivity(id: ActivityId): Promise<IActivity>;
}
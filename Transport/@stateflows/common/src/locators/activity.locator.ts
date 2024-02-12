import { Activity } from "../behaviors/activity";
import { Behavior } from "../behaviors/behavior";
import { ActivityId } from "../ids/activity.id";
import { IActivity } from "../interfaces/activity";
import { IActivityLocator } from "../interfaces/activity.locator";
import { IBehaviorLocator } from "../interfaces/behavior.locator";

export class ActivityLocator implements IActivityLocator {
    constructor(private behaviorLocator: IBehaviorLocator) {}

    locateActivity(id: ActivityId): Promise<IActivity> {
        return new Promise<IActivity>((resolve, reject) => {
            this.behaviorLocator.locateBehavior(id)
                .then(behavior => resolve(new Activity(behavior as Behavior)))
                .catch(_ => reject("State Machine not found"));
        });
    }
}
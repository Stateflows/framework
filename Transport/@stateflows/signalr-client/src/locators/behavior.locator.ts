import { HubConnection } from "@microsoft/signalr";
import { Behavior } from "../behaviors/behavior";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IBehavior } from "../interfaces/behavior";
import { IBehaviorLocator } from "../interfaces/behavior.locator";

export class BehaviorLocator implements IBehaviorLocator {
    private behaviorClasses: BehaviorClass[] = [];

    constructor(private hubPromise: Promise<HubConnection>) {
        hubPromise.then(hub => {
            hub.invoke<BehaviorClass[]>('GetAvailableClasses').then(result => {
                this.behaviorClasses = result;
            });
        });
    }

    locateBehavior(behaviorId: BehaviorId): Promise<IBehavior> {
        return new Promise<IBehavior>((resolve, reject) => {
            if (this.behaviorClasses.findIndex(behaviorClass => 
                behaviorClass.type === behaviorId.behaviorClass.type &&
                behaviorClass.name === behaviorId.behaviorClass.name
            ) !== -1) {
                resolve(new Behavior(this.hubPromise, behaviorId));
            }
            else
            {
                reject("Behavior not found");
            }
        });
    }
}
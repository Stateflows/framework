import { HubConnection } from "@microsoft/signalr";
import { Behavior } from "../behaviors/behavior";
import { BehaviorClass } from "../ids/behavior.class";
import { BehaviorId } from "../ids/behavior.id";
import { IBehavior } from "../interfaces/behavior";
import { IBehaviorLocator } from "../interfaces/behavior.locator";

export class BehaviorLocator implements IBehaviorLocator {
    private _behaviorClasses: BehaviorClass[] = [];
    private _hubPromise: Promise<HubConnection> = null;

    constructor(hubPromise: Promise<HubConnection>) {
        this._hubPromise = new Promise<HubConnection>((resolve, reject) => {
            hubPromise.then(hub => {
                hub.invoke<BehaviorClass[]>('GetAvailableClasses')
                    .then(result => {
                        this._behaviorClasses = result;
                        resolve(hub);
                    })
                    .catch(reason => reject(reason));
            });
        });
    }

    locateBehavior(behaviorId: BehaviorId): Promise<IBehavior> {
        return new Promise<IBehavior>((resolve, reject) => {
            this._hubPromise.then(hub => {
                if (this._behaviorClasses.findIndex(behaviorClass => 
                    behaviorClass.type === behaviorId.behaviorClass.type &&
                    behaviorClass.name === behaviorId.behaviorClass.name
                ) !== -1) {
                    resolve(new Behavior(hub, behaviorId));
                }
                else
                {
                    reject("Behavior not found");
                }
            })
            .catch(reason => reject(reason));
        });
    }
}
import { Event, BehaviorClass, BehaviorId, IStateflowsClientTransport, SendResult } from "@stateflows/common";

export class HttpTransport implements IStateflowsClientTransport {
    constructor(private url: string) {
        if (url.slice(-1) != '/') {
            url = url + '/';
        }
    }

    getAvailableClasses(): Promise<BehaviorClass[]> {
        return new Promise<BehaviorClass[]>(async (resolve, reject) => {
            fetch(`${this.url}stateflows/availableClasses`)
                .then(async result => resolve(await result.json() as BehaviorClass[]))
                .catch(reason => reject(reason));
        });
    }
    
    send(behaviorId: BehaviorId, event: Event): Promise<SendResult> {
        return new Promise<SendResult>(async (resolve, reject) => {
            const eventName = event.Name;
            delete event.Name;
            fetch(
                `${this.url}stateflows/send`,
                {
                    method: "POST",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        "$type": "Stateflows.Common.Transport.Classes.StateflowsRequest, Stateflows.Common.Transport",
                        BehaviorId: behaviorId,
                        Event: event
                    })
                }
            )
                .then(async result => {
                    let stateflowsResponse = await result.json();
                    let response = stateflowsResponse.Response;
                    let validation = stateflowsResponse.Validation;
                    event.Name = eventName;
                    if (response) {
                        (event as any).Response = response;
                    }

                    let sendResult = new SendResult(event, stateflowsResponse.eventStatus, validation);

                    resolve(sendResult);
                })
                .catch(reason => reject(reason));
        });
    }
}
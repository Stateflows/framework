import { Event, BehaviorClass, BehaviorId, IStateflowsClientTransport, SendResult, JsonUtils } from "@stateflows/common";

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
            fetch(
                `${this.url}stateflows/send`,
                {
                    method: "POST",
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    body: JsonUtils.stringify({
                        "$type": "Stateflows.Common.Transport.Classes.StateflowsRequest, Stateflows.Common.Transport",
                        behaviorId: behaviorId,
                        event: event
                    })
                }
            )
                .then(async result => {
                    let stateflowsResponse = await result.json();
                    let response = stateflowsResponse.response;
                    let validation = stateflowsResponse.validation;
                    if (response) {
                        (event as any).response = response;
                    }

                    let sendResult = new SendResult(event, stateflowsResponse.eventStatus, validation);

                    resolve(sendResult);
                })
                .catch(reason => reject(reason));
        });
    }
}
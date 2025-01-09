import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import * as plantUmlEncoder from 'plantuml-encoder';
import { StateflowsClient, StateMachineId, BehaviorStatus, EventStatus, PlantUmlRequest, PlantUmlResponse, Event, InitializationRequest, CompoundRequest, IStateMachine, PlantUmlNotification, CurrentStateNotification } from '@stateflows/common';
import { UseHttp } from '@stateflows/http-client';
// import { UseSignalR } from '@stateflows/signalr-client';
class OtherEvent extends Event {
  public $type: string = "Examples.Common.OtherEvent, Examples.Common";
  public name: string = "Examples.Common.OtherEvent";
  //public RequiredParameter: string | null = null;
}

@Component({
  selector: 'app-counter-component',
  templateUrl: './counter.component.html'
})
export class CounterComponent {
  public currentCount = 0;
  private stateMachine: IStateMachine | null = null;
  private stateflows: StateflowsClient = new StateflowsClient(UseHttp("https://localhost:7067/"));
  // private stateflows: StateflowsClient = new StateflowsClient(UseSignalR("https://localhost:7067/", (b) => b.withAutomaticReconnect()));
  public url: string | null = null;

  constructor(private http: HttpClient) { }

  public async refresh() {
    this.stateMachine = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "x"));
    let encoded = plantUmlEncoder.encode((await this.stateMachine.request<PlantUmlResponse>(new PlantUmlRequest())).response.plantUml);
    this.url = 'http://www.plantuml.com/plantuml/img/' + encoded;
  }

  public async go() {
    this.http.get('https://localhost:7067/StateMachine/stateMachine1/x/go').subscribe(() => this.refresh());
  }

  public async push() {
    this.http.post('https://localhost:7067/StateMachine/stateMachine1/x/push', { "dolor": "sit amet" }).subscribe(() => this.refresh());
  }

  public async incrementCounter() {
    if (this.stateMachine === null) {
      this.stateMachine = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", Math.random().toString()));

      await this.stateMachine.watch("Examples.Common.SomeNotification", (n: any) => console.log('Notification:', n));

      await this.stateMachine.watch(PlantUmlNotification.notificationName, (n: PlantUmlNotification) => {
        this.url = 'http://www.plantuml.com/plantuml/img/' + plantUmlEncoder.encode(n.plantUml);
      });
      await this.stateMachine.watchCurrentState((n: CurrentStateNotification) => console.log(n.statesStack));

      let x = await this.stateMachine.getStatus();

      if (x.response.behaviorStatus == BehaviorStatus.NotInitialized) {
        await this.stateMachine.initialize(new InitializationRequest());
      }
    }

    let result = (await this.stateMachine.request(new CompoundRequest([new OtherEvent()])));

    this.currentCount++;
  }
}

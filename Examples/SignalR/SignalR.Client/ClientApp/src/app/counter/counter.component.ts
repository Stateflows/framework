import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import * as plantUmlEncoder from 'plantuml-encoder';
import {
  StateflowsClient,
  StateMachineId,
  BehaviorStatus,
  PlantUmlInfoRequest,
  PlantUmlInfo,
  Event,
  Initialize,
  CompoundRequest,
  IStateMachineBehavior,
  StateflowsEvent, StateMachineInfo
} from '@stateflows/common';
import { UseHttp } from '@stateflows/http-client';

// import { UseSignalR } from '@stateflows/signalr-client';
@StateflowsEvent("Examples.Common.OtherEvent, Examples.Common")
class OtherEvent extends Event { }

@Component({
  selector: 'app-counter-component',
  templateUrl: './counter.component.html'
})
export class CounterComponent {
  public currentCount = 0;
  private stateMachine: IStateMachineBehavior | null = null;
  private stateflows: StateflowsClient = new StateflowsClient(UseHttp("https://localhost:7067/"));
  // private stateflows: StateflowsClient = new StateflowsClient(UseSignalR("https://localhost:7067/", (b) => b.withAutomaticReconnect()));
  public url: string | null = null;

  constructor(private http: HttpClient) { }

  public async refresh() {
    this.stateMachine = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "x"));
    let encoded = plantUmlEncoder.encode((await this.stateMachine.request<PlantUmlInfo>(new PlantUmlInfoRequest())).response.plantUml);
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

      // await this.stateMachine.watch("Examples.Common.SomeNotification", (n: any) => console.log('Notification:', n));
      //
      // await this.stateMachine.watch(PlantUmlInfo.name, (n: PlantUmlInfo) => {
      //   this.url = 'http://www.plantuml.com/plantuml/img/' + plantUmlEncoder.encode(n.plantUml);
      // });
      await this.stateMachine.watchCurrentState((n: StateMachineInfo) => console.log(n.statesTree.root.value));

      let x = await this.stateMachine.getStatus();

      let y = await this.stateMachine.getCurrentState();

      if (x.response.behaviorStatus == BehaviorStatus.NotInitialized) {
        await this.stateMachine.send(new Initialize());
      }
    }

    let ev = new CompoundRequest([new OtherEvent()]);
    let result = (await this.stateMachine.request(ev));

    this.currentCount++;
  }
}

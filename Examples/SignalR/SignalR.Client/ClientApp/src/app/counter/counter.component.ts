import { Component } from '@angular/core';
import * as plantUmlEncoder from 'plantuml-encoder';
import { StateflowsClient, StateMachineId, BehaviorStatus, EventStatus, PlantUmlRequest, PlantUmlResponse, Event } from '@stateflows/client-abstractions';
import { SignalR } from '@stateflows/signalr-client';

class OtherEvent extends Event {
  public $type: string = "Examples.Common.OtherEvent, Examples.Common";
  //public RequiredParameter: string | null = null;
}

@Component({
  selector: 'app-counter-component',
  templateUrl: './counter.component.html'
})
export class CounterComponent {
  public currentCount = 0;
  private stateflows: StateflowsClient = new StateflowsClient(SignalR("https://localhost:7067/"));
  public url: string | null = null;

  public async incrementCounter() {
    let sm = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "xx"));
    if ((await sm.getStatus()).Response.BehaviorStatus == BehaviorStatus.NotInitialized) {
      await sm.initialize();
    }

    let result = (await sm.send(new OtherEvent()));
    if (result.Status == EventStatus.Consumed) {
      let encoded = plantUmlEncoder.encode((await sm.request<PlantUmlResponse>(new PlantUmlRequest())).Response.PlantUml);
      this.url = 'http://www.plantuml.com/plantuml/img/' + encoded;
    }

    let system = await this.stateflows.system;

    this.currentCount++;
  }
}

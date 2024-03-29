import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import * as plantUmlEncoder from 'plantuml-encoder';
import { StateflowsClient, StateMachineId, BehaviorStatus, EventStatus, PlantUmlRequest, PlantUmlResponse, Event, InitializationRequest } from '@stateflows/common';
import { UseHttp } from '@stateflows/http-client';
import { UseSignalR } from '@stateflows/signalr-client';
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
  private stateflows: StateflowsClient = new StateflowsClient(UseSignalR("https://localhost:7067/"));
  public url: string | null = null;

  constructor(private http: HttpClient) { }

  public async refresh() {
    let sm = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "x"));
    let encoded = plantUmlEncoder.encode((await sm.request<PlantUmlResponse>(new PlantUmlRequest())).Response.PlantUml);
    this.url = 'http://www.plantuml.com/plantuml/img/' + encoded;
  }

  public async go() {
    this.http.get('https://localhost:7067/StateMachine/stateMachine1/x/go').subscribe(() => this.refresh());
  }

  public async push() {
    this.http.post('https://localhost:7067/StateMachine/stateMachine1/x/push', { "dolor": "sit amet" }).subscribe(() => this.refresh());
  }

  public async incrementCounter() {
    let sm = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "xxxx"));
    let x = await sm.getStatus();
    sm.send(new Event());
    console.log(x);
    if (x.Response.BehaviorStatus == BehaviorStatus.NotInitialized) {
      await sm.initialize(new InitializationRequest());
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

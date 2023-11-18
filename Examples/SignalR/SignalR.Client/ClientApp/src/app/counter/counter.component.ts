import { Component } from '@angular/core';
import { BehaviorStatus, Event, EventStatus, PlantUmlRequest, PlantUmlResponse, StateMachineId, StateflowsClient } from '@stateflows/client';

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
  private stateflows: StateflowsClient = new StateflowsClient("https://localhost:7067/");
  public url: string | null = null;

  public async incrementCounter() {
    let sm = await this.stateflows.stateMachineLocator.locateStateMachine(new StateMachineId("stateMachine1", "x"));

    if ((await sm.getStatus()).Response.BehaviorStatus == BehaviorStatus.NotInitialized) {
      await sm.initialize();
    }

    let result = (await sm.send(new OtherEvent()));
    if (result.Status == EventStatus.Consumed) {
      this.url = (await sm.request<PlantUmlResponse>(new PlantUmlRequest())).Response.PlantUmlUrl;
    } else {
      console.log(result);
    }

    this.currentCount++;
  }
}

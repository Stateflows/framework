import { StateflowsEvent } from "../decorators/stateflows-event";
import { Event } from "./event";

@StateflowsEvent("Stateflows.Extensions.PlantUml.Events.PlantUmlInfo, Stateflows.Extensions.PlantUml")
export class PlantUmlInfo extends Event {
    public plantUml: string;
}
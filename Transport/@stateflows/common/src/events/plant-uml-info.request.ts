import { StateflowsEvent } from "../decorators/stateflows-event";
import { PlantUmlInfo } from "./plant-uml-info";
import { Request } from "./request";

@StateflowsEvent("Stateflows.Extensions.PlantUml.Events.PlantUmlInfoRequest, Stateflows.Extensions.PlantUml")
export class PlantUmlInfoRequest extends Request<PlantUmlInfo> { }
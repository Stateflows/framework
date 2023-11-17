import { PlantUmlResponse } from "./plant-uml.response";
import { Request } from "./request";

export class PlantUmlRequest extends Request<PlantUmlResponse> {
    public $type = "Stateflows.Extensions.PlantUml.Events.PlantUmlRequest, Stateflows.Extensions.PlantUml";

    constructor() {
        super();
    }
}
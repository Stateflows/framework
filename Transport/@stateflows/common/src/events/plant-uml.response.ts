import { Response } from "./response";

export class PlantUmlResponse extends Response {
    constructor(
        public PlantUml: string,
    ) {
        super();
    }
}
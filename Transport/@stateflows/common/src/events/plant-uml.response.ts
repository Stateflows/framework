import { Response } from "./response";

export class PlantUmlResponse extends Response {
    constructor(
        public plantUml: string,
    ) {
        super();
    }
}
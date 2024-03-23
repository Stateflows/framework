import { Response } from "./response";

export class CurrentStateResponse extends Response {
    constructor(
        public statesStack: Array<string>,
        public expectedEvents: Array<string>,
    ) {
        super();
    }
}
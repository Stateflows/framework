import { Response } from "./response";

export class CurrentStateResponse extends Response {
    constructor(
        public StatesStack: Array<string>,
        public ExpectedEvents: Array<string>,
    ) {
        super();
    }
}
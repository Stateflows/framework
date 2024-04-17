import { Response } from "./response";

export class InitializationResponse extends Response {
    constructor(
        public initializationSuccessful: boolean,
    ) {
        super();
    }
}
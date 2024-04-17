import { Response } from "./response";

export class FinalizationResponse extends Response {
    constructor(
        public finalizationSuccessful: boolean,
    ) {
        super();
    }
}
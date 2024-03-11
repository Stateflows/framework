import { Response } from "./response";
import { RequestResult } from "../classes/request-result";

export class CompoundResponse extends Response {
    constructor(
        public Results: Array<RequestResult<Response>>,
    ) {
        super();
    }
}
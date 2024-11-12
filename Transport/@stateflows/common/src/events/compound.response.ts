import { RequestResult } from "../classes/request-result";

export class CompoundResponse {
    constructor(
        public results: Array<RequestResult<any>>,
    ) { }
}

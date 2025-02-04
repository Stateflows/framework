import { StateflowsEvent } from "../decorators/stateflows-event";
import { TokensTransfer } from "./tokens-transfer";

@StateflowsEvent("Stateflows.Activities.Events.TokensInput, Stateflows.Common")
export class TokensInput extends TokensTransfer { }
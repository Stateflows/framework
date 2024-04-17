import { HubConnectionBuilder } from "@microsoft/signalr";

export type HubConnectionBuilderAction = (builder: HubConnectionBuilder) => HubConnectionBuilder;
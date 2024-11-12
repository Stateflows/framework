declare class BehaviorClass {
    type: string;
    name: string;
    constructor(type: string, name: string);
    $type: string;
}

declare class BehaviorId {
    behaviorClass: BehaviorClass;
    instance: string;
    constructor(behaviorClass: BehaviorClass, instance: string);
    $type: string;
    toString(): string;
}

declare enum EventStatus {
    Initialized = 0,
    NotInitialized = 1,
    Undelivered = 2,
    Rejected = 3,
    Invalid = 4,
    Deferred = 5,
    Consumed = 6,
    NotConsumed = 7,
    Omitted = 8,
    Failed = 9,
    Forwarded = 10
}

declare class ValidationResult {
    errorMessage: string | null;
    memberNames: Array<string> | null;
    constructor(errorMessage: string | null, memberNames: Array<string> | null);
}

declare class EventValidation {
    isValid: boolean;
    validationResults: Array<ValidationResult>;
    constructor(isValid: boolean, validationResults: Array<ValidationResult>);
}

declare class SendResult {
    event: any;
    status: EventStatus;
    validation: EventValidation;
    constructor(event: any, status: EventStatus, validation: EventValidation);
}

declare class RequestResult<TResponse> extends SendResult {
    response: TResponse;
    constructor(response: TResponse, event: any, status: EventStatus, validation: EventValidation);
}

declare class CompoundResponse {
    results: Array<RequestResult<any>>;
    constructor(results: Array<RequestResult<any>>);
}

type NotificationHandler<TNotification> = (notification: TNotification) => void;

declare enum ResetMode {
    KeepVersionAndSubscriptions = 0,
    KeepSubscriptions = 1,
    Full = 2
}

declare abstract class Event {
    static $type: string;
    static eventName: string;
    get $type(): string;
    get eventName(): string;
}

declare abstract class Request<TResponse> extends Event {
    response: TResponse;
}

declare enum BehaviorStatus {
    Unknown = 0,
    NotInitialized = 1,
    Initialized = 2,
    Finalized = 3
}

declare class BehaviorInfo extends Event {
    behaviorStatus: BehaviorStatus;
    expectedEvents: Array<string>;
}

declare class EventHeader {
}

interface IBehavior {
    id: BehaviorId;
    send(event: Event, headers?: EventHeader[] | null): Promise<SendResult>;
    sendCompound(...events: Event[]): Promise<RequestResult<CompoundResponse>>;
    request<TResponse>(request: Request<TResponse>, headers?: EventHeader[] | null): Promise<RequestResult<TResponse>>;
    finalize(): Promise<SendResult>;
    reset(resetMode?: ResetMode): Promise<SendResult>;
    watch<TNotification extends Event>(notificationName: string, handler: NotificationHandler<TNotification>): Promise<void>;
    unwatch(notificationName: string): Promise<void>;
    getStatus(): Promise<RequestResult<BehaviorInfo>>;
    watchStatus(handler: NotificationHandler<BehaviorInfo>): Promise<void>;
    unwatchStatus(): Promise<void>;
}

interface IBehaviorLocator {
    locateBehavior(id: BehaviorId): Promise<IBehavior>;
}

declare class StateMachineId extends BehaviorId {
    constructor(name: string, instance: string);
}

declare class StateMachineInfo extends BehaviorInfo {
    statesStack: Array<string>;
}

interface IStateMachineBehavior extends IBehavior {
    getCurrentState(): Promise<RequestResult<StateMachineInfo>>;
    watchCurrentState(handler: NotificationHandler<StateMachineInfo>): Promise<void>;
    unwatchCurrentState(): Promise<void>;
}

interface IStateMachineLocator {
    locateStateMachine(id: StateMachineId): Promise<IStateMachineBehavior>;
}

declare class ActivityId extends BehaviorId {
    constructor(name: string, instance: string);
}

interface IActivityBehavior extends IBehavior {
}

interface IActivityLocator {
    locateActivity(id: ActivityId): Promise<IActivityBehavior>;
}

declare class EventHolder {
    payload: any;
    headers: EventHeader[];
    $type: string;
    id: string;
    name: string;
    sentAt: string;
    senderId: BehaviorId;
    constructor(payload: any, headers?: EventHeader[]);
}

interface IWatcher {
    id: BehaviorId;
    notify(notification: EventHolder): void;
}

interface IStateflowsClientTransport {
    getAvailableClasses(): Promise<BehaviorClass[]>;
    send(behaviorId: BehaviorId, eventHolder: EventHolder): Promise<SendResult>;
    watch(watcher: IWatcher, notificationName: string): Promise<void>;
    unwatch(watcher: IWatcher, notificationName: string): Promise<void>;
}

interface IStateflowsClientTransportFactory {
    getTransport(): Promise<IStateflowsClientTransport>;
}

declare class StateflowsClient {
    #private;
    private transportFactory;
    constructor(transportFactory: IStateflowsClientTransportFactory);
    get behaviorLocator(): IBehaviorLocator;
    get stateMachineLocator(): IStateMachineLocator;
    get activityLocator(): IActivityLocator;
}

declare function StateflowsEvent(typeName: string, eventName?: string | null): (target: any) => void;

declare class CompoundRequest extends Request<CompoundResponse> {
    constructor(events: Array<any>);
    events: Array<EventHolder>;
}

declare class Initialize extends Event {
}

declare class Finalize extends Event {
}

declare class Reset extends Event {
    mode: ResetMode;
    constructor(mode?: ResetMode);
}

declare class BehaviorInfoRequest extends Request<BehaviorInfo> {
}

declare class PlantUmlInfo extends Event {
    plantUml: string;
}

declare class PlantUmlInfoRequest extends Request<PlantUmlInfo> {
}

declare class StateMachineInfoRequest extends Request<StateMachineInfo> {
}

declare class JsonUtils {
    static stringify(object: any): string;
    static parse(json: string): any;
    static deepClone<T>(object: T): T;
}

declare class NotificationsResponse extends Event {
}

declare class NotificationsRequest extends Request<NotificationsResponse> {
}

export { ActivityId, BehaviorClass, BehaviorId, BehaviorInfo, BehaviorInfoRequest, BehaviorStatus, CompoundRequest, CompoundResponse, Event, EventHeader, EventHolder, EventStatus, Finalize, type IActivityBehavior, type IActivityLocator, type IBehavior, type IBehaviorLocator, type IStateMachineBehavior, type IStateMachineLocator, type IStateflowsClientTransport, type IStateflowsClientTransportFactory, type IWatcher, Initialize, JsonUtils, type NotificationHandler, NotificationsRequest, NotificationsResponse, PlantUmlInfo, PlantUmlInfoRequest, Request, RequestResult, Reset, SendResult, StateMachineId, StateMachineInfo, StateMachineInfoRequest, StateflowsClient, StateflowsEvent };

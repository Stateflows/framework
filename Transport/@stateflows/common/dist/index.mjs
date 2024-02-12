// src/classes/send-result.ts
var SendResult = class {
  constructor(Event2, Status, Validation) {
    this.Event = Event2;
    this.Status = Status;
    this.Validation = Validation;
  }
};

// src/classes/request-result.ts
var RequestResult = class extends SendResult {
  constructor(Response2, event, status, validation) {
    super(event, status, validation);
    this.Response = Response2;
  }
};

// src/events/event.ts
var Event = class {
};

// src/events/request.ts
var Request = class extends Event {
  Response;
};

// src/events/initialization.request.ts
var InitializationRequest = class extends Request {
  $type = "Stateflows.Common.InitializationRequest, Stateflows.Common";
};

// src/events/behavior-status.request.ts
var BehaviorStatusRequest = class extends Request {
  $type = "Stateflows.Common.BehaviorStatusRequest, Stateflows.Common";
  constructor() {
    super();
  }
};

// src/behaviors/behavior.ts
var Behavior = class _Behavior {
  constructor(transportPromiseOrBehavior, behaviorId) {
    this.behaviorId = behaviorId;
    this.#transportPromise = transportPromiseOrBehavior instanceof _Behavior ? transportPromiseOrBehavior.#transportPromise : this.#transportPromise = transportPromiseOrBehavior;
  }
  #transportPromise;
  send(event) {
    return new Promise(async (resolve, reject) => {
      let hub = await this.#transportPromise;
      resolve(await hub.send(this.behaviorId, event));
    });
  }
  request(request) {
    return new Promise(async (resolve, reject) => {
      let result = await this.send(request);
      resolve(new RequestResult(request.Response, request, result.Status, result.Validation));
    });
  }
  initialize(initializationRequest) {
    if (typeof initializationRequest === "undefined") {
      initializationRequest = new InitializationRequest();
    }
    return this.request(initializationRequest);
  }
  getStatus() {
    return this.request(new BehaviorStatusRequest());
  }
};

// src/locators/behavior.locator.ts
var BehaviorLocator = class {
  behaviorClasses = [];
  transportPromise;
  constructor(transportPromise) {
    this.transportPromise = new Promise((resolve, reject) => {
      transportPromise.then((transport) => {
        transport.getAvailableClasses().then((result) => {
          this.behaviorClasses = result;
          resolve(transport);
        });
      }).catch((reason) => reject(reason));
    });
  }
  locateBehavior(behaviorId) {
    return new Promise((resolve, reject) => {
      this.transportPromise.then((hub) => {
        if (this.behaviorClasses.findIndex(
          (behaviorClass) => behaviorClass.type === behaviorId.behaviorClass.type && behaviorClass.name === behaviorId.behaviorClass.name
        ) !== -1) {
          resolve(new Behavior(this.transportPromise, behaviorId));
        } else {
          reject("Behavior not found");
        }
      }).catch((reason) => reject(reason));
    });
  }
};

// src/events/current-state.request.ts
var CurrentStateRequest = class extends Request {
  $type = "Stateflows.StateMachines.Events.CurrentStateRequest, Stateflows.Common";
  constructor() {
    super();
  }
};

// src/behaviors/state-machine.ts
var StateMachine = class extends Behavior {
  constructor(behavior) {
    super(behavior, behavior.behaviorId);
  }
  getCurrentState() {
    return this.request(new CurrentStateRequest());
  }
};

// src/locators/state-machine.locator.ts
var StateMachineLocator = class {
  constructor(behaviorLocator) {
    this.behaviorLocator = behaviorLocator;
  }
  locateStateMachine(id) {
    return new Promise((resolve, reject) => {
      this.behaviorLocator.locateBehavior(id).then((behavior) => resolve(new StateMachine(behavior))).catch((_) => reject("State Machine not found"));
    });
  }
};

// src/ids/behavior.id.ts
var BehaviorId = class {
  constructor(behaviorClass, instance) {
    this.behaviorClass = behaviorClass;
    this.instance = instance;
  }
  $type = "Stateflows.BehaviorId, Stateflows.Common";
};

// src/ids/behavior.class.ts
var BehaviorClass = class {
  constructor(type, name) {
    this.type = type;
    this.name = name;
  }
  $type = "Stateflows.BehaviorClass, Stateflows.Common";
};

// src/events/available-behavior-classes.request.ts
var AvailableBehaviorClassesRequest = class extends Request {
  $type = "Stateflows.System.AvailableBehaviorClassesRequest, Stateflows.Common";
  constructor() {
    super();
  }
};

// src/events/behavior-instances.request.ts
var BehaviorInstancesRequest = class extends Request {
  $type = "Stateflows.System.BehaviorInstancesRequest, Stateflows.Common";
  constructor() {
    super();
  }
};

// src/behaviors/system.ts
var System = class {
  constructor(behavior) {
    this.behavior = behavior;
  }
  getAvailableBehaviorClasses() {
    return this.behavior.request(new AvailableBehaviorClassesRequest());
  }
  getBehaviorInstances() {
    return this.behavior.request(new BehaviorInstancesRequest());
  }
};

// src/behaviors/activity.ts
var Activity = class extends Behavior {
  constructor(behavior) {
    super(behavior, behavior.behaviorId);
  }
};

// src/locators/activity.locator.ts
var ActivityLocator = class {
  constructor(behaviorLocator) {
    this.behaviorLocator = behaviorLocator;
  }
  locateActivity(id) {
    return new Promise((resolve, reject) => {
      this.behaviorLocator.locateBehavior(id).then((behavior) => resolve(new Activity(behavior))).catch((_) => reject("State Machine not found"));
    });
  }
};

// src/classes/stateflows-client.ts
var StateflowsClient = class {
  constructor(transportFactory) {
    this.transportFactory = transportFactory;
  }
  #behaviorLocator = null;
  get behaviorLocator() {
    return this.#behaviorLocator ??= new BehaviorLocator(this.transportFactory.getTransport());
  }
  #stateMachineLocator = null;
  get stateMachineLocator() {
    return this.#stateMachineLocator ??= new StateMachineLocator(this.behaviorLocator);
  }
  #activityLocator = null;
  get activityLocator() {
    return this.#activityLocator ??= new ActivityLocator(this.behaviorLocator);
  }
  #systemPromise = null;
  get system() {
    return this.#systemPromise ??= new Promise((resolve, reject) => {
      this.behaviorLocator.locateBehavior(new BehaviorId(new BehaviorClass("System", "Stateflows"), "")).then((behavior) => resolve(new System(behavior))).catch((reason) => reject(reason));
    });
  }
};

// src/ids/state-machine.id.ts
var StateMachineId = class extends BehaviorId {
  constructor(name, instance) {
    super(new BehaviorClass("StateMachine", name), instance);
  }
};

// src/ids/activity.id.ts
var ActivityId = class extends BehaviorId {
  constructor(name, instance) {
    super(new BehaviorClass("Activity", name), instance);
  }
};

// src/events/response.ts
var Response = class extends Event {
};

// src/events/initialization.response.ts
var InitializationResponse = class extends Response {
  constructor(InitializationSuccessful) {
    super();
    this.InitializationSuccessful = InitializationSuccessful;
  }
};

// src/events/behavior-status.response.ts
var BehaviorStatusResponse = class extends Response {
  constructor(BehaviorStatus2) {
    super();
    this.BehaviorStatus = BehaviorStatus2;
  }
};

// src/events/plant-uml.request.ts
var PlantUmlRequest = class extends Request {
  $type = "Stateflows.Extensions.PlantUml.Events.PlantUmlRequest, Stateflows.Extensions.PlantUml";
  constructor() {
    super();
  }
};

// src/events/plant-uml.response.ts
var PlantUmlResponse = class extends Response {
  constructor(PlantUml) {
    super();
    this.PlantUml = PlantUml;
  }
};

// src/enums/event-status.ts
var EventStatus = /* @__PURE__ */ ((EventStatus2) => {
  EventStatus2[EventStatus2["Undelivered"] = 0] = "Undelivered";
  EventStatus2[EventStatus2["Rejected"] = 1] = "Rejected";
  EventStatus2[EventStatus2["Invalid"] = 2] = "Invalid";
  EventStatus2[EventStatus2["NotConsumed"] = 3] = "NotConsumed";
  EventStatus2[EventStatus2["Deferred"] = 4] = "Deferred";
  EventStatus2[EventStatus2["Consumed"] = 5] = "Consumed";
  return EventStatus2;
})(EventStatus || {});

// src/enums/behavior-status.ts
var BehaviorStatus = /* @__PURE__ */ ((BehaviorStatus2) => {
  BehaviorStatus2[BehaviorStatus2["Unknown"] = 0] = "Unknown";
  BehaviorStatus2[BehaviorStatus2["NotInitialized"] = 1] = "NotInitialized";
  BehaviorStatus2[BehaviorStatus2["Initialized"] = 2] = "Initialized";
  BehaviorStatus2[BehaviorStatus2["Finalized"] = 3] = "Finalized";
  return BehaviorStatus2;
})(BehaviorStatus || {});

// src/events/available-behavior-classes.response.ts
var AvailableBehaviorClassesResponse = class extends Response {
  constructor(AvailableBehaviorClasses) {
    super();
    this.AvailableBehaviorClasses = AvailableBehaviorClasses;
  }
};

// src/events/behavior-instances.response.ts
var BehaviorInstancesResponse = class extends Response {
  constructor(Behaviors) {
    super();
    this.Behaviors = Behaviors;
  }
};
export {
  ActivityId,
  AvailableBehaviorClassesRequest,
  AvailableBehaviorClassesResponse,
  BehaviorClass,
  BehaviorId,
  BehaviorInstancesRequest,
  BehaviorInstancesResponse,
  BehaviorStatus,
  BehaviorStatusRequest,
  BehaviorStatusResponse,
  Event,
  EventStatus,
  InitializationRequest,
  InitializationResponse,
  PlantUmlRequest,
  PlantUmlResponse,
  Request,
  RequestResult,
  Response,
  SendResult,
  StateMachineId,
  StateflowsClient
};
//# sourceMappingURL=index.mjs.map
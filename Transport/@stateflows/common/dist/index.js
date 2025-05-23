var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __knownSymbol = (name, symbol) => (symbol = Symbol[name]) ? symbol : Symbol.for("Symbol." + name);
var __typeError = (msg) => {
  throw TypeError(msg);
};
var __defNormalProp = (obj, key, value) => key in obj ? __defProp(obj, key, { enumerable: true, configurable: true, writable: true, value }) : obj[key] = value;
var __name = (target, value) => __defProp(target, "name", { value, configurable: true });
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
var __decoratorStart = (base) => {
  var _a13;
  return [, , , __create((_a13 = base == null ? void 0 : base[__knownSymbol("metadata")]) != null ? _a13 : null)];
};
var __decoratorStrings = ["class", "method", "getter", "setter", "accessor", "field", "value", "get", "set"];
var __expectFn = (fn) => fn !== void 0 && typeof fn !== "function" ? __typeError("Function expected") : fn;
var __decoratorContext = (kind, name, done, metadata, fns) => ({ kind: __decoratorStrings[kind], name, metadata, addInitializer: (fn) => done._ ? __typeError("Already initialized") : fns.push(__expectFn(fn || null)) });
var __decoratorMetadata = (array, target) => __defNormalProp(target, __knownSymbol("metadata"), array[3]);
var __runInitializers = (array, flags, self, value) => {
  for (var i = 0, fns = array[flags >> 1], n = fns && fns.length; i < n; i++) flags & 1 ? fns[i].call(self) : value = fns[i].call(self, value);
  return value;
};
var __decorateElement = (array, flags, name, decorators, target, extra) => {
  var fn, it, done, ctx, access, k = flags & 7, s = !!(flags & 8), p = !!(flags & 16);
  var j = k > 3 ? array.length + 1 : k ? s ? 1 : 2 : 0, key = __decoratorStrings[k + 5];
  var initializers = k > 3 && (array[j - 1] = []), extraInitializers = array[j] || (array[j] = []);
  var desc = k && (!p && !s && (target = target.prototype), k < 5 && (k > 3 || !p) && __getOwnPropDesc(k < 4 ? target : { get [name]() {
    return __privateGet(this, extra);
  }, set [name](x) {
    return __privateSet(this, extra, x);
  } }, name));
  k ? p && k < 4 && __name(extra, (k > 2 ? "set " : k > 1 ? "get " : "") + name) : __name(target, name);
  for (var i = decorators.length - 1; i >= 0; i--) {
    ctx = __decoratorContext(k, name, done = {}, array[3], extraInitializers);
    if (k) {
      ctx.static = s, ctx.private = p, access = ctx.access = { has: p ? (x) => __privateIn(target, x) : (x) => name in x };
      if (k ^ 3) access.get = p ? (x) => (k ^ 1 ? __privateGet : __privateMethod)(x, target, k ^ 4 ? extra : desc.get) : (x) => x[name];
      if (k > 2) access.set = p ? (x, y) => __privateSet(x, target, y, k ^ 4 ? extra : desc.set) : (x, y) => x[name] = y;
    }
    it = (0, decorators[i])(k ? k < 4 ? p ? extra : desc[key] : k > 4 ? void 0 : { get: desc.get, set: desc.set } : target, ctx), done._ = 1;
    if (k ^ 4 || it === void 0) __expectFn(it) && (k > 4 ? initializers.unshift(it) : k ? p ? extra = it : desc[key] = it : target = it);
    else if (typeof it !== "object" || it === null) __typeError("Object expected");
    else __expectFn(fn = it.get) && (desc.get = fn), __expectFn(fn = it.set) && (desc.set = fn), __expectFn(fn = it.init) && initializers.unshift(fn);
  }
  return k || __decoratorMetadata(array, target), desc && __defProp(target, name, desc), p ? k ^ 4 ? extra : desc : target;
};
var __accessCheck = (obj, member, msg) => member.has(obj) || __typeError("Cannot " + msg);
var __privateIn = (member, obj) => Object(obj) !== obj ? __typeError('Cannot use the "in" operator on this value') : member.has(obj);
var __privateGet = (obj, member, getter) => (__accessCheck(obj, member, "read from private field"), getter ? getter.call(obj) : member.get(obj));
var __privateAdd = (obj, member, value) => member.has(obj) ? __typeError("Cannot add the same private member more than once") : member instanceof WeakSet ? member.add(obj) : member.set(obj, value);
var __privateSet = (obj, member, value, setter) => (__accessCheck(obj, member, "write to private field"), setter ? setter.call(obj, value) : member.set(obj, value), value);
var __privateMethod = (obj, member, method) => (__accessCheck(obj, member, "access private method"), method);

// src/index.ts
var index_exports = {};
__export(index_exports, {
  ActivityId: () => ActivityId,
  BehaviorClass: () => BehaviorClass,
  BehaviorId: () => BehaviorId,
  BehaviorInfo: () => BehaviorInfo,
  BehaviorInfoRequest: () => BehaviorInfoRequest,
  BehaviorStatus: () => BehaviorStatus,
  CompoundRequest: () => CompoundRequest,
  CompoundResponse: () => CompoundResponse,
  Event: () => Event,
  EventHeader: () => EventHeader,
  EventHolder: () => EventHolder,
  EventStatus: () => EventStatus,
  Finalize: () => Finalize,
  Initialize: () => Initialize,
  JsonUtils: () => JsonUtils,
  NotificationsRequest: () => NotificationsRequest,
  NotificationsResponse: () => NotificationsResponse,
  PlantUmlInfo: () => PlantUmlInfo,
  PlantUmlInfoRequest: () => PlantUmlInfoRequest,
  Request: () => Request,
  RequestResult: () => RequestResult,
  Reset: () => Reset,
  SendResult: () => SendResult,
  StateMachineId: () => StateMachineId,
  StateMachineInfo: () => StateMachineInfo,
  StateMachineInfoRequest: () => StateMachineInfoRequest,
  StateflowsClient: () => StateflowsClient,
  StateflowsEvent: () => StateflowsEvent
});
module.exports = __toCommonJS(index_exports);

// src/classes/send-result.ts
var SendResult = class {
  constructor(event, status, validation) {
    this.event = event;
    this.status = status;
    this.validation = validation;
  }
};

// src/classes/request-result.ts
var RequestResult = class extends SendResult {
  constructor(response, event, status, validation) {
    super(event, status, validation);
    this.response = response;
  }
};

// src/decorators/stateflows-event.ts
function StateflowsEvent(typeName, eventName = null) {
  return function(target) {
    const constructor = target.prototype.constructor;
    constructor.$type = typeName;
    constructor.eventName = eventName === null ? typeName.split(",")[0] : eventName;
  };
}

// src/events/event.ts
var Event = class {
  get $type() {
    return this.constructor.$type;
  }
  get eventName() {
    return this.constructor.eventName;
  }
};

// src/events/behavior-info.ts
var _BehaviorInfo_decorators, _init, _a;
_BehaviorInfo_decorators = [StateflowsEvent("Stateflows.Common.Events.BehaviorInfo, Stateflows.Common")];
var BehaviorInfo = class extends (_a = Event) {
  constructor() {
    this.behaviorStatus = void 0;
    this.expectedEvents = void 0;
    super(...arguments);
  }
};
_init = __decoratorStart(_a);
BehaviorInfo = __decorateElement(_init, 0, "BehaviorInfo", _BehaviorInfo_decorators, BehaviorInfo);
__runInitializers(_init, 1, BehaviorInfo);

// src/events/request.ts
var Request = class extends Event {
};

// src/classes/event-holder.ts
var EventHolder = class {
  constructor(payload, headers = []) {
    this.payload = payload;
    this.headers = headers;
    this.$type = "Stateflows.Common.EventHolder<>, Stateflows.Common";
    this.$type = this.$type.replace("<>", "`1[[" + payload.$type + "]]");
  }
};

// src/events/compound.request.ts
var _CompoundRequest_decorators, _init2, _a2;
_CompoundRequest_decorators = [StateflowsEvent("Stateflows.Common.CompoundRequest, Stateflows.Common")];
var CompoundRequest = class extends (_a2 = Request) {
  constructor(events) {
    super();
    this.events = [];
    this.events = events.map((event) => new EventHolder(event));
  }
};
_init2 = __decoratorStart(_a2);
CompoundRequest = __decorateElement(_init2, 0, "CompoundRequest", _CompoundRequest_decorators, CompoundRequest);
__runInitializers(_init2, 1, CompoundRequest);

// src/events/behavior-info.request.ts
var _BehaviorInfoRequest_decorators, _init3, _a3;
_BehaviorInfoRequest_decorators = [StateflowsEvent("Stateflows.Common.BehaviorInfoRequest, Stateflows.Common")];
var BehaviorInfoRequest = class extends (_a3 = Request) {
};
_init3 = __decoratorStart(_a3);
BehaviorInfoRequest = __decorateElement(_init3, 0, "BehaviorInfoRequest", _BehaviorInfoRequest_decorators, BehaviorInfoRequest);
__runInitializers(_init3, 1, BehaviorInfoRequest);

// src/events/finalize.ts
var _Finalize_decorators, _init4, _a4;
_Finalize_decorators = [StateflowsEvent("Stateflows.Common.Finalize, Stateflows.Common")];
var Finalize = class extends (_a4 = Event) {
};
_init4 = __decoratorStart(_a4);
Finalize = __decorateElement(_init4, 0, "Finalize", _Finalize_decorators, Finalize);
__runInitializers(_init4, 1, Finalize);

// src/events/reset.ts
var _Reset_decorators, _init5, _a5;
_Reset_decorators = [StateflowsEvent("Stateflows.Common.Reset, Stateflows.Common")];
var Reset = class extends (_a5 = Event) {
  constructor(mode = 2 /* Full */) {
    super();
    this.mode = mode;
  }
};
_init5 = __decoratorStart(_a5);
Reset = __decorateElement(_init5, 0, "Reset", _Reset_decorators, Reset);
__runInitializers(_init5, 1, Reset);

// src/behaviors/behavior.ts
var _transportPromise, _notificationHandlers;
var _Behavior = class _Behavior {
  constructor(transportPromiseOrBehavior, id) {
    this.id = id;
    __privateAdd(this, _transportPromise);
    __privateAdd(this, _notificationHandlers, /* @__PURE__ */ new Map());
    __privateSet(this, _transportPromise, transportPromiseOrBehavior instanceof _Behavior ? __privateGet(transportPromiseOrBehavior, _transportPromise) : __privateSet(this, _transportPromise, transportPromiseOrBehavior));
  }
  async send(event, headers = []) {
    let transport = await __privateGet(this, _transportPromise);
    let result = await transport.send(this.id, new EventHolder(event, headers));
    result.event = result.event.payload;
    return result;
  }
  sendCompound(...events) {
    return this.request(new CompoundRequest(events));
  }
  async request(request, headers = []) {
    let sendResult = await this.send(request, headers);
    request.response = sendResult.event.response;
    let result = new RequestResult(request.response, request, sendResult.status, sendResult.validation);
    return result;
  }
  finalize() {
    return this.send(new Finalize());
  }
  reset(resetMode) {
    return this.send(new Reset(resetMode != null ? resetMode : 2 /* Full */));
  }
  notify(notification) {
    if (__privateGet(this, _notificationHandlers).has(notification.name)) {
      notification.payload.eventName = notification.name;
      let handlers = __privateGet(this, _notificationHandlers).get(notification.name);
      handlers.forEach((handler) => handler(notification.payload));
    }
  }
  async watch(notificationName, handler) {
    let transport = await __privateGet(this, _transportPromise);
    await transport.watch(this, notificationName);
    let handlers = __privateGet(this, _notificationHandlers).has(notificationName) ? __privateGet(this, _notificationHandlers).get(notificationName) : [];
    handlers.push((n) => handler(n));
    __privateGet(this, _notificationHandlers).set(notificationName, handlers);
  }
  async requestAndWatch(request, notificationName, handler) {
    let promise = await this.watch(notificationName, handler);
    let result = await this.request(request);
    handler(result.response);
    return promise;
  }
  async unwatch(notificationName) {
    let transport = await __privateGet(this, _transportPromise);
    await transport.unwatch(this, notificationName);
    __privateGet(this, _notificationHandlers).delete(notificationName);
  }
  getStatus() {
    return this.request(new BehaviorInfoRequest());
  }
  watchStatus(handler) {
    return this.watch(BehaviorInfo.eventName, handler);
  }
  async requestAndWatchStatus(handler) {
    let promise = this.watch(BehaviorInfo.eventName, handler);
    let result = await this.getStatus();
    handler(result.response);
    return promise;
  }
  unwatchStatus() {
    return this.unwatch(BehaviorInfo.eventName);
  }
};
_transportPromise = new WeakMap();
_notificationHandlers = new WeakMap();
var Behavior = _Behavior;

// src/locators/behavior.locator.ts
var BehaviorLocator = class {
  constructor(transportPromise) {
    this.behaviorClasses = [];
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
      this.transportPromise.then((transport) => {
        if (this.behaviorClasses.findIndex(
          (behaviorClass) => behaviorClass.type === behaviorId.behaviorClass.type && behaviorClass.name === behaviorId.behaviorClass.name
        ) !== -1) {
          let behavior = new Behavior(this.transportPromise, behaviorId);
          resolve(behavior);
        } else {
          reject("Behavior not found");
        }
      }).catch((reason) => reject(reason));
    });
  }
};

// src/events/state-machine-info.ts
var _StateMachineInfo_decorators, _init6, _a6;
_StateMachineInfo_decorators = [StateflowsEvent("Stateflows.StateMachines.StateMachineInfo, Stateflows.Common")];
var StateMachineInfo = class extends (_a6 = BehaviorInfo) {
  constructor() {
    this.statesTree = void 0;
    super(...arguments);
  }
};
_init6 = __decoratorStart(_a6);
StateMachineInfo = __decorateElement(_init6, 0, "StateMachineInfo", _StateMachineInfo_decorators, StateMachineInfo);
__runInitializers(_init6, 1, StateMachineInfo);

// src/events/state-machine-info.request.ts
var _StateMachineInfoRequest_decorators, _init7, _a7;
_StateMachineInfoRequest_decorators = [StateflowsEvent("Stateflows.StateMachines.StateMachineInfoRequest, Stateflows.Common")];
var StateMachineInfoRequest = class extends (_a7 = Request) {
};
_init7 = __decoratorStart(_a7);
StateMachineInfoRequest = __decorateElement(_init7, 0, "StateMachineInfoRequest", _StateMachineInfoRequest_decorators, StateMachineInfoRequest);
__runInitializers(_init7, 1, StateMachineInfoRequest);

// src/behaviors/state-machine.ts
var StateMachine = class extends Behavior {
  constructor(behavior) {
    super(behavior, behavior.id);
  }
  async requestAndWatchCurrentState(handler) {
    let promise = await this.watch(StateMachineInfo.eventName, handler);
    let result = await this.request(new StateMachineInfoRequest());
    handler(result.response);
    return promise;
  }
  getCurrentState() {
    return this.request(new StateMachineInfoRequest());
  }
  watchCurrentState(handler) {
    return this.watch(StateMachineInfo.eventName, handler);
  }
  unwatchCurrentState() {
    return this.unwatch(StateMachineInfo.eventName);
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

// src/behaviors/activity.ts
var Activity = class extends Behavior {
  constructor(behavior) {
    super(behavior, behavior.id);
  }
};

// src/locators/activity.locator.ts
var ActivityLocator = class {
  constructor(behaviorLocator) {
    this.behaviorLocator = behaviorLocator;
  }
  locateActivity(id) {
    return new Promise((resolve, reject) => {
      this.behaviorLocator.locateBehavior(id).then((behavior) => resolve(new Activity(behavior))).catch((_) => reject("Activity not found"));
    });
  }
};

// src/classes/stateflows-client.ts
var _behaviorLocator, _stateMachineLocator, _activityLocator;
var StateflowsClient = class {
  constructor(transportFactory) {
    this.transportFactory = transportFactory;
    __privateAdd(this, _behaviorLocator, null);
    __privateAdd(this, _stateMachineLocator, null);
    __privateAdd(this, _activityLocator, null);
  }
  get behaviorLocator() {
    var _a13;
    return (_a13 = __privateGet(this, _behaviorLocator)) != null ? _a13 : __privateSet(this, _behaviorLocator, new BehaviorLocator(this.transportFactory.getTransport()));
  }
  get stateMachineLocator() {
    var _a13;
    return (_a13 = __privateGet(this, _stateMachineLocator)) != null ? _a13 : __privateSet(this, _stateMachineLocator, new StateMachineLocator(this.behaviorLocator));
  }
  get activityLocator() {
    var _a13;
    return (_a13 = __privateGet(this, _activityLocator)) != null ? _a13 : __privateSet(this, _activityLocator, new ActivityLocator(this.behaviorLocator));
  }
};
_behaviorLocator = new WeakMap();
_stateMachineLocator = new WeakMap();
_activityLocator = new WeakMap();

// src/classes/event-header.ts
var EventHeader = class {
};

// src/ids/behavior.class.ts
var BehaviorClass = class {
  constructor(type, name) {
    this.type = type;
    this.name = name;
    this.$type = "Stateflows.BehaviorClass, Stateflows.Common";
  }
};

// src/utils/json.utils.ts
var JsonUtils = class _JsonUtils {
  static stringify(object) {
    const replacer = (key, value) => value instanceof Object && !(value instanceof Array) ? Object.keys(value).sort().reduce(
      (sorted, key2) => {
        sorted[key2] = value[key2];
        return sorted;
      },
      {}
    ) : value;
    return JSON.stringify(object, replacer);
  }
  static parse(json) {
    return JSON.parse(json);
  }
  static deepClone(object) {
    return _JsonUtils.parse(_JsonUtils.stringify(object));
  }
};

// src/ids/behavior.id.ts
var BehaviorId = class {
  constructor(behaviorClass, instance) {
    this.behaviorClass = behaviorClass;
    this.instance = instance;
    this.$type = "Stateflows.BehaviorId, Stateflows.Common";
  }
  toString() {
    return JsonUtils.stringify(this);
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

// src/events/compound.response.ts
var CompoundResponse = class {
  constructor(results) {
    this.results = results;
  }
};

// src/events/initialize.ts
var _Initialize_decorators, _init8, _a8;
_Initialize_decorators = [StateflowsEvent("Stateflows.Common.Initialize, Stateflows.Common")];
var Initialize = class extends (_a8 = Event) {
};
_init8 = __decoratorStart(_a8);
Initialize = __decorateElement(_init8, 0, "Initialize", _Initialize_decorators, Initialize);
__runInitializers(_init8, 1, Initialize);

// src/events/plant-uml-info.request.ts
var _PlantUmlInfoRequest_decorators, _init9, _a9;
_PlantUmlInfoRequest_decorators = [StateflowsEvent("Stateflows.Extensions.PlantUml.Events.PlantUmlInfoRequest, Stateflows.Extensions.PlantUml")];
var PlantUmlInfoRequest = class extends (_a9 = Request) {
};
_init9 = __decoratorStart(_a9);
PlantUmlInfoRequest = __decorateElement(_init9, 0, "PlantUmlInfoRequest", _PlantUmlInfoRequest_decorators, PlantUmlInfoRequest);
__runInitializers(_init9, 1, PlantUmlInfoRequest);

// src/events/plant-uml-info.ts
var _PlantUmlInfo_decorators, _init10, _a10;
_PlantUmlInfo_decorators = [StateflowsEvent("Stateflows.Extensions.PlantUml.Events.PlantUmlInfo, Stateflows.Extensions.PlantUml")];
var PlantUmlInfo = class extends (_a10 = Event) {
  constructor() {
    this.plantUml = void 0;
    super(...arguments);
  }
};
_init10 = __decoratorStart(_a10);
PlantUmlInfo = __decorateElement(_init10, 0, "PlantUmlInfo", _PlantUmlInfo_decorators, PlantUmlInfo);
__runInitializers(_init10, 1, PlantUmlInfo);

// src/enums/event-status.ts
var EventStatus = /* @__PURE__ */ ((EventStatus2) => {
  EventStatus2[EventStatus2["Initialized"] = 0] = "Initialized";
  EventStatus2[EventStatus2["NotInitialized"] = 1] = "NotInitialized";
  EventStatus2[EventStatus2["Undelivered"] = 2] = "Undelivered";
  EventStatus2[EventStatus2["Rejected"] = 3] = "Rejected";
  EventStatus2[EventStatus2["Invalid"] = 4] = "Invalid";
  EventStatus2[EventStatus2["Deferred"] = 5] = "Deferred";
  EventStatus2[EventStatus2["Consumed"] = 6] = "Consumed";
  EventStatus2[EventStatus2["NotConsumed"] = 7] = "NotConsumed";
  EventStatus2[EventStatus2["Omitted"] = 8] = "Omitted";
  EventStatus2[EventStatus2["Failed"] = 9] = "Failed";
  EventStatus2[EventStatus2["Forwarded"] = 10] = "Forwarded";
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

// src/events/notifications.request.ts
var _NotificationsRequest_decorators, _init11, _a11;
_NotificationsRequest_decorators = [StateflowsEvent("Stateflows.Common.NotificationsRequest, Stateflows.Common")];
var NotificationsRequest = class extends (_a11 = Request) {
};
_init11 = __decoratorStart(_a11);
NotificationsRequest = __decorateElement(_init11, 0, "NotificationsRequest", _NotificationsRequest_decorators, NotificationsRequest);
__runInitializers(_init11, 1, NotificationsRequest);

// src/events/notifications.response.ts
var _NotificationsResponse_decorators, _init12, _a12;
_NotificationsResponse_decorators = [StateflowsEvent("Stateflows.Common.NotificationsResponse, Stateflows.Common")];
var NotificationsResponse = class extends (_a12 = Event) {
};
_init12 = __decoratorStart(_a12);
NotificationsResponse = __decorateElement(_init12, 0, "NotificationsResponse", _NotificationsResponse_decorators, NotificationsResponse);
__runInitializers(_init12, 1, NotificationsResponse);
// Annotate the CommonJS export names for ESM import in node:
0 && (module.exports = {
  ActivityId,
  BehaviorClass,
  BehaviorId,
  BehaviorInfo,
  BehaviorInfoRequest,
  BehaviorStatus,
  CompoundRequest,
  CompoundResponse,
  Event,
  EventHeader,
  EventHolder,
  EventStatus,
  Finalize,
  Initialize,
  JsonUtils,
  NotificationsRequest,
  NotificationsResponse,
  PlantUmlInfo,
  PlantUmlInfoRequest,
  Request,
  RequestResult,
  Reset,
  SendResult,
  StateMachineId,
  StateMachineInfo,
  StateMachineInfoRequest,
  StateflowsClient,
  StateflowsEvent
});
//# sourceMappingURL=index.js.map
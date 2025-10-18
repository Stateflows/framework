const machineName = "Doc";
let currentInstance = "";
let notificationsSource = null;

function baseUrl(instance) {
    return `/stateflows/stateMachines/${encodeURIComponent(machineName)}/${encodeURIComponent(instance)}`;
}

function setElementText(id, text) {
    const el = document.getElementById(id);
    if (el) el.textContent = JSON.stringify(text, null, 2);
}

async function fetchJson(url, options) {
    const r = await fetch(url, options);
    const txt = await r.text();
    return txt
        ? JSON.parse(txt)
        : txt;
}

function setButtons(expectedEvents) {
    // expectedEvents: array of short names returned by GET status
    const has = (name) => expectedEvents && expectedEvents.indexOf(name) !== -1;

    document.getElementById("btn-review").classList.toggle("hide", !has("Stateflows.Examples.Common.Events.Review"));
    document.getElementById("btn-approve-manager").classList.toggle("hide", !has("Stateflows.Examples.Common.Events.Approve"));
    document.getElementById("btn-approve-finance").classList.toggle("hide", !has("Stateflows.Examples.Common.Events.Approve"));
    document.getElementById("btn-pay").classList.toggle("hide", !has("Stateflows.Examples.Common.Events.Pay"));
    document.getElementById("btn-reject").classList.toggle("hide", !has("Stateflows.Examples.Common.Events.Reject"));
}

function generateGUID() {
    const array = new Uint32Array(8);
    window.crypto.getRandomValues(array);
    let str = '';
    for (let i = 0; i < array.length; i++) {
        str += array[i].toString(16).padStart(8, '0');
    }
    return str;
}

async function create() {
    currentInstance = generateGUID();

    try {
        startNotificationsListener();
        
        await fetch(`${baseUrl(currentInstance)}/initialize`, {
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            method: "POST",
            body: JSON.stringify({ payload: {} }),
        });
        
    } catch (err) {
        alert("Create failed: " + err);
    }
}

async function review() {
    const content = prompt("Provide a review (minimum 8 characters):");
    if (content === null) return; // cancelled

    const result = await fetchJson(`${baseUrl(currentInstance)}/review`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Event: { Content : content, Rating: 42 }})
    });

    console.log(result);
    if (result && result.status && result.statusText === "Invalid" && !result.eventValidation.isValid) {
        // adapt to your API's validation result shape
        alert("Validation error: " + result.eventValidation.validationResults[0].errorMessage);
    }
}

async function approve(role) {
    // role -> header X-Role: Manager | Finance (adjust header name if different)
    await fetchJson(`${baseUrl(currentInstance)}/approve`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "X-Role": role
        },
        body: JSON.stringify({ }),
    });
}

async function pay() {
    await fetchJson(`${baseUrl(currentInstance)}/pay`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ Event: { Amount: 500 }})
    });
}

async function rejectDoc() {
    await fetchJson(`${baseUrl(currentInstance)}/reject`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({})
    });
}

function startNotificationsListener() {
    stopNotificationsListener();

    notificationsSource = new EventSource(`${baseUrl(currentInstance)}/notifications?stream=true&names=Stateflows.StateMachines.StateMachineInfo&names=PlantUmlInfo&names=Stateflows.Examples.Common.Events.RejectionNotification&names=Stateflows.Examples.Common.Events.InvoiceNotification`);

    notificationsSource.addEventListener("PlantUmlInfo", (e) => {
        let data = JSON.parse(e.data);
        const p = document.getElementById("plantUml");
        p.src = data.Payload.PNGUrl;
    });

    notificationsSource.addEventListener("Stateflows.StateMachines.StateMachineInfo", (e) => {
        let data = JSON.parse(e.data);
        setButtons(data.Payload.ExpectedEvents);
    });

    notificationsSource.addEventListener("Stateflows.Examples.Common.Events.RejectionNotification", (e) => {
        let data = JSON.parse(e.data);
        alert(data.Payload.Reason);
    });

    notificationsSource.addEventListener("Stateflows.Examples.Common.Events.InvoiceNotification", (e) => {
        alert("Invoice is created, payment time!");
    });
}

function stopNotificationsListener() {
    if (notificationsSource != null) {
        notificationsSource.Close();
    }
}

// expose simple cleanup when navigating away
window.addEventListener("beforeunload", () => stopNotificationsListener());

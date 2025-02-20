chrome.devtools.inspectedWindow.getResources((resources) => {
  let div = document.createElement('div');
  div.innerHTML = `<p>Loading...</p>`;
  document.body.appendChild(div);

  chrome.devtools.inspectedWindow.eval("window.location.origin", function(result, isException) {
    fetch(result + "/stateflows/console/manifest").then(response => {
      response.json().then(manifest => {
        if (manifest.enabled) {
          let iframe = document.createElement('iframe');
          iframe.src = result + manifest.url;
          document.body.appendChild(iframe);
        }
      });
    })
    .catch(_ => {
      div.innerHTML = `<p>Stateflows Console is not available for current page.</p><p>To use it, do the following: <ol><li>Install <a href="https://www.nuget.org/packages/Stateflows.Tools.Console" target="_blank">Stateflows.Tools.Console</a> NuGet package in your application,</li><li>Add <pre>app.UseStateflowsConsole()</pre> to your Program.cs file.</li></ol></p>`;
    });
  });
});

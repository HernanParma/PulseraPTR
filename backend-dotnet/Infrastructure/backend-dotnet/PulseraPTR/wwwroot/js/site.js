(() => {
  const hubUrl = "/hubs/pulsera";
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

  connection.on("nuevaMedicion", () => {
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("medicion");
  });
  connection.on("nuevaAlerta", () => {
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("alerta");
  });
  connection.on("nuevoEventoSos", () => {
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("sos");
  });

  connection.start()
    .then(() => connection.invoke("joinDashboard"))
    .catch(() => { /* sin conexión en páginas estáticas */ });
})();

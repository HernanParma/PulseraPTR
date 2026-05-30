(() => {
  function escapeHtml(text) {
    const d = document.createElement("div");
    d.textContent = text ?? "";
    return d.innerHTML;
  }

  function refreshNavAlertasBadge() {
    const badge = document.getElementById("navAlertasBadge");
    if (!badge) return;
    fetch("/api/alertas/contador-sin-leer")
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then((data) => {
        const n = data?.count ?? 0;
        const bell = document.getElementById("navAlertasBell");
        if (n > 0) {
          badge.textContent = n > 99 ? "99+" : String(n);
          badge.classList.remove("d-none");
          badge.removeAttribute("hidden");
          if (bell) {
            const t = n === 1 ? "1 alerta sin atender" : `${n} alertas sin atender`;
            bell.title = t;
            bell.setAttribute("aria-label", t);
          }
        } else {
          badge.classList.add("d-none");
          badge.setAttribute("hidden", "");
          if (bell) {
            bell.title = "Alertas";
            bell.setAttribute("aria-label", "Alertas");
          }
        }
      })
      .catch(() => { /* ignorar si no hay API */ });
  }

  function renderNavAlertasList(alertas) {
    const list = document.getElementById("navAlertasList");
    if (!list) return;

    if (!alertas?.length) {
      list.innerHTML =
        '<div class="dropdown-item-text text-muted small px-3 py-3 text-center">Sin alertas recientes</div>';
      return;
    }

    list.innerHTML = alertas
      .map((a) => {
        const unread = !a.leida;
        const fecha = a.fechaHora
          ? new Date(a.fechaHora).toLocaleString("es-AR", {
              day: "2-digit",
              month: "2-digit",
              hour: "2-digit",
              minute: "2-digit",
            })
          : "";
        const readBtn = unread
          ? `<button type="button" class="btn btn-sm btn-link btn-nav-alerta btn-nav-alerta--read" data-alerta-read="${a.id}" title="Marcar leída" aria-label="Marcar leída"><i class="bi bi-check-lg"></i></button>`
          : "";
        return `<div class="nav-alerta-item${unread ? " nav-alerta-item--unread" : ""}" data-alerta-id="${a.id}">
          <div class="nav-alerta-body">
            <div class="fw-semibold small text-truncate">${escapeHtml(a.pacienteNombre || "Paciente")}</div>
            <div class="nav-alerta-meta">${escapeHtml(fecha)} · ${escapeHtml(String(a.estado ?? ""))}</div>
            <div class="nav-alerta-msg">${escapeHtml(a.mensaje || "")}</div>
          </div>
          <div class="nav-alerta-actions">
            ${readBtn}
            <button type="button" class="btn btn-sm btn-link btn-nav-alerta" data-alerta-delete="${a.id}" title="Eliminar" aria-label="Eliminar alerta"><i class="bi bi-trash"></i></button>
          </div>
        </div>`;
      })
      .join("");
  }

  function loadNavAlertasList() {
    const list = document.getElementById("navAlertasList");
    if (!list) return Promise.resolve();
    return fetch("/api/alertas/recientes?cantidad=12")
      .then((r) => (r.ok ? r.json() : Promise.reject()))
      .then((data) => {
        renderNavAlertasList(data);
      })
      .catch(() => {
        list.innerHTML =
          '<div class="dropdown-item-text text-danger small px-3 py-2">No se pudieron cargar las alertas</div>';
      });
  }

  function marcarAlertaLeida(id) {
    return fetch(`/api/alertas/${id}/leer`, { method: "PUT" }).then((r) => {
      if (!r.ok) throw new Error("marcar leida");
    });
  }

  function eliminarAlerta(id) {
    return fetch(`/api/alertas/${id}`, { method: "DELETE" }).then((r) => {
      if (!r.ok) throw new Error("eliminar");
    });
  }

  function refreshNavAlertas() {
    refreshNavAlertasBadge();
    return loadNavAlertasList();
  }

  window.pulseraRefreshAlertasBadge = refreshNavAlertasBadge;
  window.pulseraRefreshNavAlertas = refreshNavAlertas;

  document.addEventListener("click", (e) => {
    const del = e.target.closest("[data-alerta-delete]");
    if (del) {
      e.preventDefault();
      e.stopPropagation();
      const id = del.getAttribute("data-alerta-delete");
      if (!id || !confirm("¿Eliminar esta alerta?")) return;
      eliminarAlerta(id)
        .then(() => refreshNavAlertas())
        .catch(() => alert("No se pudo eliminar la alerta."));
      return;
    }

    const read = e.target.closest("[data-alerta-read]");
    if (read) {
      e.preventDefault();
      e.stopPropagation();
      const id = read.getAttribute("data-alerta-read");
      if (!id) return;
      marcarAlertaLeida(id)
        .then(() => refreshNavAlertas())
        .catch(() => alert("No se pudo marcar como leída."));
    }
  });

  const bell = document.getElementById("navAlertasBell");
  if (bell) {
    bell.addEventListener("show.bs.dropdown", () => {
      loadNavAlertasList();
    });
  }

  const hubUrl = "/hubs/pulsera";
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

  connection.on("nuevaMedicion", () => {
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("medicion");
  });
  connection.on("nuevaAlerta", () => {
    refreshNavAlertas();
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("alerta");
  });
  connection.on("nuevoEventoSos", () => {
    refreshNavAlertas();
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("sos");
  });
  connection.on("glucemiaActualizada", () => {
    if (window.pulseraRealtimeReload) window.pulseraRealtimeReload("glucemia");
  });

  connection
    .start()
    .then(() => connection.invoke("joinDashboard"))
    .then(() => refreshNavAlertas())
    .catch(() => { /* sin conexión en páginas estáticas */ });
})();

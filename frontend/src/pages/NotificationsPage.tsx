import { useCallback, useEffect, useState } from "react";
import { apiFetch, formatDateTime } from "../api";
import type { FlashType, NotificationItem } from "../types";

type Notify = (type: FlashType, text: string) => void;

export function NotificationsPage({
  notify,
  onUnreadChange,
  onOpenBooking
}: {
  notify: Notify;
  onUnreadChange: (count: number) => void;
  onOpenBooking: (bookingId: string) => void;
}) {
  const [items, setItems] = useState<NotificationItem[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const notifications = await apiFetch<NotificationItem[]>("/notifications/me");
      setItems(notifications);
      onUnreadChange(notifications.filter((item) => !item.isRead).length);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Notifikacije nisu ucitane.");
    } finally {
      setLoading(false);
    }
  }, [notify, onUnreadChange]);

  useEffect(() => {
    void load();
  }, [load]);

  const openNotification = async (item: NotificationItem) => {
    setSelectedId((current) => (current === item.id ? null : item.id));
    if (item.isRead) return;

    try {
      await apiFetch<void>(`/notifications/${item.id}/read`, { method: "POST" });
      setItems((current) => {
        const next = current.map((existing) => (existing.id === item.id ? { ...existing, isRead: true } : existing));
        onUnreadChange(next.filter((existing) => !existing.isRead).length);
        return next;
      });
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Oznacavanje nije uspelo.");
    }
  };

  const markAll = async () => {
    try {
      await apiFetch<{ marked: number }>("/notifications/read-all", { method: "POST" });
      await load();
      onUnreadChange(0);
      notify("success", "Notifikacije su oznacene kao procitane.");
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Oznacavanje nije uspelo.");
    }
  };

  return (
    <section className="page-card p-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="h5 mb-0">Notifikacije</h2>
        <div className="d-flex gap-2">
          <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void load()} disabled={loading}>
            Osvezi
          </button>
          <button className="btn btn-sm btn-primary" type="button" onClick={() => void markAll()}>
            Oznaci sve kao procitano
          </button>
        </div>
      </div>
      <div className="list-group notification-list">
        {items.map((item) => {
          const expanded = selectedId === item.id;
          const normalized = normalizeNotification(item);
          return (
            <div className={`list-group-item list-group-item-action text-start ${item.isRead ? "" : "fw-semibold"}`} key={item.id} role="button" tabIndex={0} onClick={() => void openNotification(item)}>
              <div className="d-flex justify-content-between gap-3">
                <div>
                  <div>{normalized.title}</div>
                  <div className="small-muted">{formatDateTime(item.createdAt)}</div>
                </div>
                <span className={`status-chip ${item.isRead ? "success" : "warn"}`}>{item.isRead ? "Procitano" : "Novo"}</span>
              </div>
              {expanded && (
                <div className="mt-2">
                  <div className="pre-wrap">{normalized.message}</div>
                  {normalized.bookingId && (
                    <button
                      className="btn btn-sm btn-outline-primary mt-2"
                      type="button"
                      onClick={(event) => {
                        event.stopPropagation();
                        onOpenBooking(normalized.bookingId!);
                      }}
                    >
                      Otvori booking
                    </button>
                  )}
                </div>
              )}
            </div>
          );
        })}
      </div>
      {items.length === 0 && <div className="small-muted">Nema notifikacija.</div>}
    </section>
  );
}

const guidRegex = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;
const guidGlobalRegex = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi;

function normalizeNotification(item: NotificationItem) {
  const bookingId = extractBookingId(item.message);
  const titleMap: Record<string, string> = {
    "Booking approved": "Rezervacija je odobrena",
    "Booking rejected": "Rezervacija je odbijena",
    "Booking cancelled": "Rezervacija je otkazana",
    "New booking request": "Novi zahtev za rezervaciju",
    "Napomena je azurirana": "Napomena je azurirana"
  };

  let message = item.message.replace(/BookingId:\s*[0-9a-f-]{36}/gi, "").trim();
  message = message.replace(/^New booking [0-9a-f-]{36} for your ride [0-9a-f-]{36} was created and is pending approval\.$/i, "Novi putnik zeli da rezervise voznju.");
  message = message.replace(/^Passenger cancelled booking [0-9a-f-]{36} for your ride [0-9a-f-]{36}\.$/i, "Putnik je otkazao rezervaciju.");
  message = message.replace(/^Your booking [0-9a-f-]{36} for ride [0-9a-f-]{36} was approved\.$/i, "Vozac je odobrio rezervaciju.");
  message = message.replace(/^Your booking [0-9a-f-]{36} for ride [0-9a-f-]{36} was automatically approved\.$/i, "Rezervacija je automatski odobrena.");
  message = message.replace(/^Your booking [0-9a-f-]{36} for ride [0-9a-f-]{36} was rejected\.$/i, "Vozac je odbio rezervaciju.");
  message = message.replace(/^Your booking [0-9a-f-]{36} for ride [0-9a-f-]{36} was cancelled by driver\.$/i, "Vozac je otkazao rezervaciju.");
  message = message.replace(guidGlobalRegex, "booking");

  return {
    title: titleMap[item.title] || item.title,
    message,
    bookingId
  };
}

function extractBookingId(message: string) {
  const explicit = /BookingId:\s*([0-9a-f-]{36})/i.exec(message);
  if (explicit) return explicit[1];
  const generic = guidRegex.exec(message);
  return generic?.[0];
}

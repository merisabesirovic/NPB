import { useCallback, useEffect, useMemo, useState } from "react";
import { apiFetch, currency, formatDateTime, shortId } from "../api";
import { ProfileLink } from "../components/ProfileLink";
import type { Booking, FlashType, Ride } from "../types";
import { statusClass, statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;

const rideStatuses = ["Scheduled", "InProgress", "Completed", "Cancelled"];

export function MyRidesPage({ notify, onOpenProfile }: { notify: Notify; onOpenProfile: (userId: string) => void }) {
  const [rides, setRides] = useState<Ride[]>([]);
  const [driverBookings, setDriverBookings] = useState<Booking[]>([]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [rideData, bookingData] = await Promise.all([apiFetch<Ride[]>("/rides/me"), apiFetch<Booking[]>("/bookings/driver/requests")]);
      setRides((rideData || []).filter((ride) => ride.rideStatus !== "Cancelled"));
      setDriverBookings(bookingData || []);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Moje voznje nisu ucitane.");
    } finally {
      setLoading(false);
    }
  }, [notify]);

  useEffect(() => {
    void load();
  }, [load]);

  const pendingBookings = useMemo(() => driverBookings.filter((booking) => booking.bookingStatus === "Pending"), [driverBookings]);
  const approvedBookings = useMemo(() => driverBookings.filter((booking) => booking.bookingStatus === "Approved"), [driverBookings]);

  const changeStatus = async (ride: Ride, status: string) => {
    try {
      await apiFetch<Ride>(`/rides/${ride.id}/status`, { method: "PUT", body: { status } });
      notify("success", "Status je promenjen.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Promena statusa nije uspela.");
    }
  };

  const cancelRide = async (ride: Ride) => {
    if (!window.confirm("Otkazati voznju?")) return;
    try {
      await apiFetch<void>(`/rides/${ride.id}`, { method: "DELETE" });
      notify("success", "Voznja je otkazana.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Otkazivanje nije uspelo.");
    }
  };

  const bookingAction = async (booking: Booking, action: "approve" | "reject" | "mark-cash-paid") => {
    const path = action === "mark-cash-paid" ? `/driver/payments/${booking.id}/mark-cash-paid` : `/bookings/${booking.id}/${action}`;
    try {
      await apiFetch<Booking>(path, { method: "POST" });
      notify("success", "Akcija je izvrsena.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Akcija nad rezervacijom nije uspela.");
    }
  };

  return (
    <div className="row g-3">
      <div className="col-xl-7">
        <section className="page-card p-4">
          <div className="d-flex justify-content-between align-items-center mb-3">
            <h2 className="h5 mb-0">Moje voznje</h2>
            <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void load()} disabled={loading}>
              Osvezi
            </button>
          </div>
          <div className="table-responsive">
            <table className="table table-sm align-middle">
              <thead>
                <tr>
                  <th>Ruta</th>
                  <th>Polazak</th>
                  <th>Cena</th>
                  <th>Status</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {rides.map((ride) => (
                  <tr key={ride.id}>
                    <td>
                      <div className="fw-semibold">{ride.startAddress}</div>
                      <div className="small-muted">do {ride.destinationAddress}</div>
                    </td>
                    <td>{formatDateTime(ride.departureDateTime)}</td>
                    <td>{currency(ride.pricePerSeat)}</td>
                    <td>
                      <select className="form-select form-select-sm" value={rideStatusValue(ride.rideStatus)} onChange={(event) => void changeStatus(ride, event.target.value)}>
                        {rideStatuses.map((status) => (
                          <option key={status} value={status}>
                            {statusLabel(status)}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="text-end">
                      {ride.rideStatus !== "Cancelled" && ride.rideStatus !== "Completed" && (
                        <button className="btn btn-sm btn-outline-danger" type="button" onClick={() => void cancelRide(ride)}>
                          Otkazi
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {rides.length === 0 && <div className="small-muted">Nemate kreirane voznje.</div>}
        </section>
      </div>

      <div className="col-xl-5">
        <section className="page-card p-4 mb-3">
          <h2 className="h5 mb-3">Novi zahtevi za rezervaciju</h2>
          <BookingRequestList bookings={pendingBookings} onOpenProfile={onOpenProfile} onAction={bookingAction} emptyText="Nema zahteva na cekanju." />
        </section>

        <section className="page-card p-4">
          <h2 className="h5 mb-3">Odobrene rezervacije</h2>
          <BookingRequestList bookings={approvedBookings} onOpenProfile={onOpenProfile} onAction={bookingAction} emptyText="Nema odobrenih rezervacija." />
        </section>
      </div>
    </div>
  );
}

function rideStatusValue(status: string) {
  return status === "BookingOpen" || status === "BookingClosed" ? "Scheduled" : status;
}

function BookingRequestList({
  bookings,
  onOpenProfile,
  onAction,
  emptyText
}: {
  bookings: Booking[];
  onOpenProfile: (userId: string) => void;
  onAction: (booking: Booking, action: "approve" | "reject" | "mark-cash-paid") => void;
  emptyText: string;
}) {
  if (bookings.length === 0) return <div className="small-muted">{emptyText}</div>;

  return (
    <div className="list-group">
      {bookings.map((booking) => (
        <div className="list-group-item" key={booking.id}>
          <div className="d-flex justify-content-between gap-2">
            <div>
              <div className="fw-semibold">
                <ProfileLink
                  userId={booking.passengerId}
                  firstName={booking.passengerFirstName}
                  lastName={booking.passengerLastName}
                  email={booking.passengerEmail}
                  isVerified={booking.passengerIsVerified}
                  onOpen={onOpenProfile}
                />
              </div>
              <div className="small-muted">
                {booking.startAddress || shortId(booking.rideId)} {"->"} {booking.destinationAddress || ""} - {booking.seatsReserved} sedista - {currency(booking.totalPrice)}
              </div>
              <div className="small-muted">{formatDateTime(booking.departureDateTime || booking.rideDate)}</div>
              {booking.note && (
                <div className="alert alert-light border py-2 mt-2 mb-0">
                  <div className="stat-label">Napomena</div>
                  <div className="pre-wrap">{booking.note}</div>
                </div>
              )}
              {booking.refundAmount !== undefined && booking.refundAmount !== null && (
                <div className="alert alert-info py-2 mt-2 mb-0">
                  <div className="stat-label">Refundacija</div>
                  <div className="fw-semibold">{currency(booking.refundAmount)}</div>
                </div>
              )}
            </div>
            <span className={`status-chip ${statusClass(booking.bookingStatus)}`}>{statusLabel(booking.bookingStatus)}</span>
          </div>
          <div className="d-flex flex-wrap gap-2 mt-3">
            {booking.bookingStatus === "Pending" && (
              <>
                <button className="btn btn-sm btn-outline-success" type="button" onClick={() => onAction(booking, "approve")}>
                  Prihvati
                </button>
                <button className="btn btn-sm btn-outline-danger" type="button" onClick={() => onAction(booking, "reject")}>
                  Odbij
                </button>
              </>
            )}
            {booking.bookingStatus === "Approved" && booking.paymentStatus !== "Paid" && booking.paymentMethod !== "Online" && (
              <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => onAction(booking, "mark-cash-paid")}>
                Oznaci voznju kao placenu
              </button>
            )}
            {booking.bookingStatus === "Approved" && booking.paymentStatus !== "Paid" && booking.paymentMethod === "Online" && <span className="small-muted">Ceka se online placanje putnika.</span>}
          </div>
        </div>
      ))}
    </div>
  );
}

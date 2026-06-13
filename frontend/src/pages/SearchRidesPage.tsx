import { FormEvent, useCallback, useEffect, useState } from "react";
import { apiFetch, buildQuery, currency, formatDateTime } from "../api";
import { CityDatalist, CityInput } from "../components/CityInput";
import { ProfileLink } from "../components/ProfileLink";
import type { Booking, FlashType, Ride, RideListResponse, UserProfile } from "../types";
import { asText } from "../utils/forms";
import { statusClass, statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;

export function SearchRidesPage({
  notify,
  profile,
  onOpenProfile,
  onOpenBooking
}: {
  notify: Notify;
  profile: UserProfile | null;
  onOpenProfile: (userId: string) => void;
  onOpenBooking: (bookingId: string) => void;
}) {
  const [filters, setFilters] = useState({
    startAddress: "",
    destinationAddress: "",
    departureDate: "",
    minAvailableSeats: "1",
    minPrice: "",
    maxPrice: "",
    startLatitude: "",
    startLongitude: "",
    startRadiusKm: "",
    destinationLatitude: "",
    destinationLongitude: "",
    destinationRadiusKm: ""
  });
  const [rides, setRides] = useState<Ride[]>([]);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);

  const setField = (key: keyof typeof filters, value: string) => {
    setFilters((current) => ({ ...current, [key]: value }));
  };

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const query = buildQuery({
        startAddress: filters.startAddress,
        destinationAddress: filters.destinationAddress,
        departureDate: filters.departureDate,
        minAvailableSeats: filters.minAvailableSeats,
        minPrice: filters.minPrice,
        maxPrice: filters.maxPrice,
        startLatitude: filters.startLatitude,
        startLongitude: filters.startLongitude,
        startRadiusKm: filters.startLatitude && filters.startLongitude ? filters.startRadiusKm : "",
        destinationLatitude: filters.destinationLatitude,
        destinationLongitude: filters.destinationLongitude,
        destinationRadiusKm: filters.destinationLatitude && filters.destinationLongitude ? filters.destinationRadiusKm : "",
        page: 1,
        pageSize: 50
      });
      const [response, bookingData] = await Promise.all([
        apiFetch<RideListResponse>(`/rides/search${query}`),
        apiFetch<Booking[]>("/bookings/me").catch(() => [])
      ]);
      setRides((response.items || []).filter((ride) => ride.rideStatus !== "Cancelled"));
      setBookings(bookingData || []);
      setTotalCount(response.totalCount || 0);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Pretraga nije uspela.");
    } finally {
      setLoading(false);
    }
  }, [filters, notify]);

  useEffect(() => {
    void load();
  }, []);

  const submit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void load();
  };

  return (
    <div className="row g-3">
      <CityDatalist />
      <div className="col-xl-3">
        <section className="page-card p-4">
          <h2 className="h5 mb-3">Pretraga voznji</h2>
          <form onSubmit={submit}>
            <div className="mb-3">
              <CityInput id="search-start-city" label="Polazak" value={filters.startAddress} onChange={(value) => setField("startAddress", value)} />
            </div>
            <div className="mb-3">
              <CityInput id="search-destination-city" label="Odrediste" value={filters.destinationAddress} onChange={(value) => setField("destinationAddress", value)} />
            </div>
            <div className="mb-3">
              <label className="form-label" htmlFor="search-date">
                Datum
              </label>
              <input className="form-control" id="search-date" type="date" value={filters.departureDate} onChange={(event) => setField("departureDate", event.target.value)} />
            </div>
            <div className="row g-2">
              <div className="col-6">
                <label className="form-label" htmlFor="search-seats">
                  Min sedista
                </label>
                <input className="form-control" id="search-seats" type="number" min="1" value={filters.minAvailableSeats} onChange={(event) => setField("minAvailableSeats", event.target.value)} />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="search-max-price">
                  Max cena (RSD)
                </label>
                <input className="form-control" id="search-max-price" type="number" min="0" step="1" value={filters.maxPrice} onChange={(event) => setField("maxPrice", event.target.value)} />
              </div>
              <div className="col-12">
                <label className="form-label" htmlFor="search-min-price">
                  Min cena (RSD)
                </label>
                <input className="form-control" id="search-min-price" type="number" min="0" step="1" value={filters.minPrice} onChange={(event) => setField("minPrice", event.target.value)} />
              </div>
            </div>

            <div className="border-top mt-3 pt-3">
              <div className="section-title">Dodatna preciznost polaska</div>
              <div className="row g-2">
                <div className="col-6">
                  <label className="form-label" htmlFor="search-start-lat">
                    Lat
                  </label>
                  <input className="form-control" id="search-start-lat" value={filters.startLatitude} onChange={(event) => setField("startLatitude", event.target.value)} />
                </div>
                <div className="col-6">
                  <label className="form-label" htmlFor="search-start-lng">
                    Lng
                  </label>
                  <input className="form-control" id="search-start-lng" value={filters.startLongitude} onChange={(event) => setField("startLongitude", event.target.value)} />
                </div>
                <div className="col-12">
                  <label className="form-label" htmlFor="search-start-radius">
                    Radijus od (km)
                  </label>
                  <input className="form-control" id="search-start-radius" type="number" min="1" value={filters.startRadiusKm} onChange={(event) => setField("startRadiusKm", event.target.value)} />
                </div>
              </div>
            </div>

            <div className="border-top mt-3 pt-3">
              <div className="section-title">Dodatna preciznost odredista</div>
              <div className="row g-2">
                <div className="col-6">
                  <label className="form-label" htmlFor="search-destination-lat">
                    Lat
                  </label>
                  <input className="form-control" id="search-destination-lat" value={filters.destinationLatitude} onChange={(event) => setField("destinationLatitude", event.target.value)} />
                </div>
                <div className="col-6">
                  <label className="form-label" htmlFor="search-destination-lng">
                    Lng
                  </label>
                  <input className="form-control" id="search-destination-lng" value={filters.destinationLongitude} onChange={(event) => setField("destinationLongitude", event.target.value)} />
                </div>
                <div className="col-12">
                  <label className="form-label" htmlFor="search-destination-radius">
                    Radijus od (km)
                  </label>
                  <input className="form-control" id="search-destination-radius" type="number" min="1" value={filters.destinationRadiusKm} onChange={(event) => setField("destinationRadiusKm", event.target.value)} />
                </div>
              </div>
            </div>

            <button className="btn btn-primary w-100 mt-3" type="submit" disabled={loading}>
              {loading ? "Pretraga..." : "Pretrazi"}
            </button>
          </form>
        </section>
      </div>

      <div className="col-xl-9">
        <section className="page-card p-4">
          <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <h2 className="h5 mb-0">Rezultati</h2>
            <span className="status-chip">{totalCount} ukupno</span>
          </div>
          {rides.length === 0 ? (
            <div className="small-muted">Nema voznji za trenutne filtere.</div>
          ) : (
            <div className="ride-grid">
              {rides.map((ride) => (
                <RideCard
                  key={ride.id}
                  ride={ride}
                  profile={profile}
                  existingBooking={bookings.find((booking) => isActiveBookingForRide(booking, ride))}
                  notify={notify}
                  onReserved={load}
                  onOpenProfile={onOpenProfile}
                  onOpenBooking={onOpenBooking}
                />
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}

function isActiveBookingForRide(booking: Booking, ride: Ride) {
  if (booking.rideId !== ride.id) return false;
  if (["Cancelled", "Rejected", "Completed"].includes(booking.bookingStatus)) return false;

  const bookingDate = booking.rideDate || booking.departureDateTime;
  if (!bookingDate || !ride.departureDateTime) return true;
  return new Date(bookingDate).toDateString() === new Date(ride.departureDateTime).toDateString();
}

function RideCard({
  ride,
  profile,
  existingBooking,
  notify,
  onReserved,
  onOpenProfile,
  onOpenBooking
}: {
  ride: Ride;
  profile: UserProfile | null;
  existingBooking?: Booking;
  notify: Notify;
  onReserved: () => Promise<void>;
  onOpenProfile: (userId: string) => void;
  onOpenBooking: (bookingId: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [noteExistingOpen, setNoteExistingOpen] = useState(false);
  const isOwnRide = profile?.id === ride.driverId;
  const canCancelExistingBooking = existingBooking && !["InProgress", "Completed"].includes(ride.rideStatus);

  const reserve = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const raw = new FormData(event.currentTarget);
    const seats = Number(asText(raw.get("seatsReserved")) || "1");
    const paymentMethod = asText(raw.get("paymentMethod")) || "Cash";

    setSubmitting(true);
    try {
      await apiFetch<Booking>("/bookings", {
        method: "POST",
        body: {
          rideId: ride.id,
          rideDate: ride.departureDateTime,
          seatsReserved: seats,
          pickupPoint: asText(raw.get("pickupPoint")) || ride.startAddress,
          dropoffPoint: asText(raw.get("dropoffPoint")) || ride.destinationAddress,
          note: asText(raw.get("note")),
          paymentMethod
        }
      });
      notify("success", "Rezervacija je poslata.");
      setOpen(false);
      await onReserved();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Rezervacija nije uspela.");
    } finally {
      setSubmitting(false);
    }
  };

  const updateExistingNote = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!existingBooking) return;

    const raw = new FormData(event.currentTarget);
    setSubmitting(true);
    try {
      await apiFetch<Booking>(`/bookings/${existingBooking.id}`, {
        method: "PUT",
        body: { note: asText(raw.get("note")) }
      });
      notify("success", "Napomena je sacuvana.");
      setNoteExistingOpen(false);
      await onReserved();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Cuvanje napomene nije uspelo.");
    } finally {
      setSubmitting(false);
    }
  };

  const cancelExisting = async () => {
    if (!existingBooking || !window.confirm("Otkazati rezervaciju?")) return;
    setSubmitting(true);
    try {
      await apiFetch<Booking>(`/bookings/${existingBooking.id}/cancel`, { method: "POST" });
      notify("success", "Rezervacija je otkazana.");
      await onReserved();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Otkazivanje rezervacije nije uspelo.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <article className="page-card p-3">
      <div className="d-flex justify-content-between gap-2 mb-2">
        <div>
          <div className="fw-bold">{ride.startAddress}</div>
          <div className="small-muted">do {ride.destinationAddress}</div>
        </div>
        <span className={`status-chip ${statusClass(ride.rideStatus)}`}>{statusLabel(ride.rideStatus)}</span>
      </div>
      <div className="row g-2 mb-3">
        <div className="col-6">
          <div className="stat">
            <div className="stat-label">Polazak</div>
            <div className="fw-semibold">{formatDateTime(ride.departureDateTime)}</div>
          </div>
        </div>
        <div className="col-6">
          <div className="stat">
            <div className="stat-label">Cena</div>
            <div className="fw-semibold">{currency(ride.pricePerSeat)}</div>
          </div>
        </div>
        <div className="col-6">
          <div className="stat">
            <div className="stat-label">Sedista</div>
            <div className="fw-semibold">{ride.availableSeats}</div>
          </div>
        </div>
        <div className="col-6">
          <div className="stat">
            <div className="stat-label">Tip</div>
            <div className="fw-semibold">{statusLabel(ride.rideType)}</div>
          </div>
        </div>
      </div>
      <div className="small-muted mb-2">
        Vozac: <ProfileLink userId={ride.driverId} email={ride.driverEmail} isVerified={ride.driverIsVerified} onOpen={onOpenProfile} />
      </div>
      <div className="d-flex flex-wrap gap-2 mb-3">
        {ride.autoApproveBookings && <span className="badge text-bg-success">Automatska potvrda</span>}
        {ride.smokingAllowed && <span className="badge text-bg-secondary">Pusenje</span>}
        {ride.petsAllowed && <span className="badge text-bg-secondary">Ljubimci</span>}
        {ride.luggageAllowed && <span className="badge text-bg-secondary">Prtljag</span>}
        {ride.musicAllowed && <span className="badge text-bg-secondary">Muzika</span>}
        {ride.conversationAllowed && <span className="badge text-bg-secondary">Razgovor</span>}
      </div>

      {existingBooking && !isOwnRide && (
        <div className="alert alert-info py-2">
          <div className="fw-semibold">Vec imate rezervaciju za ovu voznju.</div>
          <div className="small">Status: {statusLabel(existingBooking.bookingStatus)} - sedista: {existingBooking.seatsReserved}</div>
          {existingBooking.note && (
            <div className="small mt-1">
              <span className="fw-semibold">Napomena:</span> {existingBooking.note}
            </div>
          )}
          <div className="d-flex flex-wrap gap-2 mt-2">
            <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => onOpenBooking(existingBooking.id)}>
              Otvori rezervaciju
            </button>
            <button className="btn btn-sm btn-outline-secondary" type="button" onClick={() => setNoteExistingOpen((current) => !current)}>
              {existingBooking.note ? "Izmeni napomenu" : "Dodaj napomenu"}
            </button>
            {canCancelExistingBooking && (
              <button className="btn btn-sm btn-outline-danger" type="button" onClick={() => void cancelExisting()} disabled={submitting}>
                Otkazi
              </button>
            )}
          </div>
        </div>
      )}

      {noteExistingOpen && existingBooking && (
        <form className="border-top mt-3 pt-3" onSubmit={updateExistingNote}>
          <label className="form-label" htmlFor={`existing-note-${ride.id}`}>
            Napomena za vozaca
          </label>
          <textarea className="form-control" id={`existing-note-${ride.id}`} name="note" rows={3} defaultValue={existingBooking.note || ""} />
          <button className="btn btn-secondary btn-sm mt-2" type="submit" disabled={submitting}>
            Sacuvaj napomenu
          </button>
        </form>
      )}

      {!isOwnRide && !existingBooking && (
        <button className="btn btn-outline-primary btn-sm" type="button" onClick={() => setOpen((current) => !current)}>
          {open ? "Zatvori" : "Rezervisi"}
        </button>
      )}

      {open && !isOwnRide && !existingBooking && (
        <form className="border-top mt-3 pt-3" onSubmit={reserve}>
          <div className="row g-2">
            <div className="col-md-3">
              <label className="form-label" htmlFor={`reserve-seats-${ride.id}`}>
                Sedista
              </label>
              <input className="form-control" id={`reserve-seats-${ride.id}`} name="seatsReserved" type="number" min="1" max={ride.availableSeats} defaultValue="1" required />
            </div>
            <div className="col-md-9">
              <label className="form-label" htmlFor={`reserve-pickup-${ride.id}`}>
                Ulazak
              </label>
              <input className="form-control" id={`reserve-pickup-${ride.id}`} name="pickupPoint" defaultValue={ride.startAddress} />
            </div>
            <div className="col-md-6">
              <label className="form-label" htmlFor={`reserve-dropoff-${ride.id}`}>
                Izlazak
              </label>
              <input className="form-control" id={`reserve-dropoff-${ride.id}`} name="dropoffPoint" defaultValue={ride.destinationAddress} />
            </div>
            <div className="col-md-3">
              <label className="form-label" htmlFor={`reserve-method-${ride.id}`}>
                Placanje
              </label>
              <select className="form-select" id={`reserve-method-${ride.id}`} name="paymentMethod" defaultValue="Cash">
                <option value="Cash">Kes</option>
                <option value="Online">Online</option>
              </select>
            </div>
            <div className="col-12">
              <label className="form-label" htmlFor={`reserve-note-${ride.id}`}>
                Napomena
              </label>
              <textarea className="form-control" id={`reserve-note-${ride.id}`} name="note" rows={2} placeholder="Npr. cekam kod pumpe, imam mali kofer..." />
            </div>
          </div>
          <button className="btn btn-primary btn-sm mt-3" type="submit" disabled={submitting}>
            {submitting ? "Slanje..." : "Posalji zahtev za rezervaciju"}
          </button>
        </form>
      )}
    </article>
  );
}

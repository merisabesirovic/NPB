import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import { apiFetch, currency, formatDateTime, shortId } from "../api";
import { ProfileLink, profileName } from "../components/ProfileLink";
import type { Booking, Dispute, FlashType, Review, UserProfile } from "../types";
import { asText } from "../utils/forms";
import { statusClass, statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;
type BookingRoleView = "passenger" | "driver";

export function BookingsPage({
  notify,
  profile,
  isDriver,
  onOpenProfile,
  selectedBookingId,
  onSelectedBookingHandled
}: {
  notify: Notify;
  profile: UserProfile | null;
  isDriver: boolean;
  onOpenProfile: (userId: string) => void;
  selectedBookingId: string | null;
  onSelectedBookingHandled: () => void;
}) {
  const [myBookings, setMyBookings] = useState<Booking[]>([]);
  const [driverBookings, setDriverBookings] = useState<Booking[]>([]);
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(false);
  const [activeRole, setActiveRole] = useState<BookingRoleView>("passenger");

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [passengerData, disputeData, reviewData] = await Promise.all([apiFetch<Booking[]>("/bookings/me"), apiFetch<Dispute[]>("/disputes/me"), apiFetch<Review[]>("/reviews/me/written")]);
      setMyBookings(passengerData || []);
      setDisputes(disputeData || []);
      setReviews(reviewData || []);
      if (isDriver) {
        setDriverBookings(await apiFetch<Booking[]>("/bookings/driver/requests"));
      } else {
        setDriverBookings([]);
      }
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Rezervacije nisu ucitane.");
    } finally {
      setLoading(false);
    }
  }, [isDriver, notify]);

  useEffect(() => {
    void load();
  }, [load]);

  const sortedPassengerBookings = useMemo(() => {
    return [...myBookings].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }, [myBookings]);

  const sortedDriverBookings = useMemo(() => {
    return [...driverBookings].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }, [driverBookings]);

  const visibleBookings = activeRole === "driver" && isDriver ? sortedDriverBookings : sortedPassengerBookings;
  const emptyBookingsMessage = activeRole === "driver" && isDriver ? "Nemate rezervacije za svoje voznje." : "Nemate rezervacije kao putnik.";

  useEffect(() => {
    if (!isDriver && activeRole !== "passenger") {
      setActiveRole("passenger");
    }
  }, [activeRole, isDriver]);

  useEffect(() => {
    if (!selectedBookingId || !isDriver) return;
    if (driverBookings.some((booking) => booking.id === selectedBookingId)) {
      setActiveRole("driver");
      return;
    }
    if (myBookings.some((booking) => booking.id === selectedBookingId)) {
      setActiveRole("passenger");
    }
  }, [driverBookings, isDriver, myBookings, selectedBookingId]);

  useEffect(() => {
    if (!selectedBookingId) return;
    const element = document.getElementById(`booking-${selectedBookingId}`);
    if (!element) return;
    element.scrollIntoView({ behavior: "smooth", block: "center" });
    element.classList.add("highlight-card");
    const timeout = window.setTimeout(() => {
      element.classList.remove("highlight-card");
      onSelectedBookingHandled();
    }, 1600);
    return () => window.clearTimeout(timeout);
  }, [activeRole, onSelectedBookingHandled, selectedBookingId, visibleBookings.length]);

  const cancel = async (booking: Booking) => {
    if (!window.confirm("Otkazati rezervaciju?")) return;
    try {
      await apiFetch<Booking>(`/bookings/${booking.id}/cancel`, { method: "POST" });
      notify("success", "Rezervacija je otkazana.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Otkazivanje rezervacije nije uspelo.");
    }
  };

  const updateBookingNote = async (booking: Booking, note: string) => {
    try {
      await apiFetch<Booking>(`/bookings/${booking.id}`, {
        method: "PUT",
        body: { note }
      });
      notify("success", "Napomena je sacuvana.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Cuvanje napomene nije uspelo.");
    }
  };

  const payOnline = async (booking: Booking, form: { cardholderName: string; cardNumber: string; expiry: string; cvv: string }) => {
    try {
      await apiFetch<Booking>(`/bookings/${booking.id}/pay-online`, {
        method: "POST",
        body: form
      });
      notify("success", "Placanje je uspesno evidentirano.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Placanje nije uspelo.");
    }
  };

  const createDispute = async (booking: Booking, description: string) => {
    try {
      await apiFetch<Dispute>("/disputes", {
        method: "POST",
        body: {
          bookingId: booking.id,
          description
        }
      });
      notify("success", "Spor je otvoren.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Otvaranje spora nije uspelo.");
    }
  };

  const createReview = async (booking: Booking, rating: number, comment: string) => {
    if (!profile) return;
    const reviewedUserId = profile.id === booking.passengerId ? booking.driverId : booking.passengerId;
    if (!reviewedUserId) {
      notify("warning", "Ne mogu da odredim korisnika za ocenu.");
      return;
    }

    try {
      await apiFetch<Review>("/reviews", {
        method: "POST",
        body: {
          bookingId: booking.id,
          reviewedUserId,
          rating,
          comment
        }
      });
      notify("success", "Ocena je poslata.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Slanje ocene nije uspelo.");
    }
  };

  return (
    <section className="page-card p-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="h5 mb-0">Rezervacije</h2>
        <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void load()} disabled={loading}>
          Osvezi
        </button>
      </div>
      {isDriver && (
        <div className="btn-group mb-3" role="group" aria-label="Prikaz rezervacija po ulozi">
          <button
            className={`btn btn-sm ${activeRole === "passenger" ? "btn-primary" : "btn-outline-primary"}`}
            type="button"
            onClick={() => setActiveRole("passenger")}
          >
            Kao putnik ({myBookings.length})
          </button>
          <button className={`btn btn-sm ${activeRole === "driver" ? "btn-primary" : "btn-outline-primary"}`} type="button" onClick={() => setActiveRole("driver")}>
            Kao vozac ({driverBookings.length})
          </button>
        </div>
      )}
      {visibleBookings.length === 0 ? (
        <div className="small-muted">{emptyBookingsMessage}</div>
      ) : (
        <div className="booking-grid">
          {visibleBookings.map((booking) => (
            <BookingCard
              key={booking.id}
              booking={booking}
              currentUserId={profile?.id}
              hasDispute={disputes.some((dispute) => dispute.bookingId === booking.id)}
              hasReview={reviews.some((review) => review.bookingId === booking.id && review.reviewerId === profile?.id)}
              onOpenProfile={onOpenProfile}
              onCancel={cancel}
              onUpdateNote={updateBookingNote}
              onPayOnline={payOnline}
              onDispute={createDispute}
              onReview={createReview}
            />
          ))}
        </div>
      )}
    </section>
  );
}

function BookingCard({
  booking,
  currentUserId,
  onOpenProfile,
  onCancel,
  onUpdateNote,
  onPayOnline,
  onDispute,
  onReview,
  hasDispute,
  hasReview
}: {
  booking: Booking;
  currentUserId?: string;
  onOpenProfile: (userId: string) => void;
  onCancel: (booking: Booking) => void;
  onUpdateNote: (booking: Booking, note: string) => Promise<void>;
  onPayOnline: (booking: Booking, form: { cardholderName: string; cardNumber: string; expiry: string; cvv: string }) => Promise<void>;
  onDispute: (booking: Booking, description: string) => Promise<void>;
  onReview: (booking: Booking, rating: number, comment: string) => Promise<void>;
  hasDispute: boolean;
  hasReview: boolean;
}) {
  const [disputeOpen, setDisputeOpen] = useState(false);
  const [reviewOpen, setReviewOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [paymentOpen, setPaymentOpen] = useState(false);
  const otherUserIsPassenger = currentUserId !== booking.passengerId;
  const otherUserId = otherUserIsPassenger ? booking.passengerId : booking.driverId;
  const otherUserLabel = otherUserIsPassenger
    ? profileName(booking.passengerFirstName, booking.passengerLastName, booking.passengerEmail, "Putnik")
    : profileName(booking.driverFirstName, booking.driverLastName, booking.driverEmail, "Vozac");

  const submitDispute = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const description = asText(new FormData(form).get("description"));
    await onDispute(booking, description);
    form.reset();
    setDisputeOpen(false);
  };

  const submitReview = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const raw = new FormData(form);
    await onReview(booking, Number(asText(raw.get("rating")) || "5"), asText(raw.get("comment")));
    form.reset();
    setReviewOpen(false);
  };

  const submitNote = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const raw = new FormData(event.currentTarget);
    await onUpdateNote(booking, asText(raw.get("note")));
    setEditOpen(false);
  };

  const submitPayment = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const raw = new FormData(form);
    await onPayOnline(booking, {
      cardholderName: asText(raw.get("cardholderName")),
      cardNumber: asText(raw.get("cardNumber")),
      expiry: asText(raw.get("expiry")),
      cvv: asText(raw.get("cvv"))
    });
    form.reset();
    setPaymentOpen(false);
  };

  const isPassengerBooking = currentUserId === booking.passengerId;
  const canAddNote = isPassengerBooking && !["Cancelled", "Rejected", "Completed"].includes(booking.bookingStatus);
  const canPayOnline = isPassengerBooking && booking.bookingStatus === "Approved" && booking.paymentMethod === "Online" && booking.paymentStatus !== "Paid";
  const canCancel = isPassengerBooking && canPassengerCancelBooking(booking);
  const canReview = !hasReview && booking.bookingStatus === "Completed";
  const paymentLabel = booking.paymentMethod ? `${statusLabel(booking.paymentMethod)} / ${statusLabel(booking.paymentStatus)}` : "-";

  return (
    <article className="page-card p-3" id={`booking-${booking.id}`}>
      <div className="d-flex justify-content-between align-items-start gap-2">
        <div>
          <div className="fw-bold">
            {booking.startAddress || "Voznja"} {"->"} {booking.destinationAddress || ""}
          </div>
          <div className="small-muted">{formatDateTime(booking.departureDateTime || booking.rideDate)}</div>
          <div className="small-muted">
            Druga strana:{" "}
            <ProfileLink
              userId={otherUserId}
              firstName={otherUserIsPassenger ? booking.passengerFirstName : booking.driverFirstName}
              lastName={otherUserIsPassenger ? booking.passengerLastName : booking.driverLastName}
              email={otherUserIsPassenger ? booking.passengerEmail : booking.driverEmail}
              isVerified={otherUserIsPassenger ? booking.passengerIsVerified : booking.driverIsVerified}
              onOpen={onOpenProfile}
            />
          </div>
          <div className="small-muted">Booking {shortId(booking.id)}</div>
        </div>
        <span className={`status-chip ${statusClass(booking.bookingStatus)}`}>{statusLabel(booking.bookingStatus)}</span>
      </div>

      <div className="row g-2 my-3">
        <div className="col-4">
          <div className="stat">
            <div className="stat-label">Sedista</div>
            <div className="fw-semibold">{booking.seatsReserved}</div>
          </div>
        </div>
        <div className="col-4">
          <div className="stat">
            <div className="stat-label">Cena</div>
            <div className="fw-semibold">{currency(booking.totalPrice)}</div>
          </div>
        </div>
        <div className="col-4">
          <div className="stat">
            <div className="stat-label">Placanje</div>
            <div className="fw-semibold">{paymentLabel}</div>
          </div>
        </div>
      </div>

      {booking.note && (
        <div className="alert alert-light border py-2 mb-3">
          <div className="stat-label">Napomena</div>
          <div className="pre-wrap">{booking.note}</div>
        </div>
      )}

      {booking.refundAmount !== undefined && booking.refundAmount !== null && (
        <div className="alert alert-info py-2 mb-3">
          <div className="stat-label">Refundacija</div>
          <div className="fw-semibold">{currency(booking.refundAmount)}</div>
        </div>
      )}

      <div className="d-flex flex-wrap gap-2">
        {canCancel && (
          <button className="btn btn-sm btn-outline-danger" type="button" onClick={() => onCancel(booking)}>
            Otkazi
          </button>
        )}
        {canAddNote && (
          <button className="btn btn-sm btn-outline-secondary" type="button" onClick={() => setEditOpen((current) => !current)}>
            {booking.note ? "Izmeni napomenu" : "Dodaj napomenu"}
          </button>
        )}
        {canPayOnline && (
          <button className="btn btn-sm btn-success" type="button" onClick={() => setPaymentOpen((current) => !current)}>
            Plati online
          </button>
        )}
        {!hasDispute && (
          <button className="btn btn-sm btn-outline-warning" type="button" onClick={() => setDisputeOpen((current) => !current)}>
            Spor
          </button>
        )}
        {canReview && (
          <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => setReviewOpen((current) => !current)}>
            Recenzija za {otherUserLabel}
          </button>
        )}
      </div>

      {editOpen && (
        <form className="border-top mt-3 pt-3" onSubmit={submitNote}>
          <label className="form-label" htmlFor={`booking-note-${booking.id}`}>
            Napomena za vozaca
          </label>
          <textarea className="form-control" id={`booking-note-${booking.id}`} name="note" rows={3} defaultValue={booking.note || ""} />
          <button className="btn btn-secondary btn-sm mt-2" type="submit">
            Sacuvaj napomenu
          </button>
        </form>
      )}

      {paymentOpen && (
        <form className="border-top mt-3 pt-3" onSubmit={submitPayment}>
          <div className="alert alert-info py-2">Bezbedno placanje.</div>
          <div className="row g-2">
            <div className="col-md-6">
              <label className="form-label" htmlFor={`pay-name-${booking.id}`}>
                Ime na kartici
              </label>
              <input className="form-control" id={`pay-name-${booking.id}`} name="cardholderName" required />
            </div>
            <div className="col-md-6">
              <label className="form-label" htmlFor={`pay-card-${booking.id}`}>
                Broj kartice
              </label>
              <input className="form-control" id={`pay-card-${booking.id}`} name="cardNumber" inputMode="numeric" placeholder="4242 4242 4242 4242" required />
            </div>
            <div className="col-md-4">
              <label className="form-label" htmlFor={`pay-expiry-${booking.id}`}>
                Istice
              </label>
              <input className="form-control" id={`pay-expiry-${booking.id}`} name="expiry" placeholder="12/30" required />
            </div>
            <div className="col-md-4">
              <label className="form-label" htmlFor={`pay-cvv-${booking.id}`}>
                CVV
              </label>
              <input className="form-control" id={`pay-cvv-${booking.id}`} name="cvv" inputMode="numeric" required />
            </div>
            <div className="col-md-4 d-flex align-items-end">
              <div className="fw-bold">{currency(booking.totalPrice)}</div>
            </div>
          </div>
          <button className="btn btn-success btn-sm mt-2" type="submit">
            Plati
          </button>
        </form>
      )}

      {disputeOpen && (
        <form className="border-top mt-3 pt-3" onSubmit={submitDispute}>
          <label className="form-label" htmlFor={`dispute-${booking.id}`}>
            Opis spora
          </label>
          <textarea className="form-control" id={`dispute-${booking.id}`} name="description" rows={3} required />
          <button className="btn btn-warning btn-sm mt-2" type="submit">
            Otvori spor
          </button>
        </form>
      )}

      {reviewOpen && (
        <form className="border-top mt-3 pt-3" onSubmit={submitReview}>
          <div className="row g-2">
            <div className="col-md-3">
              <label className="form-label" htmlFor={`review-rating-${booking.id}`}>
                Ocena
              </label>
              <select className="form-select" id={`review-rating-${booking.id}`} name="rating" defaultValue="5">
                {[5, 4, 3, 2, 1].map((rating) => (
                  <option key={rating} value={rating}>
                    {rating}
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-9">
              <label className="form-label" htmlFor={`review-comment-${booking.id}`}>
                Komentar
              </label>
              <input className="form-control" id={`review-comment-${booking.id}`} name="comment" />
            </div>
          </div>
          <button className="btn btn-primary btn-sm mt-2" type="submit">
            Posalji recenziju
          </button>
        </form>
      )}
    </article>
  );
}

function canPassengerCancelBooking(booking: Booking) {
  if (["Cancelled", "Rejected", "Completed"].includes(booking.bookingStatus)) return false;
  return !["InProgress", "Completed"].includes(booking.rideStatus || "");
}

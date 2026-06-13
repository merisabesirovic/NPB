import { useCallback, useEffect, useState } from "react";
import { apiFetch, formatDateTime, shortId } from "../api";
import { ProfileLink } from "../components/ProfileLink";
import type { Dispute, FlashType, Review } from "../types";
import { statusClass, statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;

export function DisputesReviewsPage({ notify, onOpenProfile }: { notify: Notify; onOpenProfile: (userId: string) => void }) {
  const [active, setActive] = useState<"disputes" | "reviews">("disputes");
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [disputeData, reviewData] = await Promise.all([apiFetch<Dispute[]>("/disputes/me"), apiFetch<Review[]>("/reviews/me")]);
      setDisputes(disputeData || []);
      setReviews(reviewData || []);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Podaci nisu ucitani.");
    } finally {
      setLoading(false);
    }
  }, [notify]);

  useEffect(() => {
    void load();
  }, [load]);

  return (
    <section className="page-card p-4">
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
        <h2 className="h5 mb-0">Ocene i sporovi</h2>
        <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void load()} disabled={loading}>
          Osvezi
        </button>
      </div>
      <div className="btn-group mb-3">
        <button className={`btn btn-sm ${active === "disputes" ? "btn-primary" : "btn-outline-primary"}`} type="button" onClick={() => setActive("disputes")}>
          Sporovi
        </button>
        <button className={`btn btn-sm ${active === "reviews" ? "btn-primary" : "btn-outline-primary"}`} type="button" onClick={() => setActive("reviews")}>
          Moje ocene
        </button>
      </div>

      {active === "disputes" ? (
        disputes.length === 0 ? (
          <div className="small-muted">Nemate sporove.</div>
        ) : (
          <div className="list-group">
            {disputes.map((dispute) => (
              <div className="list-group-item" key={dispute.id}>
                <div className="d-flex justify-content-between gap-2">
                  <div>
                    <div className="fw-semibold">
                      {dispute.startAddress || "Booking"} {"->"} {dispute.destinationAddress || shortId(dispute.bookingId)}
                    </div>
                    <div className="small-muted">
                      Otvorio/la{" "}
                      <ProfileLink
                        userId={dispute.createdByUserId}
                        firstName={dispute.createdByFirstName}
                        lastName={dispute.createdByLastName}
                        email={dispute.createdByEmail}
                        isVerified={dispute.createdByIsVerified}
                        onOpen={onOpenProfile}
                      />{" "}
                      - {formatDateTime(dispute.createdAt)}
                    </div>
                    <div className="pre-wrap">{dispute.description}</div>
                    {dispute.resolution && <div className="small-muted mt-1">Resenje: {dispute.resolution}</div>}
                  </div>
                  <span className={`status-chip ${statusClass(dispute.status)}`}>{statusLabel(dispute.status)}</span>
                </div>
              </div>
            ))}
          </div>
        )
      ) : reviews.length === 0 ? (
        <div className="small-muted">Nemate ocene.</div>
      ) : (
        <div className="list-group">
          {reviews.map((review) => (
            <div className="list-group-item" key={review.id}>
              <div className="d-flex justify-content-between gap-2">
                <div>
                  <div className="fw-semibold">Ocena {review.rating}/5</div>
                  <div className="small-muted">
                    Od{" "}
                    <ProfileLink
                      userId={review.reviewerId}
                      firstName={review.reviewerFirstName}
                      lastName={review.reviewerLastName}
                      email={review.reviewerEmail}
                      isVerified={review.reviewerIsVerified}
                      onOpen={onOpenProfile}
                    />{" "}
                    - {formatDateTime(review.createdAt)}
                  </div>
                  <div>{review.comment || "-"}</div>
                </div>
                <span className="small-muted">Booking {shortId(review.bookingId)}</span>
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}

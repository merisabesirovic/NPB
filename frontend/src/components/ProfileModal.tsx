import { useEffect, useState } from "react";
import { apiFetch, formatDateTime, rolesToText } from "../api";
import type { Review, UserProfile } from "../types";
import { ProfileLink, VerificationBadge } from "./ProfileLink";

export function ProfileModal({
  userId,
  onClose,
  onOpenProfile
}: {
  userId: string | null;
  onClose: () => void;
  onOpenProfile: (userId: string) => void;
}) {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [active, setActive] = useState<"profile" | "reviews">("profile");
  const [error, setError] = useState("");

  useEffect(() => {
    if (!userId) return;
    let cancelled = false;
    setProfile(null);
    setReviews([]);
    setActive("profile");
    setError("");

    async function load() {
      try {
        const [profileData, reviewData] = await Promise.all([apiFetch<UserProfile>(`/users/${userId}`), apiFetch<Review[]>(`/reviews/user/${userId}`).catch(() => [])]);
        if (cancelled) return;
        setProfile(profileData);
        setReviews(reviewData || []);
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : "Profil nije ucitan.");
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [userId]);

  if (!userId) return null;

  return (
    <div className="modal-backdrop-lite" role="dialog" aria-modal="true">
      <div className="profile-modal page-card p-4">
        <div className="d-flex justify-content-between align-items-start mb-3">
          <h2 className="h5 mb-0">Profil korisnika</h2>
          <button className="btn-close" type="button" aria-label="Close" onClick={onClose} />
        </div>
        {error && <div className="alert alert-danger">{error}</div>}
        {!profile && !error && <div className="small-muted">Ucitavanje...</div>}
        {profile && (
          <>
            <div className="d-flex align-items-center gap-3 mb-3">
              {profile.avatarUrl ? <img src={profile.avatarUrl} alt="Avatar" width="64" height="64" className="rounded" /> : <div className="brand-mark">RM</div>}
              <div>
                <div className="fw-bold">
                  {profile.firstName} {profile.lastName}
                  <VerificationBadge isVerified={profile.isVerified} />
                </div>
                <div className="small-muted">{profile.email}</div>
                <div className="small-muted">{rolesToText(profile.roles)}</div>
              </div>
            </div>

            <div className="btn-group mb-3">
              <button className={`btn btn-sm ${active === "profile" ? "btn-primary" : "btn-outline-primary"}`} type="button" onClick={() => setActive("profile")}>
                Profil
              </button>
              <button className={`btn btn-sm ${active === "reviews" ? "btn-primary" : "btn-outline-primary"}`} type="button" onClick={() => setActive("reviews")}>
                Recenzije ({reviews.length})
              </button>
            </div>

            {active === "profile" ? (
              <>
                <div className="row g-2">
                  <div className="col-6">
                    <div className="stat">
                      <div className="stat-label">Ocena</div>
                      <div className="stat-value">{profile.averageRating?.toFixed?.(1) ?? "0.0"}</div>
                    </div>
                  </div>
                  <div className="col-6">
                    <div className="stat">
                      <div className="stat-label">Trust</div>
                      <div className="stat-value">{Math.round(profile.trustScore || 0)}</div>
                    </div>
                  </div>
                  <div className="col-6">
                    <div className="stat">
                      <div className="stat-label">Zavrsene</div>
                      <div className="stat-value">{profile.completedRidesCount || 0}</div>
                    </div>
                  </div>
                  <div className="col-6">
                    <div className="stat">
                      <div className="stat-label">Otkazane</div>
                      <div className="stat-value">{profile.cancelledRidesCount || 0}</div>
                    </div>
                  </div>
                </div>
                {profile.vehicles?.length ? (
                  <div className="mt-3">
                    <div className="small-muted mb-2">Vozilo</div>
                    <div className="list-group">
                      {profile.vehicles.map((vehicle) => (
                        <div className="list-group-item" key={vehicle.id}>
                          <div className="fw-semibold">{vehicle.model || "Vozilo"}</div>
                          <div className="small-muted">
                            {vehicle.year || "-"} - {vehicle.seatsCount || "-"} sedista - {vehicle.isVerified ? "verifikovano" : "nije verifikovano"}
                          </div>
                          {vehicle.vehicleImageUrl && <img className="img-fluid rounded mt-2" src={vehicle.vehicleImageUrl} alt={vehicle.model || "Vozilo"} />}
                        </div>
                      ))}
                    </div>
                  </div>
                ) : null}
              </>
            ) : reviews.length === 0 ? (
              <div className="small-muted">Nema recenzija za ovaj profil.</div>
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
                      <span className="status-chip success">{review.rating}/5</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}

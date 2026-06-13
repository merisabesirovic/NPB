import { FormEvent, useEffect, useState } from "react";
import { apiFetch, formatDateForInput, rolesToText } from "../api";
import { VerificationBadge } from "../components/ProfileLink";
import type { FlashType, UserProfile } from "../types";
import { ROLE } from "../types";
import { appendFileIf, appendIf, asText, passwordHint } from "../utils/forms";

type Notify = (type: FlashType, text: string) => void;

export function ProfilePage({
  profile,
  refreshProfile,
  notify,
  onLogout
}: {
  profile: UserProfile | null;
  refreshProfile: () => Promise<void>;
  notify: Notify;
  onLogout: () => void;
}) {
  const [updating, setUpdating] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [passwordChanging, setPasswordChanging] = useState(false);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [avatarError, setAvatarError] = useState(false);
  const isDriver = ((profile?.roles || 0) & ROLE.Driver) === ROLE.Driver;
  const avatarSrc = avatarPreview || profile?.avatarUrl || "";

  useEffect(() => {
    setAvatarError(false);
  }, [avatarSrc]);

  useEffect(() => {
    return () => {
      if (avatarPreview) URL.revokeObjectURL(avatarPreview);
    };
  }, [avatarPreview]);

  const previewAvatar = (file: File | null) => {
    if (!file || file.size === 0) {
      setAvatarPreview(null);
      return;
    }

    setAvatarPreview(URL.createObjectURL(file));
  };

  const updateProfile = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const raw = new FormData(event.currentTarget);
    const payload = new FormData();

    appendIf(payload, "FirstName", raw.get("firstName"));
    appendIf(payload, "LastName", raw.get("lastName"));
    appendIf(payload, "PhoneNumber", raw.get("phoneNumber"));
    appendIf(payload, "DateOfBirth", raw.get("dateOfBirth"));
    appendIf(payload, "Biography", raw.get("biography"));
    appendFileIf(payload, "Avatar", raw.get("avatar"));

    const vehicleModel = asText(raw.get("vehicleModel"));
    const vehicleYear = asText(raw.get("vehicleYear"));
    const vehicleSeats = asText(raw.get("vehicleSeats"));
    const vehicleLicense = asText(raw.get("vehicleLicense"));
    const vehicleId = asText(raw.get("vehicleId"));
    const vehicleImage = raw.get("vehicleImage");
    const hasVehicle = vehicleModel || vehicleYear || vehicleSeats || vehicleLicense || vehicleId || (vehicleImage instanceof File && vehicleImage.size > 0);

    if (hasVehicle) {
      if (vehicleId) payload.append("VehicleUpdates[0].Id", vehicleId);
      if (vehicleModel) payload.append("VehicleUpdates[0].Model", vehicleModel);
      if (vehicleYear) payload.append("VehicleUpdates[0].Year", vehicleYear);
      if (vehicleSeats) payload.append("VehicleUpdates[0].SeatsCount", vehicleSeats);
      if (vehicleLicense) payload.append("VehicleUpdates[0].LicenseNumber", vehicleLicense);
      appendFileIf(payload, "VehicleUpdates[0].VehicleImage", vehicleImage);
    }

    setUpdating(true);
    try {
      await apiFetch<{ message: string }>("/profile", { method: "PUT", body: payload });
      await refreshProfile();
      setAvatarPreview(null);
      notify("success", "Profil je sacuvan.");
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Cuvanje profila nije uspelo.");
    } finally {
      setUpdating(false);
    }
  };

  const uploadIdentity = async (event: FormEvent<HTMLFormElement>, endpoint: string, successText: string) => {
    event.preventDefault();
    const form = event.currentTarget;
    const raw = new FormData(form);
    const payload = new FormData();
    appendFileIf(payload, "File", raw.get("identityFile"));

    setUploading(true);
    try {
      await apiFetch<{ docId: string; url: string; status?: string }>(endpoint, { method: "POST", body: payload });
      notify("success", successText);
      await refreshProfile();
      form.reset();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Upload dokumenta nije uspeo.");
    } finally {
      setUploading(false);
    }
  };

  const changePassword = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const raw = new FormData(event.currentTarget);
    setPasswordChanging(true);
    try {
      await apiFetch<{ requireRelogin: boolean }>("/users/me/change-password", {
        method: "POST",
        body: {
          currentPassword: asText(raw.get("currentPassword")),
          newPassword: asText(raw.get("newPassword"))
        }
      });
      notify("success", "Lozinka je promenjena. Potrebna je ponovna prijava.");
      onLogout();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Promena lozinke nije uspela.");
    } finally {
      setPasswordChanging(false);
    }
  };

  return (
    <div className="row g-3">
      <div className="col-xl-4">
        <section className="page-card p-4 h-100">
          <h2 className="h5 mb-3">Profil</h2>
          {profile ? (
            <>
              <div className="d-flex align-items-center gap-3 mb-3">
                {avatarSrc && !avatarError ? (
                  <img src={avatarSrc} alt="Avatar" width="64" height="64" className="profile-avatar" onError={() => setAvatarError(true)} />
                ) : (
                  <div className="brand-mark">RM</div>
                )}
                <div>
                  <div className="fw-bold">
                    {profile.firstName} {profile.lastName}
                    <VerificationBadge isVerified={profile.isVerified} />
                  </div>
                  <div className="small-muted">{profile.email}</div>
                  <div className="small-muted">{rolesToText(profile.roles)}</div>
                </div>
              </div>
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
              </div>
              {isDriver && (
                <>
                  <hr />
                  <div className="small-muted mb-2">Vozila</div>
                  {profile.vehicles?.length ? (
                    <ul className="list-group">
                      {profile.vehicles.map((vehicle) => (
                        <li className="list-group-item" key={vehicle.id}>
                          <div className="fw-semibold">{vehicle.model || "Vozilo"}</div>
                          <div className="small-muted">
                            {vehicle.year || "-"} - {vehicle.seatsCount || "-"} sedista - {vehicle.isVerified ? "verifikovano" : "nije verifikovano"}
                          </div>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <div className="small-muted">Nema vozila.</div>
                  )}
                </>
              )}
            </>
          ) : (
            <div className="small-muted">Profil se ucitava.</div>
          )}
        </section>
      </div>

      <div className="col-xl-8">
        <section className="page-card p-4 mb-3">
          <h2 className="h5 mb-3">Izmena profila</h2>
          <form onSubmit={updateProfile}>
            <div className="row g-3">
              <div className="col-md-6">
                <label className="form-label" htmlFor="profile-first-name">
                  Ime
                </label>
                <input className="form-control" id="profile-first-name" name="firstName" defaultValue={profile?.firstName || ""} />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="profile-last-name">
                  Prezime
                </label>
                <input className="form-control" id="profile-last-name" name="lastName" defaultValue={profile?.lastName || ""} />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="profile-phone">
                  Telefon
                </label>
                <input className="form-control" id="profile-phone" name="phoneNumber" defaultValue={profile?.phoneNumber || ""} />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="profile-dob">
                  Datum rodjenja
                </label>
                <input className="form-control" id="profile-dob" name="dateOfBirth" type="date" defaultValue={formatDateForInput(profile?.dateOfBirth)} />
              </div>
              <div className="col-12">
                <label className="form-label" htmlFor="profile-bio">
                  Biografija
                </label>
                <textarea className="form-control" id="profile-bio" name="biography" rows={2} defaultValue={profile?.biography || ""} />
              </div>
              <div className="col-md-6">
                <label className="form-label" htmlFor="profile-avatar">
                  Novi avatar
                </label>
                <input
                  className="form-control"
                  id="profile-avatar"
                  name="avatar"
                  type="file"
                  accept="image/*"
                  onChange={(event) => previewAvatar(event.currentTarget.files?.[0] ?? null)}
                />
              </div>
            </div>

            {isDriver && (
              <div className="border-top mt-3 pt-3">
                <div className="section-title">Vozilo</div>
                <div className="row g-3">
                <div className="col-md-4">
                  <label className="form-label" htmlFor="vehicle-id">
                    Postojece vozilo
                  </label>
                  <select className="form-select" id="vehicle-id" name="vehicleId">
                    <option value="">Novo vozilo</option>
                    {profile?.vehicles?.map((vehicle) => (
                      <option key={vehicle.id} value={vehicle.id}>
                        {vehicle.model || vehicle.id.slice(0, 8)}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-md-4">
                  <label className="form-label" htmlFor="vehicle-model">
                    Model
                  </label>
                  <input className="form-control" id="vehicle-model" name="vehicleModel" />
                </div>
                <div className="col-md-2">
                  <label className="form-label" htmlFor="vehicle-year">
                    Godina
                  </label>
                  <input className="form-control" id="vehicle-year" name="vehicleYear" type="number" min="1950" max="2100" />
                </div>
                <div className="col-md-2">
                  <label className="form-label" htmlFor="vehicle-seats">
                    Sedista
                  </label>
                  <input className="form-control" id="vehicle-seats" name="vehicleSeats" type="number" min="1" />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="vehicle-license">
                    Broj dozvole
                  </label>
                  <input className="form-control" id="vehicle-license" name="vehicleLicense" />
                </div>
                <div className="col-md-6">
                  <label className="form-label" htmlFor="vehicle-image">
                    Slika vozila
                  </label>
                  <input className="form-control" id="vehicle-image" name="vehicleImage" type="file" accept="image/*" />
                </div>
                </div>
              </div>
            )}

            <button className="btn btn-primary mt-3" type="submit" disabled={updating}>
              {updating ? "Cuvanje..." : "Sacuvaj profil"}
            </button>
          </form>
        </section>

        <div className="row g-3">
          {profile && !profile.isVerified && (
            <div className="col-lg-6">
              <section className="page-card p-4 h-100">
                <h2 className="h6 mb-3">Verifikacija identiteta</h2>
                {profile.identityVerificationPending ? (
                  <div className="alert alert-warning mb-0">ID dokument je poslat i ceka odobrenje admina.</div>
                ) : (
                  <form onSubmit={(event) => void uploadIdentity(event, "/verification/upload-passenger-identity", "ID dokument je poslat adminu na odobrenje.")}>
                    <input className="form-control mb-3" name="identityFile" type="file" required />
                    <button className="btn btn-outline-success" type="submit" disabled={uploading}>
                      {uploading ? "Slanje..." : "Posalji ID dokument"}
                    </button>
                  </form>
                )}
              </section>
            </div>
          )}
          {isDriver && (
            <div className="col-lg-6">
            <section className="page-card p-4 h-100">
              <h2 className="h6 mb-3">Upload ID dokumenta</h2>
              <form onSubmit={(event) => void uploadIdentity(event, "/verification/upload-identity", "Dokument je poslat na verifikaciju.")}>
                <input className="form-control mb-3" name="identityFile" type="file" required />
                <button className="btn btn-outline-primary" type="submit" disabled={uploading}>
                  {uploading ? "Slanje..." : "Posalji dokument"}
                </button>
              </form>
            </section>
          </div>
          )}
          <div className={isDriver || (profile && !profile.isVerified) ? "col-lg-6" : "col-lg-12"}>
            <section className="page-card p-4 h-100">
              <h2 className="h6 mb-3">Promena lozinke</h2>
              <form onSubmit={changePassword}>
                <input className="form-control mb-2" name="currentPassword" type="password" placeholder="Trenutna lozinka" required />
                <input className="form-control mb-2" name="newPassword" type="password" placeholder="Nova lozinka" required />
                <div className="form-hint mb-3">{passwordHint()}</div>
                <button className="btn btn-outline-danger" type="submit" disabled={passwordChanging}>
                  {passwordChanging ? "Cuvanje..." : "Promeni lozinku"}
                </button>
              </form>
            </section>
          </div>
        </div>
      </div>
    </div>
  );
}

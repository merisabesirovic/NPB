import { FormEvent, useState } from "react";
import { apiFetch } from "../api";
import type { FlashType } from "../types";
import { appendFileIf, appendIf, passwordHint } from "../utils/forms";

type Notify = (type: FlashType, text: string) => void;

export function RegisterPage({ notify, onShowLogin }: { notify: Notify; onShowLogin: () => void }) {
  const [driver, setDriver] = useState(false);
  const [loading, setLoading] = useState(false);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const raw = new FormData(form);
    const payload = new FormData();

    appendIf(payload, "FirstName", raw.get("firstName"));
    appendIf(payload, "LastName", raw.get("lastName"));
    appendIf(payload, "Email", raw.get("email"));
    appendIf(payload, "Password", raw.get("password"));
    appendIf(payload, "PhoneNumber", raw.get("phoneNumber"));
    appendIf(payload, "DateOfBirth", raw.get("dateOfBirth"));
    appendIf(payload, "Biography", raw.get("biography"));
    appendFileIf(payload, "Avatar", raw.get("avatar"));
    payload.append("RegisterAsDriver", driver ? "true" : "false");

    if (driver) {
      appendIf(payload, "DriverLicenseNumber", raw.get("driverLicenseNumber"));
      appendIf(payload, "VehicleModel", raw.get("vehicleModel"));
      appendIf(payload, "VehicleYear", raw.get("vehicleYear"));
      appendIf(payload, "VehicleSeats", raw.get("vehicleSeats"));
      appendFileIf(payload, "IdentityDocumentFile", raw.get("identityDocumentFile"));
    }

    setLoading(true);
    try {
      await apiFetch<{ id: string; email: string }>("/auth/register", {
        method: "POST",
        body: payload
      });
      form.reset();
      setDriver(false);
      notify("success", "Nalog je kreiran. Mozete da se prijavite.");
      onShowLogin();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Registracija nije uspela.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page-card p-4 auth-panel-wide">
      <div className="d-flex justify-content-between align-items-start gap-3 mb-3">
        <h2 className="h4 mb-0">Registracija</h2>
        <button className="btn btn-outline-secondary btn-sm" type="button" onClick={onShowLogin}>
          Nazad na prijavu
        </button>
      </div>
      <form onSubmit={submit}>
        <div className="row g-3">
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-first-name">
              Ime
            </label>
            <input className="form-control" id="register-first-name" name="firstName" required />
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-last-name">
              Prezime
            </label>
            <input className="form-control" id="register-last-name" name="lastName" required />
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-email">
              Email
            </label>
            <input className="form-control" id="register-email" name="email" type="email" required />
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-password">
              Lozinka
            </label>
            <input className="form-control" id="register-password" name="password" type="password" required />
            <div className="form-hint">{passwordHint()}</div>
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-phone">
              Telefon
            </label>
            <input className="form-control" id="register-phone" name="phoneNumber" />
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-date">
              Datum rodjenja
            </label>
            <input className="form-control" id="register-date" name="dateOfBirth" type="date" />
          </div>
          <div className="col-12">
            <label className="form-label" htmlFor="register-bio">
              Biografija
            </label>
            <textarea className="form-control" id="register-bio" name="biography" rows={2} />
          </div>
          <div className="col-md-6">
            <label className="form-label" htmlFor="register-avatar">
              Avatar
            </label>
            <input className="form-control" id="register-avatar" name="avatar" type="file" accept="image/*" />
          </div>
          <div className="col-md-6 d-flex align-items-end">
            <div className="form-check">
              <input className="form-check-input" id="register-driver" type="checkbox" checked={driver} onChange={(event) => setDriver(event.target.checked)} />
              <label className="form-check-label" htmlFor="register-driver">
                Registruj me i kao vozaca
              </label>
            </div>
          </div>
        </div>

        {driver && (
          <div className="row g-3 mt-1 border-top pt-3">
            <div className="col-md-6">
              <label className="form-label" htmlFor="register-license">
                Broj vozacke dozvole
              </label>
              <input className="form-control" id="register-license" name="driverLicenseNumber" required={driver} />
            </div>
            <div className="col-md-6">
              <label className="form-label" htmlFor="register-identity">
                ID dokument
              </label>
              <input className="form-control" id="register-identity" name="identityDocumentFile" type="file" required={driver} />
            </div>
            <div className="col-md-5">
              <label className="form-label" htmlFor="register-vehicle-model">
                Model vozila
              </label>
              <input className="form-control" id="register-vehicle-model" name="vehicleModel" />
            </div>
            <div className="col-md-3">
              <label className="form-label" htmlFor="register-vehicle-year">
                Godina
              </label>
              <input className="form-control" id="register-vehicle-year" name="vehicleYear" type="number" min="1950" max="2100" />
            </div>
            <div className="col-md-4">
              <label className="form-label" htmlFor="register-vehicle-seats">
                Sedista
              </label>
              <input className="form-control" id="register-vehicle-seats" name="vehicleSeats" type="number" min="1" defaultValue="4" />
            </div>
          </div>
        )}

        <button className="btn btn-success mt-3" type="submit" disabled={loading}>
          {loading ? "Kreiranje..." : "Kreiraj nalog"}
        </button>
      </form>
    </section>
  );
}

import { FormEvent, useCallback, useEffect, useState } from "react";
import { apiFetch, formatDateTime, rolesToText, shortId } from "../api";
import { ProfileLink } from "../components/ProfileLink";
import type { Dispute, DriverRequest, FlashType, UserProfile, VehicleVerificationRequest } from "../types";
import { asText, passwordHint } from "../utils/forms";
import { statusClass, statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;
type AdminTab = "verifications" | "vehicles" | "users" | "disputes";

const disputeStatuses = ["Open", "InReview", "Resolved", "Rejected"];

function documentTypeLabel(value?: string | number) {
  const key = String(value ?? "");
  const labels: Record<string, string> = {
    "0": "Vozacka dozvola",
    DriverLicense: "Vozacka dozvola",
    "1": "Saobracajna dozvola",
    RegistrationCertificate: "Saobracajna dozvola",
    "2": "ID dokument vozaca",
    IdentityCard: "ID dokument vozaca",
    "3": "ID dokument putnika",
    PassengerIdentityCard: "ID dokument putnika"
  };

  return labels[key] || "Dokument";
}

export function AdminPage({ notify, onOpenProfile }: { notify: Notify; onOpenProfile: (userId: string) => void }) {
  const [active, setActive] = useState<AdminTab>("verifications");
  const [requests, setRequests] = useState<DriverRequest[]>([]);
  const [vehicles, setVehicles] = useState<VehicleVerificationRequest[]>([]);
  const [users, setUsers] = useState<UserProfile[]>([]);
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const [requestData, vehicleData, userData, disputeData] = await Promise.all([
        apiFetch<DriverRequest[]>("/admin/verification-requests"),
        apiFetch<VehicleVerificationRequest[]>("/admin/vehicle-requests"),
        apiFetch<UserProfile[]>("/users/admin/all"),
        apiFetch<Dispute[]>("/disputes/admin/all")
      ]);
      setRequests(requestData || []);
      setVehicles(vehicleData || []);
      setUsers(userData || []);
      setDisputes(disputeData || []);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Admin podaci nisu ucitani.");
    } finally {
      setLoading(false);
    }
  }, [notify]);

  useEffect(() => {
    void load();
  }, [load]);

  const verificationAction = async (request: DriverRequest, action: "approve" | "reject") => {
    const documentId = request.documentId || request.id;
    if (!documentId) {
      notify("danger", "Nedostaje ID dokumenta.");
      return;
    }
    try {
      await apiFetch<unknown>(`/admin/${action}/${documentId}`, { method: "POST" });
      notify("success", action === "approve" ? "Dokument je odobren." : "Dokument je odbijen.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Verifikacija nije uspela.");
    }
  };

  const verifyVehicle = async (vehicle: VehicleVerificationRequest) => {
    try {
      await apiFetch<unknown>(`/admin/vehicles/${vehicle.vehicleId}/verify`, { method: "POST" });
      notify("success", "Vozilo je verifikovano.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Verifikacija vozila nije uspela.");
    }
  };

  const createAdmin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = event.currentTarget;
    const raw = new FormData(form);
    try {
      await apiFetch<UserProfile>("/users/admin/create", {
        method: "POST",
        body: {
          email: asText(raw.get("email")),
          password: asText(raw.get("password")),
          firstName: asText(raw.get("firstName")),
          lastName: asText(raw.get("lastName")),
          isAdmin: true,
          isVerified: true
        }
      });
      notify("success", "Admin je kreiran.");
      form.reset();
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Kreiranje admina nije uspelo.");
    }
  };

  const deleteUser = async (user: UserProfile) => {
    if (!window.confirm(`Obrisati ${user.email}?`)) return;
    try {
      await apiFetch<void>(`/users/admin/${user.id}`, { method: "DELETE" });
      notify("success", "Korisnik je obrisan.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Brisanje nije uspelo.");
    }
  };

  const changeUserPassword = async (user: UserProfile) => {
    const newPassword = window.prompt(`Nova lozinka za ${user.email}`);
    if (!newPassword) return;
    try {
      await apiFetch<void>(`/users/admin/${user.id}/change-password`, {
        method: "POST",
        body: { newPassword }
      });
      notify("success", "Lozinka je promenjena.");
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Promena lozinke nije uspela.");
    }
  };

  const changeDispute = async (event: FormEvent<HTMLFormElement>, disputeId: string) => {
    event.preventDefault();
    const raw = new FormData(event.currentTarget);
    try {
      await apiFetch<Dispute>(`/disputes/${disputeId}/status`, {
        method: "POST",
        body: {
          status: asText(raw.get("status")),
          resolution: asText(raw.get("resolution"))
        }
      });
      notify("success", "Spor je azuriran.");
      await load();
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Azuriranje spora nije uspelo.");
    }
  };

  const tabs: Array<{ key: AdminTab; label: string; count: number }> = [
    { key: "verifications", label: "Verifikacije", count: requests.length },
    { key: "vehicles", label: "Vozila", count: vehicles.length },
    { key: "users", label: "Korisnici", count: users.length },
    { key: "disputes", label: "Sporovi", count: disputes.length }
  ];

  return (
    <div className="row g-3">
      <div className="col-12">
        <section className="page-card p-4">
          <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <h2 className="h5 mb-0">Admin panel</h2>
            <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void load()} disabled={loading}>
              Osvezi
            </button>
          </div>
          <div className="nav nav-pills gap-2">
            {tabs.map((tab) => (
              <button className={`nav-link ${active === tab.key ? "active" : ""}`} type="button" key={tab.key} onClick={() => setActive(tab.key)}>
                {tab.label}
                <span className="badge text-bg-light ms-2">{tab.count}</span>
              </button>
            ))}
          </div>
        </section>
      </div>

      {active === "verifications" && (
        <div className="col-12">
          <section className="page-card p-4">
            <h3 className="h6 mb-3">Verifikacije dokumenata</h3>
            <div className="table-responsive">
              <table className="table table-sm align-middle">
                <thead>
                  <tr>
                    <th>Korisnik</th>
                    <th>Dokument</th>
                    <th>Upload</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {requests.map((request) => (
                    <tr key={request.documentId || request.id}>
                      <td>
                        <ProfileLink
                          userId={request.userId}
                          firstName={request.userFirstName}
                          lastName={request.userLastName}
                          email={request.userEmail || request.email}
                          onOpen={onOpenProfile}
                        />
                      </td>
                      <td>
                        <a className="driver-doc-link d-inline-block truncate" href={request.fileUrl} target="_blank" rel="noreferrer">
                          {documentTypeLabel(request.documentType)}
                        </a>
                      </td>
                      <td>{formatDateTime(request.uploadedAt)}</td>
                      <td className="text-end">
                        <div className="btn-group btn-group-sm">
                          <button className="btn btn-outline-success" type="button" onClick={() => void verificationAction(request, "approve")}>
                            Odobri
                          </button>
                          <button className="btn btn-outline-danger" type="button" onClick={() => void verificationAction(request, "reject")}>
                            Odbij
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {requests.length === 0 && <div className="small-muted">Nema zahteva na cekanju.</div>}
          </section>
        </div>
      )}

      {active === "vehicles" && (
        <div className="col-12">
          <section className="page-card p-4">
            <h3 className="h6 mb-3">Verifikacija vozila</h3>
            <div className="table-responsive">
              <table className="table table-sm align-middle">
                <thead>
                  <tr>
                    <th>Vlasnik</th>
                    <th>Vozilo</th>
                    <th>Dozvola</th>
                    <th>Slika</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {vehicles.map((vehicle) => (
                    <tr key={vehicle.vehicleId}>
                      <td>
                        <ProfileLink
                          userId={vehicle.userId}
                          firstName={vehicle.userFirstName}
                          lastName={vehicle.userLastName}
                          email={vehicle.userEmail}
                          onOpen={onOpenProfile}
                        />
                      </td>
                      <td>
                        <div className="fw-semibold">{vehicle.model || "Vozilo"}</div>
                        <div className="small-muted">
                          {vehicle.year || "-"} - {vehicle.seatsCount || "-"} sedista
                        </div>
                      </td>
                      <td>{vehicle.licenseNumber || "-"}</td>
                      <td>
                        {vehicle.vehicleImageUrl ? (
                          <a className="driver-doc-link" href={vehicle.vehicleImageUrl} target="_blank" rel="noreferrer">
                            Otvori sliku
                          </a>
                        ) : (
                          <span className="small-muted">Nema slike</span>
                        )}
                      </td>
                      <td className="text-end">
                        <button className="btn btn-sm btn-outline-success" type="button" onClick={() => void verifyVehicle(vehicle)}>
                          Verifikuj
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {vehicles.length === 0 && <div className="small-muted">Nema vozila koja cekaju verifikaciju.</div>}
          </section>
        </div>
      )}

      {active === "users" && (
        <>
          <div className="col-xl-5">
            <section className="page-card p-4 h-100">
              <h3 className="h6 mb-3">Kreiraj admina</h3>
              <form onSubmit={createAdmin}>
                <div className="row g-2">
                  <div className="col-md-6">
                    <input className="form-control" name="firstName" placeholder="Ime" required />
                  </div>
                  <div className="col-md-6">
                    <input className="form-control" name="lastName" placeholder="Prezime" required />
                  </div>
                  <div className="col-md-6">
                    <input className="form-control" name="email" type="email" placeholder="Email" required />
                  </div>
                  <div className="col-md-6">
                    <input className="form-control" name="password" type="password" placeholder="Lozinka" required />
                  </div>
                </div>
                <div className="form-hint mt-2">{passwordHint()}</div>
                <button className="btn btn-primary mt-3" type="submit">
                  Kreiraj admina
                </button>
              </form>
            </section>
          </div>

          <div className="col-xl-7">
            <section className="page-card p-4">
              <h3 className="h6 mb-3">Korisnici</h3>
              <div className="table-responsive">
                <table className="table table-sm align-middle">
                  <thead>
                    <tr>
                      <th>Korisnik</th>
                      <th>Uloge</th>
                      <th>Trust</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((user) => (
                      <tr key={user.id}>
                        <td>
                          <ProfileLink userId={user.id} firstName={user.firstName} lastName={user.lastName} email={user.email} isVerified={user.isVerified} onOpen={onOpenProfile} />
                        </td>
                        <td>{rolesToText(user.roles)}</td>
                        <td>{Math.round(user.trustScore || 0)}</td>
                        <td className="text-end">
                          <div className="btn-group btn-group-sm">
                            <button className="btn btn-outline-primary" type="button" onClick={() => void changeUserPassword(user)}>
                              Lozinka
                            </button>
                            <button className="btn btn-outline-danger" type="button" onClick={() => void deleteUser(user)}>
                              Obrisi
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          </div>
        </>
      )}

      {active === "disputes" && (
        <div className="col-12">
          <section className="page-card p-4">
            <h3 className="h6 mb-3">Sporovi</h3>
            <div className="list-group">
              {disputes.map((dispute) => (
                <form className="list-group-item" key={dispute.id} onSubmit={(event) => void changeDispute(event, dispute.id)}>
                  <div className="d-flex justify-content-between gap-2 mb-2">
                    <div>
                      <div className="fw-semibold">
                        {dispute.startAddress || "Booking"} {"->"} {dispute.destinationAddress || shortId(dispute.bookingId)}
                      </div>
                      <div className="small-muted">
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
                    </div>
                    <span className={`status-chip ${statusClass(dispute.status)}`}>{statusLabel(dispute.status)}</span>
                  </div>
                  <div className="pre-wrap mb-2">{dispute.description}</div>
                  <select className="form-select form-select-sm mb-2" name="status" defaultValue={dispute.status}>
                    {disputeStatuses.map((status) => (
                      <option key={status} value={status}>
                        {statusLabel(status)}
                      </option>
                    ))}
                  </select>
                  <textarea className="form-control form-control-sm mb-2" name="resolution" defaultValue={dispute.resolution || ""} placeholder="Resenje" rows={2} />
                  <button className="btn btn-sm btn-outline-primary" type="submit">
                    Sacuvaj
                  </button>
                </form>
              ))}
            </div>
            {disputes.length === 0 && <div className="small-muted">Nema sporova.</div>}
          </section>
        </div>
      )}
    </div>
  );
}

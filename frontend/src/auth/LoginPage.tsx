import { FormEvent, useState } from "react";
import { apiFetch } from "../api";
import type { FlashType, LoginResponse } from "../types";
import { asText } from "../utils/forms";

type Notify = (type: FlashType, text: string) => void;

export function LoginPage({
  onLogin,
  notify,
  onShowRegister
}: {
  onLogin: (token: string, pending?: boolean) => void;
  notify: Notify;
  onShowRegister: () => void;
}) {
  const [loading, setLoading] = useState(false);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setLoading(true);

    try {
      const response = await apiFetch<LoginResponse>("/auth/login", {
        method: "POST",
        body: {
          email: asText(form.get("email")),
          password: asText(form.get("password"))
        }
      });
      onLogin(response.token, response.driverVerificationPending);
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Login nije uspeo.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page-card p-4 auth-panel">
      <h2 className="h4 mb-3">Prijava</h2>
      <form onSubmit={submit}>
        <div className="mb-3">
          <label className="form-label" htmlFor="login-email">
            Email
          </label>
          <input className="form-control" id="login-email" name="email" type="email" autoComplete="email" required />
        </div>
        <div className="mb-3">
          <label className="form-label" htmlFor="login-password">
            Lozinka
          </label>
          <input className="form-control" id="login-password" name="password" type="password" autoComplete="current-password" required />
        </div>
        <button className="btn btn-primary w-100" type="submit" disabled={loading}>
          {loading ? "Prijava..." : "Prijavi se"}
        </button>
      </form>
      <div className="border-top mt-4 pt-3 text-center">
        <button className="btn btn-link" type="button" onClick={onShowRegister}>
          Nemate nalog? Registracija
        </button>
      </div>
    </section>
  );
}


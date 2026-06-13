const TOKEN_KEY = "rideMateToken";

type ApiBody = BodyInit | Record<string, unknown> | null | undefined;

interface ApiOptions extends Omit<RequestInit, "body"> {
  body?: ApiBody;
  token?: string | null;
}

export class ApiError extends Error {
  status: number;
  payload: unknown;

  constructor(message: string, status: number, payload: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

export function getStoredToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function storeToken(token: string) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function apiBaseUrl() {
  return import.meta.env.VITE_API_BASE_URL || "/api";
}

export function buildQuery(params: Record<string, string | number | boolean | null | undefined>) {
  const search = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") return;
    search.set(key, String(value));
  });

  const query = search.toString();
  return query ? `?${query}` : "";
}

function isJsonCandidate(body: ApiBody): body is Record<string, unknown> {
  return !!body && typeof body === "object" && !(body instanceof FormData) && !(body instanceof URLSearchParams) && !(body instanceof Blob);
}

function normalizePath(path: string) {
  if (/^https?:\/\//i.test(path)) return path;
  const base = apiBaseUrl().replace(/\/$/, "");
  const cleanPath = path.startsWith("/") ? path : `/${path}`;
  return `${base}${cleanPath}`;
}

async function parseResponse(response: Response) {
  const text = await response.text();
  if (!text) return null;

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

function errorMessage(payload: unknown, fallback: string) {
  const translate = (message: string) => translateErrorMessage(message);
  if (typeof payload === "string") return translate(payload);
  if (payload && typeof payload === "object") {
    const objectPayload = payload as { error?: string; message?: string; title?: string; errors?: Record<string, string[]> };
    if (objectPayload.errors) {
      const firstError = Object.values(objectPayload.errors).flat().find(Boolean);
      if (firstError) return translate(firstError);
    }
    return translate(objectPayload.error || objectPayload.message || objectPayload.title || fallback);
  }
  return translate(fallback);
}

function translateErrorMessage(message: string) {
  const exact: Record<string, string> = {
    "Ride not found": "Voznja nije pronadjena.",
    "Drivers cannot book their own rides": "Ne mozete rezervisati sopstvenu voznju.",
    "RideDate is required for recurring rides": "Datum voznje je obavezan za ponavljajuce voznje.",
    "Ride is not available on requested date": "Voznja nije dostupna za izabrani datum.",
    "SeatsReserved must be > 0": "Broj sedista mora biti veci od 0.",
    "Ride not open for booking": "Voznja trenutno nije otvorena za rezervacije.",
    "Not enough available seats for selected date": "Nema dovoljno slobodnih sedista za izabrani datum.",
    "Not enough available seats": "Nema dovoljno slobodnih sedista.",
    "Not enough seats to approve booking for that date": "Nema dovoljno slobodnih sedista za odobravanje te rezervacije.",
    "You already have an active booking for this ride": "Vec imate aktivnu rezervaciju za ovu voznju.",
    "Invalid payment method": "Neispravan nacin placanja.",
    "This booking cannot be changed": "Ova rezervacija ne moze da se menja.",
    "Only approved bookings can be paid online": "Online placanje je moguce tek kada vozac odobri rezervaciju.",
    "Online bookings must be paid by passenger": "Online rezervaciju mora da plati putnik.",
    "Card data is required": "Podaci kartice su obavezni.",
    "Only approved bookings can be marked paid": "Samo odobrene rezervacije mogu biti oznacene kao placene.",
    "Only pending bookings can be approved": "Samo rezervacije na cekanju mogu biti odobrene.",
    "Only pending bookings can be rejected": "Samo rezervacije na cekanju mogu biti odbijene.",
    "A dispute already exists for this booking": "Spor za ovu rezervaciju vec postoji.",
    "A review already exists for this booking": "Recenzija za ovu rezervaciju vec postoji.",
    "Reviews can only be left after the ride is completed": "Recenziju mozete ostaviti tek nakon zavrsene voznje.",
    "Rating must be between 1 and 5": "Ocena mora biti od 1 do 5.",
    "Booking not found": "Rezervacija nije pronadjena.",
    "Reviewed user not found": "Korisnik za ocenu nije pronadjen.",
    "Reviewed user did not participate in this booking": "Korisnik nije ucestvovao u ovoj rezervaciji.",
    "Email and password required": "Email i lozinka su obavezni.",
    "Invalid credentials": "Pogresan email ili lozinka.",
    "Invalid refresh token": "Sesija je istekla. Prijavite se ponovo.",
    "Invalid email format": "Email format nije ispravan.",
    "Email already in use": "Email je vec zauzet.",
    "Password is required": "Lozinka je obavezna.",
    "Password must be at least 8 characters long": "Lozinka mora imati najmanje 8 karaktera.",
    "Password must contain at least one uppercase letter": "Lozinka mora imati bar jedno veliko slovo.",
    "Password must contain at least one lowercase letter": "Lozinka mora imati bar jedno malo slovo.",
    "Password must contain at least one digit": "Lozinka mora imati bar jedan broj.",
    "Password must contain at least one special character": "Lozinka mora imati bar jedan specijalni znak.",
    "Admin may only create another admin via this endpoint": "Admin moze da kreira samo drugog admina.",
    "Driver registration requires DriverLicenseNumber": "Broj vozacke dozvole je obavezan za registraciju vozaca.",
    "Driver registration requires identity document upload": "Dokument za verifikaciju identiteta je obavezan za registraciju vozaca.",
    "Driver profile is waiting for admin approval": "Vozacki profil ceka odobrenje admina.",
    "Identity is already verified": "Identitet je vec verifikovan.",
    "Passenger identity document is already pending approval": "ID dokument vec ceka odobrenje admina.",
    "Vehicle not found": "Vozilo nije pronadjeno.",
    "Vehicle already verified": "Vozilo je vec verifikovano.",
    "Profile was changed while you were editing. Refresh profile and try again.": "Profil je izmenjen u medjuvremenu. Osvezite profil i pokusajte ponovo.",
    "Booking status is no longer used. Ride is available for booking as soon as it is created.": "Status otvoreno za rezervacije se vise ne koristi. Voznja je dostupna za rezervisanje cim je kreirana.",
    "Completed ride status cannot be changed": "Zavrsena voznja vise ne moze da menja status.",
    "Cancelled ride status cannot be changed": "Otkazana voznja vise ne moze da menja status.",
    "Driver already has a ride in that time range": "Vec imate voznju u tom vremenskom periodu.",
    "Ride cannot be started before departure time": "Voznja ne moze da krene pre vremena polaska.",
    "Ride must be in progress before it can be completed": "Voznja mora biti u toku pre nego sto moze da se oznaci kao zavrsena.",
    "Ride cannot be completed before destination time": "Voznja ne moze da se zavrsi pre vremena dolaska.",
    "Passenger bookings cannot be cancelled after ride has started": "Rezervacija ne moze da se otkaze nakon sto je voznja krenula.",
    "Completed bookings cannot be cancelled": "Zavrsena rezervacija ne moze da se otkaze."
  };

  return exact[message] || message;
}

export async function apiFetch<T>(path: string, options: ApiOptions = {}): Promise<T> {
  const headers = new Headers(options.headers);
  const token = options.token ?? getStoredToken();
  let body = options.body;

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  if (isJsonCandidate(body)) {
    headers.set("Content-Type", "application/json");
    body = JSON.stringify(body);
  }

  const response = await fetch(normalizePath(path), {
    ...options,
    headers,
    body: body as BodyInit | null | undefined
  });
  const payload = await parseResponse(response);

  if (!response.ok) {
    throw new ApiError(errorMessage(payload, `Request failed (${response.status})`), response.status, payload);
  }

  return payload as T;
}

export function shortId(id?: string) {
  return id ? id.slice(0, 8) : "-";
}

export function formatDateTime(value?: string) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

export function formatDateForInput(value?: string) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  return date.toISOString().slice(0, 10);
}

export function formatDateTimeForInput(value?: string) {
  if (!value) return "";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "";
  const offset = date.getTimezoneOffset();
  const local = new Date(date.getTime() - offset * 60_000);
  return local.toISOString().slice(0, 16);
}

export function inputDateTimeToIso(value: FormDataEntryValue | null) {
  if (typeof value !== "string" || !value) return "";
  return new Date(value).toISOString();
}

export function currency(value?: number) {
  if (value === undefined || value === null || Number.isNaN(value)) return "-";
  return new Intl.NumberFormat("sr-RS", { style: "currency", currency: "RSD", maximumFractionDigits: 0 }).format(value);
}

export function rolesToText(flags?: number) {
  const roles: string[] = [];
  if (!flags) return "Nema uloge";
  if ((flags & 4) === 4) return "Admin";
  if ((flags & 1) === 1) roles.push("Vozac");
  if ((flags & 2) === 2) roles.push("Putnik");
  if ((flags & 4) === 4) roles.push("Admin");
  return roles.join(", ") || "Nema uloge";
}

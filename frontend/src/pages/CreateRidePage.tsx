import { FormEvent, useState } from "react";
import { apiFetch } from "../api";
import { CityDatalist, CityInput } from "../components/CityInput";
import type { FlashType, Ride } from "../types";
import { defaultDateTime, inputDateTimeToIso } from "../utils/forms";
import { statusLabel } from "../utils/status";

type Notify = (type: FlashType, text: string) => void;

const rideTypes = ["OneTime", "RecurringWeekdays", "RecurringWeekend", "LongTerm"];

export function CreateRidePage({ notify }: { notify: Notify }) {
  const [form, setForm] = useState({
    startAddress: "Novi Sad",
    startLatitude: "45.2671",
    startLongitude: "19.8335",
    destinationAddress: "Beograd",
    destinationLatitude: "44.8125",
    destinationLongitude: "20.4612",
    departureDateTime: defaultDateTime(24),
    destinationDateTime: defaultDateTime(25),
    availableSeats: "3",
    pricePerSeat: "900",
    rideType: "OneTime",
    autoApproveBookings: true,
    smokingAllowed: false,
    petsAllowed: false,
    luggageAllowed: true,
    musicAllowed: true,
    conversationAllowed: true
  });
  const [loading, setLoading] = useState(false);

  const setField = (key: keyof typeof form, value: string | boolean) => {
    setForm((current) => ({ ...current, [key]: value }));
  };

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setLoading(true);
    try {
      await apiFetch<Ride>("/rides", {
        method: "POST",
        body: {
          startAddress: form.startAddress,
          startLatitude: Number(form.startLatitude),
          startLongitude: Number(form.startLongitude),
          destinationAddress: form.destinationAddress,
          destinationLatitude: Number(form.destinationLatitude),
          destinationLongitude: Number(form.destinationLongitude),
          departureDateTime: inputDateTimeToIso(form.departureDateTime),
          destinationDateTime: inputDateTimeToIso(form.destinationDateTime),
          availableSeats: Number(form.availableSeats),
          pricePerSeat: Number(form.pricePerSeat),
          rideType: form.rideType,
          autoApproveBookings: form.autoApproveBookings,
          smokingAllowed: form.smokingAllowed,
          petsAllowed: form.petsAllowed,
          luggageAllowed: form.luggageAllowed,
          musicAllowed: form.musicAllowed,
          conversationAllowed: form.conversationAllowed
        }
      });
      notify("success", "Voznja je kreirana.");
      setForm((current) => ({
        ...current,
        departureDateTime: defaultDateTime(24),
        destinationDateTime: defaultDateTime(25)
      }));
    } catch (error) {
      notify("danger", error instanceof Error ? error.message : "Kreiranje voznje nije uspelo.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="page-card p-4">
      <CityDatalist />
      <h2 className="h5 mb-3">Nova voznja</h2>
      <form onSubmit={submit}>
        <div className="row g-3">
          <div className="col-lg-6">
            <div className="section-title">Polazak</div>
            <CityInput id="ride-start-city" label="Grad / adresa polaska" value={form.startAddress} onChange={(value) => setField("startAddress", value)} required />
            <div className="row g-2 mt-1">
              <div className="col-6">
                <label className="form-label" htmlFor="ride-start-lat">
                  Lat
                </label>
                <input className="form-control" id="ride-start-lat" value={form.startLatitude} onChange={(event) => setField("startLatitude", event.target.value)} required />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="ride-start-lng">
                  Lng
                </label>
                <input className="form-control" id="ride-start-lng" value={form.startLongitude} onChange={(event) => setField("startLongitude", event.target.value)} required />
              </div>
            </div>
          </div>

          <div className="col-lg-6">
            <div className="section-title">Odrediste</div>
            <CityInput id="ride-destination-city" label="Grad / adresa odredista" value={form.destinationAddress} onChange={(value) => setField("destinationAddress", value)} required />
            <div className="row g-2 mt-1">
              <div className="col-6">
                <label className="form-label" htmlFor="ride-destination-lat">
                  Lat
                </label>
                <input className="form-control" id="ride-destination-lat" value={form.destinationLatitude} onChange={(event) => setField("destinationLatitude", event.target.value)} required />
              </div>
              <div className="col-6">
                <label className="form-label" htmlFor="ride-destination-lng">
                  Lng
                </label>
                <input className="form-control" id="ride-destination-lng" value={form.destinationLongitude} onChange={(event) => setField("destinationLongitude", event.target.value)} required />
              </div>
            </div>
          </div>

          <div className="col-md-3">
            <label className="form-label" htmlFor="ride-departure">
              Polazak
            </label>
            <input className="form-control" id="ride-departure" type="datetime-local" value={form.departureDateTime} onChange={(event) => setField("departureDateTime", event.target.value)} required />
          </div>
          <div className="col-md-3">
            <label className="form-label" htmlFor="ride-arrival">
              Dolazak
            </label>
            <input className="form-control" id="ride-arrival" type="datetime-local" value={form.destinationDateTime} onChange={(event) => setField("destinationDateTime", event.target.value)} required />
          </div>
          <div className="col-md-2">
            <label className="form-label" htmlFor="ride-seats">
              Sedista
            </label>
            <input className="form-control" id="ride-seats" type="number" min="1" value={form.availableSeats} onChange={(event) => setField("availableSeats", event.target.value)} required />
          </div>
          <div className="col-md-2">
            <label className="form-label" htmlFor="ride-price">
              Cena (RSD)
            </label>
            <input className="form-control" id="ride-price" type="number" min="1" step="1" value={form.pricePerSeat} onChange={(event) => setField("pricePerSeat", event.target.value)} required />
          </div>
          <div className="col-md-2">
            <label className="form-label" htmlFor="ride-type">
              Tip
            </label>
            <select className="form-select" id="ride-type" value={form.rideType} onChange={(event) => setField("rideType", event.target.value)}>
              {rideTypes.map((type) => (
                <option key={type} value={type}>
                  {statusLabel(type)}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="row g-2 mt-3">
          {[
            ["autoApproveBookings", "Automatska potvrda"],
            ["smokingAllowed", "Pusenje"],
            ["petsAllowed", "Ljubimci"],
            ["luggageAllowed", "Prtljag"],
            ["musicAllowed", "Muzika"],
            ["conversationAllowed", "Razgovor"]
          ].map(([key, label]) => (
            <div className="col-sm-6 col-lg-2" key={key}>
              <div className="form-check">
                <input
                  className="form-check-input"
                  id={`ride-${key}`}
                  type="checkbox"
                  checked={Boolean(form[key as keyof typeof form])}
                  onChange={(event) => setField(key as keyof typeof form, event.target.checked)}
                />
                <label className="form-check-label" htmlFor={`ride-${key}`}>
                  {label}
                </label>
              </div>
            </div>
          ))}
        </div>

        <button className="btn btn-success mt-3" type="submit" disabled={loading}>
          {loading ? "Kreiranje..." : "Objavi voznju"}
        </button>
      </form>
    </section>
  );
}

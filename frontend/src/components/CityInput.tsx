import { serbiaCities } from "../data/serbiaCities";

export function CityInput({
  id,
  label,
  value,
  onChange,
  required = false
}: {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
}) {
  return (
    <>
      <label className="form-label" htmlFor={id}>
        {label}
      </label>
      <input className="form-control" id={id} list="serbia-city-options" value={value} onChange={(event) => onChange(event.target.value)} required={required} />
    </>
  );
}

export function CityDatalist() {
  return (
    <datalist id="serbia-city-options">
      {serbiaCities.map((city) => (
        <option key={city} value={city} />
      ))}
    </datalist>
  );
}


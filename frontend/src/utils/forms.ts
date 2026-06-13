export function asText(value: FormDataEntryValue | null) {
  return typeof value === "string" ? value.trim() : "";
}

export function appendIf(formData: FormData, key: string, value: FormDataEntryValue | null) {
  if (typeof value === "string" && value.trim() !== "") formData.append(key, value.trim());
}

export function appendFileIf(formData: FormData, key: string, value: FormDataEntryValue | null) {
  if (value instanceof File && value.size > 0) formData.append(key, value);
}

export function inputDateTimeToIso(value: string) {
  if (!value) return "";
  return new Date(value).toISOString();
}

export function defaultDateTime(hoursFromNow: number) {
  const date = new Date(Date.now() + hoursFromNow * 60 * 60 * 1000);
  const offset = date.getTimezoneOffset();
  return new Date(date.getTime() - offset * 60_000).toISOString().slice(0, 16);
}

export function passwordHint() {
  return "Min 8 karaktera, veliko i malo slovo, broj i specijalni znak.";
}


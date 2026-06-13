export function statusClass(status?: string) {
  if (!status) return "";
  if (["Approved", "Completed", "Paid", "Resolved"].includes(status)) return "success";
  if (["Pending", "Scheduled", "BookingOpen", "BookingClosed", "InReview", "InProgress"].includes(status)) return "warn";
  if (["Rejected", "Cancelled", "Refunded"].includes(status)) return "danger";
  return "";
}

export function statusLabel(status?: string) {
  if (!status) return "-";

  const labels: Record<string, string> = {
    Scheduled: "Zakazano",
    BookingOpen: "Zakazano",
    BookingClosed: "Zakazano",
    InProgress: "U toku",
    Completed: "Zavrseno",
    Cancelled: "Otkazano",
    Pending: "Na cekanju",
    Approved: "Odobreno",
    Rejected: "Odbijeno",
    Paid: "Placeno",
    Refunded: "Refundirano",
    Open: "Otvoren",
    InReview: "U obradi",
    Resolved: "Resen",
    Cash: "Kes",
    Online: "Online",
    OneTime: "Jednokratna",
    RecurringWeekdays: "Radnim danima",
    RecurringWeekend: "Vikendom",
    LongTerm: "Dugorocna"
  };

  return labels[status] || status;
}

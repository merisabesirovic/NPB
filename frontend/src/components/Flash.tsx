import type { FlashMessage } from "../types";

export function Flash({ flash, onDismiss }: { flash: FlashMessage | null; onDismiss: () => void }) {
  if (!flash) return null;
  return (
    <div className={`alert alert-${flash.type} d-flex justify-content-between align-items-start gap-3`} role="alert">
      <span>{flash.text}</span>
      <button className="btn-close" type="button" aria-label="Close" onClick={onDismiss} />
    </div>
  );
}


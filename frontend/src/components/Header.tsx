import type { ReactNode } from "react";

export function Header({ right }: { right?: ReactNode }) {
  return (
    <header className="topbar py-3">
      <div className="container-fluid px-4 d-flex flex-wrap justify-content-between align-items-center gap-3">
        <div className="d-flex align-items-center gap-3">
          <span className="brand-mark">RM</span>
          <div>
            <h1 className="h4 mb-0">Ride Mate</h1>
          </div>
        </div>
        {right}
      </div>
    </header>
  );
}


import { useCallback, useEffect, useMemo, useState } from "react";
import { ApiError, apiFetch, clearToken, rolesToText, storeToken } from "./api";
import { LoginPage } from "./auth/LoginPage";
import { RegisterPage } from "./auth/RegisterPage";
import { Flash } from "./components/Flash";
import { Header } from "./components/Header";
import { ProfileModal } from "./components/ProfileModal";
import { VerificationBadge } from "./components/ProfileLink";
import { AdminPage } from "./pages/AdminPage";
import { BookingsPage } from "./pages/BookingsPage";
import { CreateRidePage } from "./pages/CreateRidePage";
import { DisputesReviewsPage } from "./pages/DisputesReviewsPage";
import { MyRidesPage } from "./pages/MyRidesPage";
import { NotificationsPage } from "./pages/NotificationsPage";
import { ProfilePage } from "./pages/ProfilePage";
import { SearchRidesPage } from "./pages/SearchRidesPage";
import type { FlashMessage, FlashType, NotificationItem, TabKey, UserProfile } from "./types";
import { ROLE } from "./types";

type AuthView = "login" | "register";

function App() {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem("rideMateToken"));
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [profileLoading, setProfileLoading] = useState(false);
  const [authChecked, setAuthChecked] = useState(false);
  const [activeTab, setActiveTab] = useState<TabKey>("search");
  const [authView, setAuthView] = useState<AuthView>("login");
  const [flash, setFlash] = useState<FlashMessage | null>(null);
  const [profileModalUserId, setProfileModalUserId] = useState<string | null>(null);
  const [unreadNotifications, setUnreadNotifications] = useState(0);
  const [selectedBookingId, setSelectedBookingId] = useState<string | null>(null);

  const notify = useCallback((type: FlashType, text: string) => {
    setFlash({ type, text });
  }, []);

  const refreshProfile = useCallback(async (options?: { quiet401?: boolean }) => {
    if (!localStorage.getItem("rideMateToken")) {
      setAuthChecked(true);
      return;
    }

    setProfileLoading(true);
    try {
      const data = await apiFetch<UserProfile>("/profile");
      setProfile(data);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Ne mogu da ucitam profil.";
      if (error instanceof ApiError && error.status === 401) {
        clearToken();
        setToken(null);
        setProfile(null);
        if (!options?.quiet401) notify("danger", "Sesija je istekla. Prijavite se ponovo.");
        return;
      }
      notify("danger", message);
    } finally {
      setProfileLoading(false);
      setAuthChecked(true);
    }
  }, [notify]);

  useEffect(() => {
    if (token) {
      setAuthChecked(false);
      void refreshProfile({ quiet401: true });
      return;
    }
    setProfile(null);
    setAuthChecked(true);
  }, [token, refreshProfile]);

  const refreshUnreadNotifications = useCallback(async () => {
    if (!localStorage.getItem("rideMateToken")) return;
    try {
      const notifications = await apiFetch<NotificationItem[]>("/notifications/me");
      setUnreadNotifications(notifications.filter((item) => !item.isRead).length);
    } catch {
      setUnreadNotifications(0);
    }
  }, []);

  useEffect(() => {
    if (token && authChecked) void refreshUnreadNotifications();
  }, [authChecked, refreshUnreadNotifications, token]);

  const login = (newToken: string, pending?: boolean) => {
    storeToken(newToken);
    setProfile(null);
    setUnreadNotifications(0);
    setAuthChecked(false);
    setToken(newToken);
    setActiveTab("search");
    notify(pending ? "warning" : "success", pending ? "Ulogovani ste. Verifikacija vozaca je jos na cekanju." : "Uspesna prijava.");
  };

  const logout = () => {
    clearToken();
    setToken(null);
    setProfile(null);
    setActiveTab("search");
    setAuthView("login");
    notify("info", "Odjavljeni ste.");
  };

  const roleFlags = typeof profile?.roles === "number" ? profile.roles : 0;
  const isDriver = (roleFlags & ROLE.Driver) === ROLE.Driver;
  const isAdmin = (roleFlags & ROLE.Admin) === ROLE.Admin;
  const driverVerificationPending = Boolean(profile?.driverVerificationPending || profile?.driverVerificationStatus === "Pending");

  const tabs = useMemo(() => {
    if (isAdmin) {
      return [{ key: "admin" as TabKey, label: "Admin", show: true }];
    }

    const items: Array<{ key: TabKey; label: string; show: boolean }> = [
      { key: "search", label: "Pretraga", show: true },
      { key: "profile", label: "Profil", show: true },
      { key: "createRide", label: "Nova voznja", show: isDriver },
      { key: "myRides", label: "Moje voznje", show: isDriver },
      { key: "bookings", label: "Rezervacije", show: true },
      { key: "notifications", label: "Notifikacije", show: true },
      { key: "disputes", label: "Ocene i sporovi", show: true },
      { key: "admin", label: "Admin", show: false }
    ];
    return items.filter((item) => item.show);
  }, [isAdmin, isDriver]);

  useEffect(() => {
    if (isAdmin && activeTab !== "admin") setActiveTab("admin");
  }, [activeTab, isAdmin]);

  useEffect(() => {
    if (!tabs.some((tab) => tab.key === activeTab)) {
      setActiveTab(tabs[0]?.key ?? "search");
    }
  }, [activeTab, tabs]);

  const openProfile = (userId: string) => {
    setProfileModalUserId(userId);
  };

  const openBooking = (bookingId: string) => {
    setSelectedBookingId(bookingId);
    setActiveTab("bookings");
  };

  const renderTab = () => {
    switch (activeTab) {
      case "profile":
        return <ProfilePage profile={profile} refreshProfile={refreshProfile} notify={notify} onLogout={logout} />;
      case "createRide":
        return <CreateRidePage notify={notify} />;
      case "myRides":
        return <MyRidesPage notify={notify} onOpenProfile={openProfile} />;
      case "bookings":
        return <BookingsPage notify={notify} profile={profile} isDriver={isDriver} onOpenProfile={openProfile} selectedBookingId={selectedBookingId} onSelectedBookingHandled={() => setSelectedBookingId(null)} />;
      case "notifications":
        return <NotificationsPage notify={notify} onUnreadChange={setUnreadNotifications} onOpenBooking={openBooking} />;
      case "disputes":
        return <DisputesReviewsPage notify={notify} onOpenProfile={openProfile} />;
      case "admin":
        return <AdminPage notify={notify} onOpenProfile={openProfile} />;
      case "search":
      default:
        return <SearchRidesPage notify={notify} profile={profile} onOpenProfile={openProfile} onOpenBooking={openBooking} />;
    }
  };

  if (!token) {
    return (
      <div className="app-shell">
        <Header />
        <main className="auth-wrap">
          <Flash flash={flash} onDismiss={() => setFlash(null)} />
          {authView === "login" ? (
            <LoginPage onLogin={login} notify={notify} onShowRegister={() => setAuthView("register")} />
          ) : (
            <RegisterPage notify={notify} onShowLogin={() => setAuthView("login")} />
          )}
        </main>
      </div>
    );
  }

  if (!authChecked) {
    return (
      <div className="app-shell">
        <Header />
        <main className="content-wrap">
          <div className="page-card p-4">
            <div className="small-muted">Proveravam sesiju...</div>
          </div>
        </main>
      </div>
    );
  }

  return (
    <div className="app-shell">
      <Header
        right={
          <div className="d-flex flex-wrap align-items-center gap-2">
            <span className="small">
              {profileLoading ? (
                "Profil..."
              ) : profile ? (
                <span className="profile-name-inline">
                  {profile.firstName} {profile.lastName}
                  <VerificationBadge isVerified={profile.isVerified} />
                  <span>({rolesToText(roleFlags)})</span>
                </span>
              ) : (
                "Ucitavanje profila"
              )}
            </span>
            <button className="btn btn-sm btn-outline-light" type="button" onClick={() => void refreshProfile()}>
              Osvezi
            </button>
            <button className="btn btn-sm btn-light" type="button" onClick={logout}>
              Odjava
            </button>
          </div>
        }
      />

      <main className="content-wrap">
        <Flash flash={flash} onDismiss={() => setFlash(null)} />

        {driverVerificationPending && !isAdmin && (
          <div className="alert alert-warning approval-banner" role="status">
            <strong>Ceka se odobrenje vozackog profila.</strong>
            <span> Admin treba da odobri dokumenta pre nego sto mozete da objavljujete voznje i upravljate zahtevima kao vozac.</span>
          </div>
        )}

        <div className="page-card p-3 mb-3">
          <ul className="nav nav-pills gap-2">
            {tabs.map((tab) => (
              <li className="nav-item" key={tab.key}>
                <button className={`nav-link ${activeTab === tab.key ? "active" : ""}`} type="button" onClick={() => setActiveTab(tab.key)}>
                  {tab.label}
                  {tab.key === "notifications" && unreadNotifications > 0 && <span className="badge text-bg-danger ms-2">{unreadNotifications}</span>}
                </button>
              </li>
            ))}
          </ul>
        </div>

        {renderTab()}
      </main>

      <ProfileModal userId={profileModalUserId} onClose={() => setProfileModalUserId(null)} onOpenProfile={openProfile} />
    </div>
  );
}

export default App;

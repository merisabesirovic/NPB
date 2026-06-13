export function profileName(firstName?: string, lastName?: string, email?: string, fallback = "Profil") {
  const fullName = `${firstName || ""} ${lastName || ""}`.trim();
  return fullName || email || fallback;
}

export function VerificationBadge({ isVerified }: { isVerified?: boolean }) {
  if (!isVerified) return null;
  return <span className="verification-badge" aria-label="Verifikovan profil" title="Verifikovan profil" />;
}

export function ProfileLink({
  userId,
  firstName,
  lastName,
  email,
  isVerified,
  onOpen
}: {
  userId?: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  isVerified?: boolean;
  onOpen: (userId: string) => void;
}) {
  const content = (
    <>
      {profileName(firstName, lastName, email)}
      <VerificationBadge isVerified={isVerified} />
    </>
  );

  if (!userId) return <span className="profile-name-inline">{content}</span>;

  return (
    <button className="btn btn-link p-0 align-baseline profile-link profile-name-inline" type="button" onClick={() => onOpen(userId)}>
      {content}
    </button>
  );
}

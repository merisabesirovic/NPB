export type FlashType = "success" | "danger" | "warning" | "info";

export interface FlashMessage {
  type: FlashType;
  text: string;
}

export interface Vehicle {
  id: string;
  model: string;
  year: number;
  seatsCount: number;
  vehicleImageUrl?: string;
  licenseNumber?: string;
  registrationCertificateUrl?: string;
  isVerified?: boolean;
}

export interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  biography?: string;
  avatarUrl?: string;
  averageRating: number;
  trustScore: number;
  completedRidesCount: number;
  cancelledRidesCount: number;
  roles: number;
  isVerified?: boolean;
  identityVerificationPending?: boolean;
  driverVerificationPending?: boolean;
  driverVerificationApproved?: boolean;
  driverVerificationStatus?: string;
  vehicles?: Vehicle[];
}

export interface LoginResponse {
  token: string;
  tokenExpiresAt?: string;
  refreshToken?: string;
  refreshTokenExpiresAt?: string;
  driverVerificationPending?: boolean;
}

export interface Ride {
  id: string;
  driverId: string;
  driverEmail?: string;
  driverIsVerified?: boolean;
  startAddress: string;
  startLatitude: number;
  startLongitude: number;
  destinationAddress: string;
  destinationLatitude: number;
  destinationLongitude: number;
  departureDateTime: string;
  destinationDateTime: string;
  availableSeats: number;
  pricePerSeat: number;
  rideType: string;
  rideStatus: string;
  autoApproveBookings: boolean;
  smokingAllowed: boolean;
  petsAllowed: boolean;
  luggageAllowed: boolean;
  musicAllowed: boolean;
  conversationAllowed: boolean;
  createdAt: string;
}

export interface RideListResponse {
  totalCount: number;
  page: number;
  pageSize: number;
  items: Ride[];
  debugInfo?: {
    dbCandidates: number;
    validCandidates: number;
    availableRides: number;
    dateFilterApplied: boolean;
    priceFilterApplied: boolean;
  };
}

export interface Booking {
  id: string;
  rideId: string;
  rideDate?: string;
  passengerId: string;
  seatsReserved: number;
  pickupPoint: string;
  dropoffPoint: string;
  note?: string;
  bookingStatus: string;
  totalPrice: number;
  createdAt: string;
  paymentMethod?: string;
  paymentStatus?: string;
  paidAmount?: number;
  refundAmount?: number;
  driverId?: string;
  driverEmail?: string;
  driverFirstName?: string;
  driverLastName?: string;
  driverIsVerified?: boolean;
  passengerEmail?: string;
  passengerFirstName?: string;
  passengerLastName?: string;
  passengerIsVerified?: boolean;
  startAddress?: string;
  destinationAddress?: string;
  departureDateTime?: string;
  rideStatus?: string;
}

export interface NotificationItem {
  id: string;
  userId: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface Review {
  id: string;
  bookingId: string;
  reviewerId: string;
  reviewedUserId: string;
  reviewerEmail?: string;
  reviewerFirstName?: string;
  reviewerLastName?: string;
  reviewerIsVerified?: boolean;
  reviewedUserEmail?: string;
  reviewedUserFirstName?: string;
  reviewedUserLastName?: string;
  reviewedUserIsVerified?: boolean;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface Dispute {
  id: string;
  bookingId: string;
  createdByUserId: string;
  createdByEmail?: string;
  createdByFirstName?: string;
  createdByLastName?: string;
  createdByIsVerified?: boolean;
  startAddress?: string;
  destinationAddress?: string;
  description: string;
  status: string;
  resolution?: string;
  createdAt: string;
}

export interface DriverRequest {
  documentId?: string;
  id?: string;
  userId: string;
  userEmail?: string;
  email?: string;
  userFirstName?: string;
  userLastName?: string;
  documentType?: string | number;
  fileUrl: string;
  uploadedAt: string;
}

export interface VehicleVerificationRequest {
  vehicleId: string;
  userId: string;
  userEmail?: string;
  userFirstName?: string;
  userLastName?: string;
  model?: string;
  year?: number;
  seatsCount?: number;
  vehicleImageUrl?: string;
  licenseNumber?: string;
  registrationCertificateUrl?: string;
  isVerified?: boolean;
}

export const ROLE = {
  Driver: 1,
  Passenger: 2,
  Admin: 4
} as const;

export type TabKey =
  | "search"
  | "profile"
  | "createRide"
  | "myRides"
  | "bookings"
  | "notifications"
  | "disputes"
  | "admin";

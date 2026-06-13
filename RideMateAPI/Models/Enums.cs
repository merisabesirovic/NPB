using System;

namespace RideMateAPI.Models
{
    [Flags]
    public enum UserRole
    {
        None = 0,
        Driver = 1,
        Passenger = 2,
        Admin = 4
    }

    public enum RideType
    {
        OneTime,
        RecurringWeekdays,
        RecurringWeekend,
        LongTerm
    }

    public enum RideStatus
    {
        Scheduled,
        BookingOpen,
        BookingClosed,
        InProgress,
        Completed,
        Cancelled
    }

    public enum RecurringType
    {
        Daily,
        Weekdays,
        Weekends
    }

    public enum BookingStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Completed
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Refunded
    }

    public enum PaymentMethod
    {
        Cash,
        Online
    }

    public enum DisputeStatus
    {
        Open,
        InReview,
        Resolved,
        Rejected
    }

    public enum DocumentType
    {
        DriverLicense,
        RegistrationCertificate,
        IdentityCard,
        PassengerIdentityCard
    }

    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected
    }
}

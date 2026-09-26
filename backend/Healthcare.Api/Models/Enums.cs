namespace Healthcare.Api.Models;

public enum UserRole { SuperAdmin, Admin, Doctor, Patient }
public enum UserStatus { Pending, Active, Blocked, Rejected }
public enum DoctorApprovalStatus { Pending, Approved, Rejected }
public enum ScheduleStatus { Draft, Published, Closed }
public enum AppointmentStatus { Booked, Ongoing, Completed, Cancelled }
public enum PaymentStatus { Pending, Paid, Refunded, Failed }
public enum OtpPurpose { Registration, DoctorApplication, PasswordReset }

namespace STLMS.Application.Common.Interfaces;

public interface IEmailSender
{
    /// <summary>Returns false (never throws) when delivery didn't happen - e.g. SMTP not
    /// configured. Callers must treat "not sent" as a first-class outcome, not an exception, so a
    /// missing SMTP config never blocks the underlying action (registration, password reset,
    /// etc.) from completing.</summary>
    Task<bool> SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}

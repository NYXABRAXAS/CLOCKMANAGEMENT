using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using STLMS.Application.Common.Interfaces;
using STLMS.Domain.Entities;
using STLMS.Domain.Interfaces;

namespace STLMS.Infrastructure.ExternalServices.Push;

/// <summary>Sends real push notifications via Firebase Cloud Messaging - but, like SmtpEmailSender,
/// never touches the Firebase SDK at all (and never throws) when Firebase:ServiceAccountJsonPath
/// isn't configured, so the app runs fine without a Firebase project. This code is written and
/// wired end-to-end but cannot be verified with real device delivery on this machine - there's no
/// Firebase project or physical/browser device token available in this environment, the same
/// "written but unverified without credentials" situation as the Google/Microsoft OAuth providers.</summary>
public class FcmPushSender(IConfiguration configuration, IUnitOfWork uow, ILogger<FcmPushSender> logger) : IPushSender
{
    private static FirebaseApp? _app;
    private static readonly object AppLock = new();

    public async Task<bool> SendToUserAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        var credentialPath = configuration["Firebase:ServiceAccountJsonPath"];
        if (string.IsNullOrWhiteSpace(credentialPath) || !File.Exists(credentialPath))
        {
            logger.LogWarning("Push not sent (Firebase not configured): \"{Title}\" -> user {UserId}", title, userId);
            return false;
        }

        var devices = await uow.Repository<UserDevice>().FindAsync(d => d.UserId == userId, ct);
        if (devices.Count == 0) return false;

        try
        {
            var messaging = FirebaseMessaging.GetMessaging(GetOrCreateApp(credentialPath));
            var response = await messaging.SendEachForMulticastAsync(
                new MulticastMessage
                {
                    Tokens = devices.Select(d => d.FcmToken).ToList(),
                    Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = message },
                },
                ct);

            if (response.FailureCount > 0)
            {
                var failedTokens = devices
                    .Where((_, i) => !response.Responses[i].IsSuccess)
                    .ToList();
                foreach (var device in failedTokens)
                {
                    uow.Repository<UserDevice>().Remove(device);
                }
                if (failedTokens.Count > 0) await uow.SaveChangesAsync(ct);
            }

            return response.SuccessCount > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send push \"{Title}\" to user {UserId}", title, userId);
            return false;
        }
    }

    private static FirebaseApp GetOrCreateApp(string credentialPath)
    {
        if (_app is not null) return _app;
        lock (AppLock)
        {
            _app ??= FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(credentialPath) }, "STLMS");
            return _app;
        }
    }
}

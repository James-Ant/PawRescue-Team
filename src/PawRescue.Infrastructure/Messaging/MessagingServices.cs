using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PawRescue.Application.Interfaces;
using PawRescue.Application.Common;

namespace PawRescue.Infrastructure.Messaging;

/// <summary>Firebase Cloud Messaging — works for Android (and iOS through APNs set up in Firebase).</summary>
public class FcmPushNotificationSender : IPushNotificationSender
{
    private static readonly object InitLock = new();
    private readonly ILogger<FcmPushNotificationSender> _logger;

    public FcmPushNotificationSender(IOptions<FirebaseOptions> options, ILogger<FcmPushNotificationSender> logger)
    {
        _logger = logger;
        lock (InitLock)
        {
            if (FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(options.Value.CredentialsPath)
                });
            }
        }
    }

    public async Task SendAsync(string deviceToken, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new FirebaseAdmin.Messaging.Notification { Title = title, Body = body },
            Data = data is null ? null : new Dictionary<string, string>(data),
            Android = new AndroidConfig { Priority = Priority.High }
        };

        var id = await FirebaseMessaging.DefaultInstance.SendAsync(message, ct);
        _logger.LogDebug("FCM message sent: {MessageId}", id);
    }
}

/// <summary>Used when Firebase isn't configured: notifications are still saved in-app; push is only logged.</summary>
public class LoggingPushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<LoggingPushNotificationSender> _logger;
    public LoggingPushNotificationSender(ILogger<LoggingPushNotificationSender> logger) => _logger = logger;

    public Task SendAsync(string deviceToken, string title, string body, IDictionary<string, string>? data = null, CancellationToken ct = default)
    {
        _logger.LogInformation("[PUSH disabled] to {Token}: {Title} — {Body}", deviceToken[..Math.Min(10, deviceToken.Length)] + "…", title, body);
        return Task.CompletedTask;
    }
}

/// <summary>
/// PARTIAL: prints the email (with the reset code) to the console.
/// Replace with SMTP (MailKit) or SendGrid for production.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;
    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogWarning("[EMAIL - dev only] To: {To} | {Subject} | {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}

/// <summary>
/// UNFINISHED FEATURE: AI animal health detection.
/// Kept behind an interface so a real model (Azure Custom Vision / ML.NET / ONNX) can be plugged in later.
/// </summary>
public class NotImplementedHealthAnalyzer : IAnimalHealthAnalyzer
{
    public Task<HealthAnalysisResult> AnalyzeAsync(string imageUrl, CancellationToken ct = default)
        => throw new FeatureNotImplementedException("AI animal health detection");
}

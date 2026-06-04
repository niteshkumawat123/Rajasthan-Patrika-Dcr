using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System.Collections.Concurrent;

namespace DCRSupplyApp.Services;

public class FirebaseNotificationService
{
    private static bool _initialized = false;
    private static readonly ConcurrentDictionary<string, List<string>> _userTokens = new();

    public FirebaseNotificationService(IWebHostEnvironment env)
    {
        if (!_initialized)
        {
            var path = Path.Combine(env.ContentRootPath, "firebase-service-account.json");
            if (File.Exists(path))
            {
                try
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });
                    _initialized = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Firebase initialization failed: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Firebase service account file not found at: {path}");
            }
        }
    }

    /// <summary>
    /// Register a user's FCM token (call from client after login)
    /// </summary>
    public void RegisterToken(string empCode, string fcmToken)
    {
        var tokens = _userTokens.GetOrAdd(empCode, _ => new List<string>());
        if (!tokens.Contains(fcmToken))
            tokens.Add(fcmToken);
    }

    /// <summary>
    /// Remove a token (on logout)
    /// </summary>
    public void RemoveToken(string empCode, string fcmToken)
    {
        if (_userTokens.TryGetValue(empCode, out var tokens))
            tokens.Remove(fcmToken);
    }

    /// <summary>
    /// Send notification to a specific user by empCode
    /// </summary>
    public async Task<bool> SendToUserAsync(string empCode, string title, string body)
    {
        if (!_initialized) return false;
        if (!_userTokens.TryGetValue(empCode, out var tokens) || tokens.Count == 0)
            return false;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Webpush = new WebpushConfig
            {
                Notification = new WebpushNotification
                {
                    Icon = "/images/logo.png"
                }
            }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        return response.SuccessCount > 0;
    }

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    public async Task SendToUsersAsync(List<string> empCodes, string title, string body)
    {
        foreach (var empCode in empCodes)
        {
            await SendToUserAsync(empCode, title, body);
        }
    }

    /// <summary>
    /// Subscribe a token to a topic for role-based notifications
    /// </summary>
    public async Task SubscribeToTopicAsync(string token, string topic)
    {
        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(new List<string> { token }, topic);
            Console.WriteLine($"Subscribed to topic '{topic}': {response.SuccessCount} success, {response.FailureCount} failure");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error subscribing to topic: {ex.Message}");
        }
    }

    /// <summary>
    /// Send notification to all HO users (topic-based)
    /// </summary>
    public async Task SendToTopicAsync(string topic, string title, string body)
    {
        if (!_initialized) return;
        var message = new Message
        {
            Topic = topic,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }

    public async Task UnsubscribeFromTopicAsync(string fcmToken, string topic)
    {
        await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(new[] { fcmToken }, topic);
    }
}

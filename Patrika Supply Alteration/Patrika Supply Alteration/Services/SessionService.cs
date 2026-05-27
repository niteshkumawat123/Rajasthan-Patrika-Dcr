using System.Text.Json;
using DCRSupplyApp.Models;

namespace DCRSupplyApp.Services;

public class SessionService
{
    private const string UserKey = "DCR_User";

    public void SetUser(ISession session, UserSessionModel user)
    {
        var json = JsonSerializer.Serialize(user);
        session.SetString(UserKey, json);
    }

    public UserSessionModel? GetUser(ISession session)
    {
        var json = session.GetString(UserKey);
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<UserSessionModel>(json);
    }

    public void ClearUser(ISession session)
    {
        session.Clear();
    }
}

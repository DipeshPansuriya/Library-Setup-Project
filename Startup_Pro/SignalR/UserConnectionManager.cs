using Microsoft.AspNetCore.SignalR;


public class UserConnectionManager : IUserConnectionManager
{
    private static readonly Dictionary<string, List<string>> userConnectionMap = new Dictionary<string, List<string>>();
    private static readonly object lockObj = new object();
    private static IHubCallerClients _callerClients;

    public void KeepUserConnection(string orgProdId, string connectionId)
    {
        lock (lockObj)
        {
            if (!userConnectionMap.ContainsKey(orgProdId))
            {
                userConnectionMap[orgProdId] = new List<string>();
            }
            userConnectionMap[orgProdId].Add(connectionId);
        }
    }

    public void RemoveUserConnection(string orgProdId, string connectionId)
    {
        lock (lockObj)
        {
            if (userConnectionMap.ContainsKey(orgProdId))
            {
                _ = userConnectionMap[orgProdId].Remove(connectionId);
                if (!userConnectionMap[orgProdId].Any())
                {
                    _ = userConnectionMap.Remove(orgProdId);
                }
            }
        }
    }

    public List<string> GetUserConnections(string orgProdIds)
    {
        lock (lockObj)
        {
            List<string> connectionIds = new List<string>();
            if (!string.IsNullOrEmpty(orgProdIds))
            {
                foreach (string orgProdId in orgProdIds.Split(","))
                {
                    if (userConnectionMap.ContainsKey(orgProdId))
                    {
                        connectionIds.AddRange(userConnectionMap[orgProdId]);
                    }
                }
            }
            return connectionIds;
        }
    }
    public void SetHubClient(IHubCallerClients callerClients)
    {
        _callerClients = callerClients;
    }
    public IHubCallerClients GetHubClient()
    {
        return _callerClients;
    }
}


using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IUserConnectionManager
{
    void KeepUserConnection(string userId, string connectionId);
    void RemoveUserConnection(string userId, string connectionId);
    List<string> GetUserConnections(string userIds);
    void SetHubClient(IHubCallerClients _callerClients);
    IHubCallerClients GetHubClient();
}


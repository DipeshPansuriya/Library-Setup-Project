using KLSPL.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// This service is created to handle SignalR events
/// This service has been designed according to Gateway project only. If user wants to use this service directly in other project other than host porject(Gateway) this may perform unexpected
/// </summary>
public interface ISignalRMessageHub
{
    Task SendMessage(List<string> connectionIds, MessageResponseDataDto messageResponseDataDto);
    Task SendMessageToAll(MessageResponseDataDto messageResponseDataDto);
}
public class SignalRMessageHub : Hub, ISignalRMessageHub
{
    private readonly IUserConnectionManager _userConnectionManager;
    public SignalRMessageHub(IUserConnectionManager userConnectionManager)
    {
        _userConnectionManager = userConnectionManager;
    }
    /// <summary>
    /// This function called automatically when client send any connection request to the SignalR hub
    /// In this function User driven ConnectionId mantain to use further to send messages
    /// In this function also maintain HubCallerClient object to send SignalR message event to client when it needed. 
    /// Note: Object of HubCallerClient is created isoleted according to hosting projects
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        if (AppSettings.signalRSetting.IsEnabled == true)
        {
            string orgProdIdData = Context.GetHttpContext().Request.Query["OrgProdId"];
            string orgProdId = !string.IsNullOrEmpty(orgProdIdData) ? orgProdIdData : "0";
            if (orgProdId != "0") _userConnectionManager.KeepUserConnection(orgProdId, Context.ConnectionId);
            _userConnectionManager.SetHubClient(Clients);
            await base.OnConnectedAsync();
        }
    }

    /// <summary>
    /// This function called automatically when client send any Stop connection request to the SignalR hub
    /// In this function connection data removed for ConnectionId
    /// </summary>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception exception = null)
    {
        string orgProdIdData = Context.GetHttpContext().Request.Query["OrgProdId"];
        string orgProdId = !string.IsNullOrEmpty(orgProdIdData) ? orgProdIdData : "0";
        if (orgProdId != "0") _userConnectionManager.RemoveUserConnection(orgProdId, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    /// <summary>
    /// This function is desiged to send message to all connected clients according to given connection Ids
    /// This function has been designed according to Gateway project only. If user wants to use this function directly in other project other than host porject(Gateway) this may perform unexpected
    /// This function can be use with help of some cross project communication machenism(e.g RestClient request) from any project in the solution
    /// </summary>
    /// <param name="connectionIds"></param>
    /// <param name="messageResponseDataDto"></param>
    /// <returns></returns>
    public async Task SendMessage(List<string> connectionIds, MessageResponseDataDto messageResponseDataDto)
    {
        if (connectionIds?.Count > 0)
        {
            try
            {
                IHubCallerClients _callerClients = _userConnectionManager.GetHubClient();
                await _callerClients.Clients(connectionIds).SendAsync(messageResponseDataDto.SubscriptionEventName, messageResponseDataDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    /// <summary>
    /// This function is desiged to send message to all connected clients
    /// This function has been designed according to Gateway project only. If user wants to use this function directly in other project other than host porject(Gateway) this may perform unexpected
    /// This function can be use with help of some cross project communication machenism(e.g RestClient request) from any project in the solution
    /// </summary>
    /// <param name="messageResponseDataDto"></param>
    /// <returns></returns>
    public async Task SendMessageToAll(MessageResponseDataDto messageResponseDataDto)
    {
        try
        {
            IHubCallerClients _callerClients = _userConnectionManager.GetHubClient();
            await _callerClients.All.SendAsync(messageResponseDataDto.SubscriptionEventName, messageResponseDataDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    /// <summary>
    /// This a example method to brodcast from client side using function name(SendBookingRequestCreatedMessage)
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public async Task SendBookingRequestCreatedMessage(long jobId)
    {
        if (jobId > 0)
        {
            // EXP_JobId
            string orgProdId = "2054";
            List<string> connectionIds = _userConnectionManager.GetUserConnections(orgProdId);
            if (connectionIds.Count > 0)
            {
                IEnumerable<string> con = connectionIds.Where(x => x != Context.ConnectionId);
                string response = "Test Data";
                IHubCallerClients _callerClients = _userConnectionManager.GetHubClient();
                await _callerClients.Clients(con).SendAsync("BookingRequestCreatedEvent", new MessageResponseDataDto { ResponseData = response, SubscriptionEventName = "BookingRequestCreatedEvent", TriggeredEventName = "SendBookingRequestCreatedMessage" });
            }
        }
    }
}


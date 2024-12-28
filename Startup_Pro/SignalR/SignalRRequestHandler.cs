using KLSPL.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// This service is simple designed to send SignalR message event request to SignalRMessageHub in Host project(Gateway) with help of RestClient request
/// </summary>
public interface ISignalRRequestHandler
{
    void SendMessage(MessageResponseDataDto messageResponseDataDto);
    void SendMessageToAll(MessageResponseDataDto messageResponseDataDto);
}
public class SignalRRequestHandler : ISignalRRequestHandler
{
    public void SendMessage(MessageResponseDataDto messageResponseDataDto)
    {
        if (AppSettings.signalRSetting.IsEnabled == true && !string.IsNullOrEmpty(messageResponseDataDto.OrgProdIds))
        {
            RestClient rc = new()
            {
                ContentType = "application/json",
                URL = AppSettings.APIURL + "api/Generic/SendSignalRMessage",
                Method = HttpVerb.POST,
                PostData = JsonConvert.SerializeObject(messageResponseDataDto)
            };
            _ = rc.MakeRequest();

        }
    }
    public void SendMessageToAll(MessageResponseDataDto messageResponseDataDto)
    {
        if (AppSettings.signalRSetting.IsEnabled == true)
        {
            RestClient rc = new()
            {
                ContentType = "application/json",
                URL = AppSettings.APIURL + "api/Generic/SendSignalRMessageToAll",
                Method = HttpVerb.POST,
                PostData = JsonConvert.SerializeObject(messageResponseDataDto)
            };
            _ = rc.MakeRequest();
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.SignalR;

namespace Fathom.API.Services.SignalR;

/// <summary>
/// Responsible for ushering events to the UI and allowing simple DI hook to send data
/// </summary>
public interface IEventHub
{
    Task SendMessageAsync(string method, SignalRMessage message, bool onlyAdmins = true, CancellationToken ct = default);
    Task SendMessageToAsync(string method, SignalRMessage message, int userId, CancellationToken ct = default);
}

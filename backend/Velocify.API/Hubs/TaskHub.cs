using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Velocify.API.Hubs;

/// <summary>
/// SignalR hub for real-time task notifications and updates.
/// Handles connection lifecycle, authentication, and user group management.
/// 
/// AUTHENTICATION:
/// - Connections are authenticated using JWT tokens from the Authorization header or query string
/// - The [Authorize] attribute ensures only authenticated users can connect
/// - JWT claims are validated by the ASP.NET Core authentication middleware
/// 
/// USER GROUPS:
/// - Each user is added to a group identified by their UserId
/// - Groups enable targeted broadcasting (e.g., notify only the assigned user)
/// - Group membership is managed automatically on connect/disconnect
/// 
/// REQUIREMENTS:
/// - 6.5: Authenticate connections using JWT token
/// - 6.6: Add connections to user-specific groups
/// </summary>
[Authorize]
public class TaskHub : Hub
{
    /// <summary>
    /// Called when a client establishes a connection to the hub.
    /// Authenticates the user via JWT and adds the connection to a user-specific group.
    /// 
    /// AUTHENTICATION FLOW:
    /// 1. Client includes JWT in Authorization header or access_token query parameter
    /// 2. ASP.NET Core authentication middleware validates the JWT
    /// 3. If valid, Context.User is populated with claims
    /// 4. If invalid, connection is rejected before this method is called
    /// 
    /// GROUP MANAGEMENT:
    /// - UserId is extracted from JWT claims (Sub claim contains the user's GUID)
    /// - Connection is added to a group named with the UserId
    /// - Multiple connections from the same user join the same group
    /// - This enables broadcasting to all of a user's devices/tabs
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        // Extract UserId from JWT claims
        // The Sub (Subject) claim contains the user's unique identifier
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? Context.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            // This should never happen due to [Authorize] attribute
            // but we handle it defensively
            throw new HubException("User identity not found in token claims");
        }

        // Add connection to user-specific group
        // Group name is the UserId, enabling targeted notifications
        await Groups.AddToGroupAsync(Context.ConnectionId, userIdClaim);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Removes the connection from the user's group.
    /// 
    /// CLEANUP:
    /// - SignalR automatically removes the connection from all groups on disconnect
    /// - We call RemoveFromGroupAsync explicitly for clarity and logging purposes
    /// - If this is the user's last connection, the group becomes empty (no overhead)
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Extract UserId from JWT claims
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userIdClaim))
        {
            // Remove connection from user-specific group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userIdClaim);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

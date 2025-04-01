using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BonusesApp.Server.Configuration
{
    public static class OidcServerConfig
    {
        public const string ServerName = "BonusesApp API";
        public const string BonusesAppClientID = "bonusesapp_spa";
        public const string SwaggerClientID = "swagger_ui";

        public static async Task RegisterClientApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

            // Angular SPA Client
            if (await manager.FindByClientIdAsync(BonusesAppClientID) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = BonusesAppClientID,
                    ClientType = ClientTypes.Public,
                    DisplayName = "BonusesApp SPA",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Phone,
                        Permissions.Scopes.Address,
                        Permissions.Scopes.Roles
                    }
                });
            }

            // Swagger UI Client
            if (await manager.FindByClientIdAsync(SwaggerClientID) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = SwaggerClientID,
                    ClientType = ClientTypes.Public,
                    DisplayName = "Swagger UI",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.Password
                    }
                });
            }
        }
    }
}

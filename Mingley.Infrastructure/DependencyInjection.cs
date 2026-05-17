
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mingley.Application.Interfaces;
using Mingley.Infrastructure.Persistence;
using Mingley.Infrastructure.Services;

namespace Mingley.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MingleyDbContext>(opt =>
            opt.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("Mingley.Infrastructure")
            )
        );

        // Domain services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDiscoverService, DiscoverService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<ICallService, CallService>();
        services.AddScoped<ISuperChatService, SuperChatService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();

        // Agora RTC token generator (no external package — self-contained)
        services.AddScoped<AgoraTokenService>();

        return services;
    }
}

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Mingley.Application.Interfaces;
//using Mingley.Infrastructure.Persistence;
//using Mingley.Infrastructure.Services;

//namespace Mingley.Infrastructure;

//public static class DependencyInjection
//{
//    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
//    {
//        services.AddDbContext<MingleyDbContext>(opt =>
//            opt.UseNpgsql(
//                config.GetConnectionString("DefaultConnection"),
//                b => b.MigrationsAssembly("Mingley.Infrastructure")
//            )
//        );

//        // Domain services
//        services.AddScoped<ITokenService, TokenService>();
//        services.AddScoped<INotificationService, NotificationService>();
//        services.AddScoped<IAuthService, AuthService>();
//        services.AddScoped<IUserService, UserService>();
//        services.AddScoped<IDiscoverService, DiscoverService>();
//        services.AddScoped<IChatService, ChatService>();
//        services.AddScoped<ICallService, CallService>();
//        services.AddScoped<ISuperChatService, SuperChatService>();
//        services.AddScoped<IWalletService, WalletService>();
//        services.AddScoped<ISubscriptionService, SubscriptionService>();

//        // Agora RTC token generator (no external package — self-contained)
//        services.AddScoped<AgoraTokenService>();

//        return services;
//    }
//}

////using Microsoft.EntityFrameworkCore;
////using Microsoft.Extensions.Configuration;
////using Microsoft.Extensions.DependencyInjection;
////using Mingley.Application.Interfaces;
////using Mingley.Infrastructure.Persistence;
////using Mingley.Infrastructure.Services;

////namespace Mingley.Infrastructure;

////public static class DependencyInjection
////{
////    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
////    {
////        services.AddDbContext<MingleyDbContext>(opt =>
////            opt.UseNpgsql(
////                config.GetConnectionString("DefaultConnection"),
////                b => b.MigrationsAssembly("Mingley.Infrastructure")
////            )
////        );

////        services.AddScoped<ITokenService,        TokenService>();
////        services.AddScoped<INotificationService, NotificationService>();
////        services.AddScoped<IAuthService,         AuthService>();
////        services.AddScoped<IUserService,         UserService>();
////        services.AddScoped<IDiscoverService,     DiscoverService>();
////        services.AddScoped<IChatService,         ChatService>();
////        services.AddScoped<ICallService,         CallService>();
////        services.AddScoped<ISuperChatService,    SuperChatService>();
////        services.AddScoped<IWalletService,       WalletService>();
////        services.AddScoped<ISubscriptionService, SubscriptionService>();

////        return services;
////    }
////}

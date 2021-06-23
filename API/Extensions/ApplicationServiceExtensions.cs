using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using API.SignalR;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options=>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IPhotoService, PhotoService>();

            services.AddScoped<LogUserActivity>();

            services.AddSingleton<PresenceTracker>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

            services.Configure<CloudinarySettings>(
                config.GetSection("CloudinarySettings")
            );

            return services;

        }
    }
}
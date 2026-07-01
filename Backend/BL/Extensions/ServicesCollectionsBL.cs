using BL.Services.Abstraction;
using BL.Services.Implementation;
using DAL.Database;
using DAL.Entities;
using DAL.Repositories;
using DAL.Repositories.Abstraction;
using DAL.Repositories.Implementation;
using DAL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Extensions
{
    public static class ServicesCollectionsBL
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IResourceCheckService, ResourceCheckService>();
            services.AddScoped<IUserTopicMasteryService, UserTopicMasteryService>();
            services.AddScoped<IExamSessionService, ExamSessionService>();
            services.AddScoped<IUserDashboardService, UserDashboardService>();

            return services;
        }
    }
}

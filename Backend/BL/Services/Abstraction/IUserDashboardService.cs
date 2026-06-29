using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Abstraction
{
    public interface IUserDashboardService
    {
        Task<UserDashboardDto> GetDashboardAsync(string userId);
    }
}

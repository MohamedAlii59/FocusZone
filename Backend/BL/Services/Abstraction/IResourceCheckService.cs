using BL.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services.Abstraction
{
    public interface IResourceCheckService
    {
        Task<ResourceCheckResult> CheckAsync(string url, CancellationToken ct = default);
    }
}

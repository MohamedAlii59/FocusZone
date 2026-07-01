using System;
using System.Collections.Generic;
using System.Text;

namespace BL.DTOs
{
    public sealed class ResourceCheckResult
    {
        public required string Type { get; init; }
        public required bool CanEmbed { get; init; }
        public string? Url { get; init; }

        public static ResourceCheckResult Unknown() =>new() { Type = "unknown", CanEmbed = false, Url = null };
    }

}

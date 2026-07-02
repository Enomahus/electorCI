using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Configurations
{
    public class TokenConfiguration
    {
        public string ValidIssuer { get; set; } = "ElectorCI.Web";
        public string? Secret { get; set; }
        public double AccessTokenExpirationMinutes { get; set; } = 10;
        public double RefreshTokenExpirationMinutes { get; set; } = 60 * 24 * 2;
    }
}

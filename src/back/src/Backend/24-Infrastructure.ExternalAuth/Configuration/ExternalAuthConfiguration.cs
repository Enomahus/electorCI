using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.ExternalAuth.Configuration
{
    public class ExternalAuthConfiguration
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}

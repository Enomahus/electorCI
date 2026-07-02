using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Constants
{
    public static class AppConstants
    {
        public static readonly string SuperAdminRole = "SuperAdmin";
        public static readonly string OrganismAgentRole = "OrganismAgentRole";
        public static readonly string ElectorRole = "ElectorRole";

        // App links
        public const string ConfirmGuestRequestLink =
            "{0}/registration-request-confirm?token={1}&id={2}";
        public const string ConfirmPasswordResetLink = "{0}/reset-password?token={1}&email={2}";

        // Elector by polling station
        public const int MAX_ELECTORS_PER_STATION = 500;
    }
}

using Microsoft.Data.SqlClient;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;

namespace Infrastructure.Persistence.SQLServer
{
    public static class ConfigureTelemetryServices
    {
        public static void AddEntityFrameworkCoreTelemetry(
            EntityFrameworkInstrumentationOptions options
        )
        {
            options.SetDbStatementForText = true;
            options.EnrichWithIDbCommand = (activity, command) =>
            {
                foreach (var item in command.Parameters.OfType<SqlParameter>())
                {
                    activity.SetTag(item.ParameterName, item.SqlValue);
                }
            };
        }
    }
}

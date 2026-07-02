using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Tools.Logging;

[ExcludeFromCodeCoverage]
public class ActivitySourceLog : IDisposable
{
    public static ActivitySourceLog HTTP = new ActivitySourceLog("HTTP");
    public static ActivitySourceLog EF = new ActivitySourceLog("EF");
    public static ActivitySourceLog WEB = new ActivitySourceLog("WEB");
    public static ActivitySourceLog TOOLS = new ActivitySourceLog("TOOLS");
    public static ActivitySourceLog CONTROLLER = new ActivitySourceLog("CONTROLLER");
    public static ActivitySourceLog REPOSITORY = new ActivitySourceLog("REPOSITORY");
    public static ActivitySourceLog INFRASTRUCTURE = new ActivitySourceLog("INFRASTRUCTURE");
    public static ActivitySourceLog CQRS = new ActivitySourceLog("CQRS");

    private ActivitySource _activitySource;

    public ActivitySourceLog(string source)
    {
        _activitySource = new ActivitySource(source);
    }

    public string Name => _activitySource.Name;

    public ActivityLog Start([CallerFilePath] string filePath = "", [CallerMemberName] string method = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath).Split("\\").LastOrDefault();

        string message = $"[{_activitySource.Name}] {className}.{method}";

        return Start(message);
    }

    public ActivityLog Start(string message)
    {
        var activity = _activitySource.StartActivity(message);

        return new ActivityLog(activity);
    }


    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RmlUiNet;

namespace Client.UI.Desktop.RmlInterfaces;

internal class SystemInterfaceImple(ILogger<SystemInterfaceImple> logger) : SystemInterface
{
    public override bool LogMessage(LogType type, string message)
    {
        logger.Log(type switch
        {
            LogType.Always => LogLevel.Critical,
            LogType.Error => LogLevel.Error,
            LogType.Assert => LogLevel.Critical,
            LogType.Warning => LogLevel.Warning,
            LogType.Info => LogLevel.Information,
            LogType.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        }, "{message}", message);
        return true;
    }

    public override void SetClipboardText(string text)
    {
        logger.LogInformation("SetClipboardText called with text: {Text}", text);
    }

    public override string GetClipboardText()
    {
        logger.LogInformation("GetClipboardText called");
        return string.Empty;
    }

    public override void ActivateKeyboard(float caretX, float caretY, float lineHeight)
    {
        logger.LogInformation("ActivateKeyboard called at ({X}, {Y}) with line height {LineHeight}", caretX, caretY,
            lineHeight);
    }

    public override void DeactivateKeyboard()
    {
        logger.LogInformation("DeactivateKeyboard called");
    }

    private i64? _startTime;

    public override double ElapsedTime
    {
        get
        {
            _startTime ??= Stopwatch.GetTimestamp();
            var elapsed = Stopwatch.GetElapsedTime(_startTime.Value);
            return elapsed.TotalMilliseconds;
        }
    }
}
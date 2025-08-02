using System;
using Hexa.NET.ImGui;

namespace Client.Platform.ImGuiPlatform
{
    public interface IImGuiPlatform : IDisposable
    {
        void Init(ImGuiContextPtr context);
        void Update(float deltaSeconds);
        void BeginFrame();
        void EndFrame();
        void SetImGuiController(ImGuiController controller);
    }
}

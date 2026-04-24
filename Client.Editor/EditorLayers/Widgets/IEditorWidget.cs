namespace Client.Editor.EditorLayers.Widgets;

public interface IEditorWidget
{
    void Render();
    void RenderMenu();
}

public interface IEditorWidgetStatusLeft
{
    void RenderStatusLeft();
}

public interface IEditorWidgetStatusCenter
{
    void RenderStatusCenter();
}

public interface IEditorWidgetStatusRight
{
    void RenderStatusRight();
}
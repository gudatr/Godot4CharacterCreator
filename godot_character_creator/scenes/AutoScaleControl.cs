using Godot;
public partial class AutoScaleControl : Control
{
    public override void _Process(double delta)
    {
        var viewport = GetViewport().GetVisibleRect().Size;
        Scale = new Vector2(viewport.X / 1920, viewport.Y / 1080);
    }
}

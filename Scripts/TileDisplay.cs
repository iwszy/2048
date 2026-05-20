using Godot;

public partial class TileDisplay : Control
{
    public const float TileSize = 100f;

    private int _value;
    private ColorRect _background;
    private Label _label;

    public int Value => _value;

    public TileDisplay() {
        CustomMinimumSize = new Vector2(TileSize, TileSize);
    }

    public override void _Ready() {
        _background = new ColorRect();
        _background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_background);

        _label = new Label();
        _label.HorizontalAlignment = HorizontalAlignment.Center;
        _label.VerticalAlignment = VerticalAlignment.Center;
        _label.SetAnchorsPreset(LayoutPreset.FullRect);
        _label.AddThemeFontSizeOverride("font_size", 32);
        AddChild(_label);

        SetValue(0);
    }

    public void SetValue(int value) {
        _value = value;
        _label.Text = value == 0 ? "" : value.ToString();
        _background.Color = GetTileColor(value);
        _label.Modulate = value <= 4 ? new Color("3C3A32") : Colors.White;

        // Adjust font size for large numbers
        if (value >= 1000) {
            _label.AddThemeFontSizeOverride("font_size", 22);
        } else if (value >= 100) {
            _label.AddThemeFontSizeOverride("font_size", 28);
        } else {
            _label.AddThemeFontSizeOverride("font_size", 32);
        }
    }

    public void PlaySpawnAnimation() {
        Scale = Vector2.Zero;
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(this, "scale", Vector2.One, 0.2f);
    }

    private static Color GetTileColor(int value) => value switch {
        0 => new Color("CDC1B4"),
        2 => new Color("EEE4DA"),
        4 => new Color("EDE0C8"),
        8 => new Color("F2B179"),
        16 => new Color("F59563"),
        32 => new Color("F67C5F"),
        64 => new Color("F65E3B"),
        128 => new Color("EDCF72"),
        256 => new Color("EDCC61"),
        512 => new Color("EDC850"),
        1024 => new Color("EDC53F"),
        2048 => new Color("EDC22E"),
        _ => new Color("3C3A32"),
    };
}
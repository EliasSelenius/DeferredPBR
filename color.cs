
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct color {
    public float red, green, blue, alpha;

    public color(float rgb) : this(rgb, 1f) {}
    public color(float rgb, float a) => (red, green, blue, alpha) = (rgb, rgb, rgb, a);
    public color(float r, float g, float b) => (red, green, blue, alpha) = (r, g, b, 1f);
    public color(float r, float g, float b, float a) => (red, green, blue, alpha) = (r, g, b, a);

    public color withAlpha(float a) => new color(red, green, blue, a);

    public static color rgb(float r, float g, float b) => new color(r, g, b);
    public static color rgba(float r, float g, float b, float a) => new color(r, g, b, a);
    //         int f = 0xffffffff;
    public static color hex(uint rgba) => 
        new color(
            ((rgba & 0xff000000) >> 24) / 255f,
            ((rgba & 0x00ff0000) >> 16) / 255f,
            ((rgba & 0x0000ff00) >> 8) / 255f,
            (rgba & 0x000000ff) / 255f);

    



    public static implicit operator color(float rgb) => new color(rgb);
    public static implicit operator color((float, float, float) t) => new color(t.Item1, t.Item2, t.Item3);
    public static implicit operator color((float, float, float, float) t) => new color(t.Item1, t.Item2, t.Item3, t.Item4);
    
    public static color invert(color c) => new color(1f - c.red, 1f - c.green, 1f - c.blue);

    public override string ToString() => $"({red}, {green}, {blue}, {alpha})";

    #region colors

    public static readonly color white = new color(1f);
    public static readonly color silver = new color(0.75f);
    public static readonly color gray = new color(0.5f);
    public static readonly color black = new color(0f);


    #endregion

}

class ColorPalette {
}
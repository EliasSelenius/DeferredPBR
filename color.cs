

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct color {
    public float red, green, blue, alpha;

    public color(float rgb) : this(rgb, 1f) {}
    public color(float rgb, float a) => (red, green, blue, alpha) = (rgb, rgb, rgb, a);
    public color(float r, float g, float b) => (red, green, blue, alpha) = (r, g, b, 1f);
    public color(float r, float g, float b, float a) => (red, green, blue, alpha) = (r, g, b, a);


    public static color rgb(float r, float g, float b) => new color(r, g, b);


    #region colors

    public static readonly color white = new color(1f);
    public static readonly color silver = new color(0.75f);
    public static readonly color gray = new color(0.5f);
    public static readonly color black = new color(0f);


    #endregion

}
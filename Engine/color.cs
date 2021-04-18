using System.Runtime.InteropServices;

namespace Engine {


    [StructLayout(LayoutKind.Sequential)]
    public struct color {
        public float r, g, b, a;

        public color(float rgb) : this(rgb, 1f) {}
        public color(float rgb, float _a) => (r, g, b, a) = (rgb, rgb, rgb, _a);
        public color(float _r, float _g, float _b) => (r, g, b, a) = (_r, _g, _b, 1f);
        public color(float _r, float _g, float _b, float _a) => (r, g, b, a) = (_r, _g, _b, _a);

        public color withAlpha(float a) => new color(r, g, b, a);

        public static color rgb(float r, float g, float b) => new color(r, g, b);
        public static color rgba(float r, float g, float b, float a) => new color(r, g, b, a);
        //         int f = 0xffffffff;
        public static color hex(uint rgba) => 
            new color(
                ((rgba & 0xff000000) >> 24) / 255f,
                ((rgba & 0x00ff0000) >> 16) / 255f,
                ((rgba & 0x0000ff00) >> 8) / 255f,
                (rgba & 0x000000ff) / 255f);

        
        public static color operator *(in color c, float s) => new color(c.r * s, c.g * s, c.b * s, c.a);


        public static implicit operator color(float rgb) => new color(rgb);
        public static implicit operator color((float, float, float) t) => new color(t.Item1, t.Item2, t.Item3);
        public static implicit operator color((float, float, float, float) t) => new color(t.Item1, t.Item2, t.Item3, t.Item4);
        
        public static color invert(color c) => new color(1f - c.r, 1f - c.g, 1f - c.b);

        public override string ToString() => $"({r}, {g}, {b}, {a})";

        public static void color2vec(in color color, out Nums.vec4 v) => v = new Nums.vec4(color.r, color.g, color.b, color.a);

        #region colors

        public static readonly color white = new color(1f);
        public static readonly color silver = new color(0.75f);
        public static readonly color gray = new color(0.5f);
        public static readonly color black = new color(0f);

        public static readonly color red = new color(1, 0, 0);
        public static readonly color green = new color(0, 1, 0);
        public static readonly color blue = new color(0, 0, 1);
        public static readonly color yellow = new color(1, 1, 0);
        public static readonly color purple = new color(1, 0, 1);


        #endregion

    }

    public class ColorPalette {
    }

    public class ColorTheme {

        // var c = color.hex(0x004156AF);

        public static readonly ColorTheme darkGreenish = new ColorTheme {
            primaryColor = color.hex(0x84A98CFF),
            backgroundColor = color.hex(0x52796FDF),
            textColor = color.hex(0xCAD2C5FF)
        };


        public color primaryColor { get; init; }
        public color backgroundColor { get; init; }
        public color textColor { get; init; }
        
    }
}
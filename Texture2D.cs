using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct color {
    public float red, green, blue, alpha;

    public color(float rgb) : this(rgb, 1f) {}
    public color(float rgb, float a) => (red, green, blue, alpha) = (rgb, rgb, rgb, a);
    public color(float r, float g, float b, float a) => (red, green, blue, alpha) = (r, g, b, a);

}

public class Texture2D {
    int id;

    public int width { get; private set; }
    public int height { get; private set; }
    public color[,] pixels { get; private set; }
    public readonly WrapMode wrapMode = WrapMode.Repeat;
    public readonly Filter filter = Filter.Linear;
    public bool genMipmap = true;

    public Texture2D(WrapMode wmode, Filter f, int w, int h) : this(wmode, f, new color[w,h]) { }
    public Texture2D(WrapMode wmode, Filter f, color[,] pixels) {
        (width, height) = (pixels.GetLength(0), pixels.GetLength(1));         
        this.pixels = pixels;
        wrapMode = wmode;
        filter = f;
        id = GLUtils.createTexture2D(this.pixels, PixelInternalFormat.Rgba, wrapMode, filter, genMipmap);
    }

    public void bind(TextureUnit unit) => GLUtils.bindTex2D(unit, id);

    public void resize(int w, int h) {
        (width, height) = (w, h);
        pixels = new color[width, height];
    }

    public static Texture2D fromFile(string filename) {
        return new Texture2D(WrapMode.Repeat, Filter.Nearest, Utils.bitmapToColorArray(new System.Drawing.Bitmap(System.Drawing.Image.FromFile(filename))));
    }

    public void applyChanges() {
        GL.BindTexture(TextureTarget.Texture2D, id);
        GLUtils.applyTextureData(PixelInternalFormat.Rgba, pixels);
        if (genMipmap) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
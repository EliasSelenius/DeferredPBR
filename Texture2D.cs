using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
struct color {
    public float red, green, blue, alpha;
}

class Texture2D {
    int id;

    public color[,] pixels;
    public WrapMode wrapMode = WrapMode.Repeat;
    public Filter filter = Filter.Linear;

    public Texture2D(color[,] pixels) {
        this.pixels = pixels;
        id = GLUtils.createTexture2D(this.pixels, PixelInternalFormat.Rgba, wrapMode, filter, true);
    }

    public void applyChanges() {
        GL.BindTexture(TextureTarget.Texture2D, id);
        GLUtils.applyTextureData(PixelInternalFormat.Rgba, pixels);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }
}
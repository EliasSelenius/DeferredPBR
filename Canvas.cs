
using Nums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

abstract class Canvas {

    public static Canvas activeCanvas;

    protected static int width => Renderer.windowWidth;
    protected static int height => Renderer.windowHeight;
    
    protected static readonly vec2 pivotTopLeft = new vec2(1, 1); 
    protected static readonly vec2 pivotCenter = new vec2(1, 1); 

    private static readonly Dictionary<string, Textbox> cachedTextMeshes = new Dictionary<string, Textbox>();

    static Mesh<posUvVertex> rectMesh;

    static Canvas() {
        activeCanvas = new MenueCanvas();
        rectMesh = MeshFactory<posUvVertex>.genQuad();
    }



    public void rect(vec2 pos, vec2 size, color color, vec2? pivot = null) {
        pos -= new vec2(width, height) / 2f;
        pos += (size * (pivot ?? vec2.one)) / 2f;
        pos *= (1, -1);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref size.x);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
        GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.red);
        rectMesh.render();
    }
    public void text(Textbox tb, vec2 pos, int fontSize, color color) {
        
        pos -= new vec2(width, height) / 2f;
        pos *= (1, -1);

        var one = vec2.one * fontSize;
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref one.x);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
        GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.red);
        tb.render();
    }

    public void text(string text, vec2 pos, int fontSize, color color) {
        if (!cachedTextMeshes.ContainsKey(text)) cachedTextMeshes.Add(text, new Textbox(text));
        
        this.text(cachedTextMeshes[text], pos, fontSize, color);
    }


    //public bool isHovering() 

    
    public abstract void render();

}

class MenueCanvas : Canvas {

    Textbox fps = new Textbox();

    public override void render() {

        rect(0, (width, height / 15f), color.gray); 

        
        fps.setText("Time: " + (Renderer.time * 1000.0).ToString("##.#") + "ms, fps: " + Renderer.fps.ToString());
        text(fps, 0, 16, color.white);

        
    }
}



enum Flow {
    block,
    inline
}

class BoxTransform {

    public vec2 pixelSize = vec2.one;
    public float pixelWidth => pixelSize.x;
    public float pixelHeight => pixelSize.y;

    public float totalWidth => marginLeft + pixelWidth + marginRight;
    public float totalHeight => marginTop + pixelHeight + marginBottom;

    public vec4 margin;
    public float marginTop => margin.x;
    public float marginBottom => margin.y;
    public float marginLeft => margin.z;
    public float marginRight => margin.w;

    public vec4 padding;
    public float paddingTop => padding.x;
    public float paddingBottom => padding.y;
    public float paddingLeft => padding.z;
    public float paddingRight => padding.w;
    

}


public enum Origin {
    center,
    top,
    bottom,
    left,
    right,
    topLeft,
    topRight,
    bottomLeft,
    bottomRight
}


using Nums;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

abstract class Canvas {

    public static Canvas activeCanvas;

    static readonly Dictionary<string, Textbox> cachedTextMeshes = new Dictionary<string, Textbox>();
    static Mesh<posUvVertex> rectMesh;
    static Canvas() {
        activeCanvas = new MenueCanvas();
        rectMesh = MeshFactory<posUvVertex>.genQuad();
    }


    protected vec2 canvasSize => new vec2(Renderer.windowWidth, Renderer.windowHeight);
    protected int canvasWidth => Renderer.windowWidth;
    protected int canvasHeight => Renderer.windowHeight;
    protected vec2 size => boxStack.TryPeek(out box b) ? b.size : canvasSize;
    protected float width => size.x;
    protected float height => size.y;

    private readonly Stack<box> boxStack = new Stack<box>(); 
    struct box {
        public vec2 pos;
        public vec2 size;
    }

    #region start ... end 
    
    public void start(vec2 pos, vec2 size) {

        var prevPos = boxStack.TryPeek(out box p) ? p.pos : vec2.zero;

        boxStack.Push(new box {
            pos = prevPos + pos,
            size = size
        });
    }

    public void fill(color c) {
        var b = boxStack.Peek();  
        rect(b.pos, b.size, c);
    }

    public void text(string text, int fontSize, color c) {
        var b = boxStack.Peek();
        this.text(text, b.pos, fontSize, c);
    }

    public void end() {
        boxStack.Pop();
    }

    #endregion


    public void rect(vec2 pos, vec2 size, color color, vec2? pivot = null) {
        pos -= canvasSize / 2f;
        pos += (size * (pivot ?? vec2.one)) / 2f;
        pos *= (1, -1);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref size.x);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
        GL.Uniform4(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.red);
        Renderer.whiteTexture.bind(TextureUnit.Texture0);
        rectMesh.render();
    }
    public void text(Textbox tb, vec2 pos, int fontSize, color color) {
        
        pos -= canvasSize / 2f;
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


    public bool clicking() {
        var o = boxStack.TryPeek(out box p) ? p.pos : vec2.zero;
        var c = o + size;
        return app.window.IsMouseButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)
            && Mouse.position.x < c.x && Mouse.position.x > o.x && Mouse.position.y < c.y && Mouse.position.y > o.y && Mouse.state == MouseState.free;
    } 

    
    public abstract void render();

}



class MenueCanvas : Canvas {

    Textbox fps = new Textbox();
    Textbox mousePosText = new Textbox();

    bool chk_state = false;

    public override void render() {

        var alt = app.window.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt);        
        Mouse.state = alt ? MouseState.free : MouseState.disabled;


        //rect(0, (width, height / 15f), color.gray); 
        start(100, 1000);
        var cs = new[] { color.gray, color.white };
        for (int i = 1; i <= 10; i++) {
            start(20, size - 40);
            fill(cs[i % 2]);
            text("Hello, World!", 20, cs[(i + 1) % 2]);
        }
        for (int i = 0; i < 10; i++) end();
        end();

        start(300, size - 600);

        if (button(10, 30)) {
            fill(color.gray);
        }
        text("Penis", 100, color.gray);

        checkbox(100, 20, ref chk_state);

        end();
        
        text("cursorgrab: " + app.window.CursorGrabbed, (0, 32), 16, color.white);

        
        fps.setText("Time: " + (Renderer.time * 1000.0).ToString("##.#") + "ms, fps: " + Renderer.fps.ToString());
        text(fps, 0, 16, color.white);

        
        mousePosText.setText(Mouse.position.ToString());
        text(mousePosText, (0, 16), 16, color.white);

        

    }

    void checkbox(vec2 pos, vec2 size, ref bool state) {
        start(pos, size);
        if (state) fill(color.white);
        else fill(color.gray);
        
        if (clicking()) state = !state;
        end();
    }

    bool button(vec2 pos, vec2 size) {
        start(pos, size);
        var res = clicking();
        fill(color.white);
        end();
        return res;
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

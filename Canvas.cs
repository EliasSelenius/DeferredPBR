
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
    protected vec2 size => currentBox.size;
    protected float width => size.x;
    protected float height => size.y;

    
    BoxModel rootBox = new BoxModel { pos = vec2.zero, size = new vec2(Renderer.windowWidth, Renderer.windowHeight) };
    protected BoxModel currentBox => boxStack.TryPeek(out BoxModel b) ? b : rootBox;
    readonly Stack<BoxModel> boxStack = new Stack<BoxModel>(); 
    public class BoxModel {
        public vec2 pos;
        public vec2 size;
        public vec2 displacement;
    }

    #region start ... end 
    
    public void start(vec2 pos, vec2 size) {

        boxStack.Push(new BoxModel {
            pos = currentBox.pos + pos,
            size = size
        });
    }

    public void start(vec2 size) {
        var box = currentBox;

        if (box.displacement.x + size.x > box.size.x) {
            box.displacement.x = 0;
            box.displacement.y += size.y;
        } 
        start(box.displacement, size);
        box.displacement.x += size.x;
    }

    public void fill(color c) {
        var b = currentBox;  
        rect(b.pos, b.size, c);
    }

    public void text(string text, int fontSize, color c) {
        var b = currentBox;
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
        var o = boxStack.TryPeek(out BoxModel p) ? p.pos : vec2.zero;
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

        var colors = new[] { color.black, color.silver, color.rgb(1, 0, 0), color.rgb(0,1,1), color.rgb(0,1,0) };
        start(150, size - 300);
        //fill (color.gray);
        for (int i = 0; i < 100; i++) {
            start(size / 10f);
            var alpha = (Mouse.position - currentBox.pos).length / 1000f;
            fill(color.rgba(1,1,1,alpha));
            text("" + alpha, 22, color.rgb(.9f, .4f, .1f));
            end();
        }
        end();



        //rect(0, (width, height / 15f), color.gray); 
        
        text("cursorgrab: " + app.window.CursorGrabbed, (0, 32), 16, color.white);

        
        fps.setText("Time: " + (Renderer.deltaTime * 1000.0).ToString("##.#") + "ms, fps: " + Renderer.fps.ToString());
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

    bool button(string t, vec2 pos, vec2 size, color c) {
        start(pos, size);
        var res = clicking();
        fill(c);
        text(t, (int)size.y + 1, color.invert(c));
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

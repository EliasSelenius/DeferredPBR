
using Nums;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

/*

    Textbox class:
        generate text mesh from string
        render using font

*/

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


public class Textbox {
    private Font font;
    private readonly Mesh<posUvVertex> mesh = new Mesh<posUvVertex>();
    private float scale = 1.0f;

    static string[] words = new[] {
        "graphics", "hello", "world", "universe", "is", "four", "music", "programing", "random", "that", 
        "cool", "red", "green", "blue", "terminator", "beat", "artist", "tool", "internett", "openGL", "basic", "advanced", "whiskey", "antilope", 
        "artificial", "intelligence", "movie", "camera", "game", "object", "wheat", "farm", "car"
    };

    static string genSentence() {
        int numWords = 10;
        string res = "";
        for (int i = 0; i < numWords; i++) {
            res += math.pick(words) + " ";
        }
        return res;
    }

    public Textbox() {
        font = Font.arial;

        //addChar(font.getGlyph('E'), 0);
        setText(genSentence());
        mesh.genNormals();
        mesh.bufferdata();
    }

    private void setText(string text) {
        vec2 pos = vec2.zero;
        for(int i = 0; i < text.Length; i++) {
            var g = font.getGlyph(text[i]);
            addChar(g, i, pos);
            pos.x += g.advance;
        }
    }

    private void addChar(Font.Glyph glyph, int charIndex, vec2 offset) {
        void addv(vec2 pos, vec2 uv) {
            pos += offset;
            mesh.vertices.Add(new posUvVertex {
                position = new vec3(pos.x, pos.y, 0) * scale,
                uv = uv 
            });
        }

        var atlasSize = new vec2(font.atlas.width, font.atlas.height);

        var p = glyph.pos / atlasSize;
        var n = glyph.size / atlasSize;
        var pn = p + n;

        addv((glyph.offset + (0, glyph.size.y)) * new vec2(1, -1), (pn.y, p.x));
        addv((glyph.offset + glyph.size) * new vec2(1, -1), pn.yx);
        addv(glyph.offset * new vec2(1, -1), p.yx);
        addv((glyph.offset + (glyph.size.x, 0)) * new vec2(1, -1), (p.y, pn.x));


        uint i = (uint)charIndex * 4;

        mesh.addTriangles(new uint[] {
            i + 0, i + 1, i + 2,
            i + 1, i + 3, i + 2
        });
    }

    public void render() {
        //font.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
        mesh.render();
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


/*
    first iteration of ui rendering
    every element is an inline-block aligned to the left
*/

abstract class GuiLayer {
    public void rect(float x, float y, float w, float h) {}
    public void text(Textbox tb) {}
    //public string textInput() {}
    
}

class Canvas {
 
    static Mesh<posUvVertex> boxMesh;
    static Textbox text;

    static Canvas() {
        boxMesh = MeshFactory<posUvVertex>.genQuad();
        text = new Textbox();
    }

    static List<BoxTransform> boxes = new List<BoxTransform> {
        new BoxTransform {
            pixelSize = (500f, 100f),
            margin = 10
        },
        new BoxTransform {
            pixelSize = (600, 100),
            margin = 10
        },
        new BoxTransform {
            pixelSize = (600, 100),
            margin = 10
        },
        new BoxTransform {
            pixelSize = (100, 100),
            margin = 100
        },
        new BoxTransform {
            pixelSize = (600, 100),
            margin = 100
        }

    };
 
 
    public static void render() {
        float currentWidth = 0;
        float currentHeight = 0;
        float maxHeight = 0;

        Renderer.whiteTexture.bind(TextureUnit.Texture0);
        for (int i = 0; i < boxes.Count; i++) {
            var box = boxes[i];
            

            if (currentWidth + box.totalWidth > Renderer.windowWidth) {
                currentWidth = 0;
                currentHeight += maxHeight;
                maxHeight = 0;
            }

            maxHeight = math.max(maxHeight, box.totalHeight);

            var pos = new vec2(currentWidth, currentHeight) + (box.pixelSize/2f) + box.margin.zx;
     
            currentWidth += box.totalWidth;

            pos -= new vec2(Renderer.windowWidth, Renderer.windowHeight) / 2f;
            drawRect(box.pixelSize, pos * (1, -1), vec3.one, Origin.center); 

            drawRect((box.totalWidth, box.marginTop), (pos - (0, (box.pixelHeight + box.marginTop)/2f)) * (1, -1), (0.5f, 0.1f, 0.05f), Origin.center); 

        }
    }

    static void drawRect(vec2 size, vec2 pos, vec3 color, Origin origin) {
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "size"), 1, ref size.x);
        GL.Uniform2(GL.GetUniformLocation(Renderer.textShader.id, "pos"), 1, ref pos.x);
        GL.Uniform3(GL.GetUniformLocation(Renderer.textShader.id, "color"), 1, ref color.x);
        boxMesh.render();
        //text.render();
    }
}
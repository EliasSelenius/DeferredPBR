
using Nums;

/*

    Textbox class:
        generate text mesh from string


*/



public class Textbox {
    private Font font;
    private readonly Mesh<posUvVertex> mesh = new Mesh<posUvVertex>();
    private float scale = 3f;

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
        font.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
        mesh.render();
    }
}
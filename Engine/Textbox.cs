
using Nums;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

/*

    Textbox class:
        generate text mesh from string
        render using font

*/

namespace Engine {



    public static class Text {

        internal static float length(string text, int length, int fontsize, Font font) {
            float accl = 0;
            for (int i = 0; i < length; i++) {
                var glyph = font.getGlyph(text[i]);
                accl += glyph.advance;
            }
            return accl / font.lineHeight * fontsize;
        }

        internal static void genText(string text, vec2 textOffset, int fontSize, in color color, Font font, Meshdata<Gui.textVertex> meshdata) {

            uint initialVertCount = (uint)meshdata.vertices.Count;
            color.color2vec(color, out vec4 vertColor);

            vec2 pos = vec2.zero;
            for(int i = 0; i < text.Length; i++) {
                var g = font.getGlyph(text[i]);
                addChar(g, i, pos);
                pos.x += g.advance;// * fontSize;
            }
            //meshdata.genNormals();

            void addChar(Font.Glyph glyph, int charIndex, vec2 offset) {
                void addv(vec2 pos, vec2 uv) {
                    pos += offset;
                    pos /= font.lineHeight;
                    
                    pos *= fontSize;
                    pos += textOffset;

                    meshdata.vertices.Add(new Gui.textVertex {
                        position = new vec2(pos.x, pos.y),
                        uv = uv,
                        color = vertColor
                    });
                }

                var atlasSize = new vec2(font.atlas.width, font.atlas.height);

                var p = glyph.pos / atlasSize;
                var n = glyph.size / atlasSize;
                var pn = p + n;

                var nSize = glyph.size / font.lineHeight;
                var nOffset = glyph.offset / font.lineHeight;

                addv((glyph.offset + (0, glyph.size.y)) * new vec2(1, -1), (pn.y, p.x));
                addv((glyph.offset + glyph.size)        * new vec2(1, -1),  pn.yx);
                addv(glyph.offset                       * new vec2(1, -1),  p.yx);
                addv((glyph.offset + (glyph.size.x, 0)) * new vec2(1, -1), (p.y, pn.x));


                uint i = initialVertCount + (uint)charIndex * 4;

                meshdata.addTriangles(new uint[] {
                    i + 0, i + 1, i + 2,
                    i + 1, i + 3, i + 2
                });
            }
        }

    }

    public class Textbox {
        private Font font;
        private readonly Mesh<posUvVertex> mesh = new Mesh<posUvVertex>();

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
        }

        public Textbox(string text) : this() {
            setText(text);
        }

        public void setText(string text) {
            mesh.data.clear();
            vec2 pos = vec2.zero;
            for(int i = 0; i < text.Length; i++) {
                var g = font.getGlyph(text[i]);
                addChar(g, i, pos);
                pos.x += g.advance;
            }
            mesh.data.genNormals();
            mesh.updateBuffers();
        }

        private void addChar(Font.Glyph glyph, int charIndex, vec2 offset) {
            void addv(vec2 pos, vec2 uv) {
                pos += offset;
                pos /= font.lineHeight;
                mesh.data.vertices.Add(new posUvVertex {
                    position = new vec3(pos.x, pos.y, 0),
                    uv = uv 
                });
            }

            var atlasSize = new vec2(font.atlas.width, font.atlas.height);

            var p = glyph.pos / atlasSize;
            var n = glyph.size / atlasSize;
            var pn = p + n;

            var nPos = glyph.pos / font.lineHeight;
            var nSize = glyph.size / font.lineHeight;
            var nOffset = glyph.offset / font.lineHeight;

            addv((glyph.offset + (0, glyph.size.y)) * new vec2(1, -1), (pn.y, p.x));
            addv((glyph.offset + glyph.size) * new vec2(1, -1), pn.yx);
            addv(glyph.offset * new vec2(1, -1), p.yx);
            addv((glyph.offset + (glyph.size.x, 0)) * new vec2(1, -1), (p.y, pn.x));


            uint i = (uint)charIndex * 4;

            mesh.data.addTriangles(new uint[] {
                i + 0, i + 1, i + 2,
                i + 1, i + 3, i + 2
            });
        }

        public void render() {
            font.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
            mesh.render();
        }
    }
}


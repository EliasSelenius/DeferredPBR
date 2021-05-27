
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nums;

namespace Engine.Gui {
    public class Font {

        
        public static readonly Font arial = Assets.getFont("Engine.data.fonts.arial.fnt");
        public static readonly Font monospaced = Assets.getFont("Engine.data.fonts.monospaced.fnt");

        public readonly Texture2D atlas;
        public vec2 atlasSize => new vec2(atlas.width, atlas.height);

        public readonly List<Glyph> glyphs = new List<Glyph>();

        public int paddingTop { get; private set; }
        public int paddingBottom { get; private set; }
        public int paddingLeft { get; private set; }
        public int paddingRight { get; private set; }

        public int lineHeight { get; private set; }
        public int baseLine { get; private set; }

        public class Glyph {
            public readonly Font font;
            public readonly int id;
            public readonly float advance;

            public readonly vec2 v0, v1, v2, v3;
            public readonly vec2 uv0, uv1, uv2, uv3;

            public Glyph(Font font, int id, vec2 pos, vec2 size, vec2 offset, float advance) {
                this.font = font;
                this.id = id;
                this.advance = advance / font.lineHeight;

                // compute vertex attribute values

                // uvs
                var p = pos / font.atlasSize;
                var n = size / font.atlasSize;
                var pn = p + n;
                uv0 = (pn.y, p.x);
                uv1 = pn.yx;
                uv2 = p.yx;
                uv3 = (p.y, pn.x);

                // pos
                vec2 invy = new vec2(1, -1);
                v0 = invy * (offset + (0, size.y)) / font.lineHeight;
                v1 = invy * (offset + size)        / font.lineHeight;
                v2 = invy * offset                 / font.lineHeight;
                v3 = invy * (offset + (size.x, 0)) / font.lineHeight;
            }
        }

        public Font(string fontdata, Texture2D atlas) : this(fontdata.Split('\n', '\r'), atlas) {}
        public Font(string[] fontData, Texture2D atlas) {
            this.atlas = atlas;
            this.loadFontData(fontData);
        }

        private void loadFontData(string[] fontdata) {
            var values = new Dictionary<string, string>();

            fontdata = fontdata.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            int _getInt(string key) => int.Parse(values[key]);
            int[] _getArray(string key) => values[key].Split(',').Select(x => int.Parse(x)).ToArray();

            int lineIndex = 0;
            bool _next() {
                values.Clear();

                if (lineIndex >= fontdata.Length)
                    return false;

                var line = fontdata[lineIndex];

                // end if line is empty string:
                if (string.IsNullOrEmpty(line))
                    return false;

                foreach (var item in line.Split(' ')) {
                    var s = item.Split('=');
                    if (s.Length != 2) continue;
                    values.Add(s[0], s[1]);
                }
                lineIndex++;
                return true;
            }


            // load padding
            _next();
            var p = _getArray("padding");
            paddingTop = p[0];
            paddingLeft = p[1];
            paddingBottom = p[2];
            paddingRight = p[3];

            // load line height
            _next();
            lineHeight = _getInt("lineHeight");
            baseLine = _getInt("base");

            // load characters
            _next(); // skip page
            _next(); // skip chars count

            var atlasSize = new vec2(atlas.width, atlas.height);

            while(_next()) {
                glyphs.Add(
                    new Glyph(
                        this,
                        _getInt("id"),
                        new vec2(_getInt("x"), _getInt("y"))             ,//    / atlasSize,
                        new vec2(_getInt("width"), _getInt("height"))    ,//    / atlasSize,
                        new vec2(_getInt("xoffset"), _getInt("yoffset")) ,//    / atlasSize,
                        _getInt("xadvance")                               //    / atlasSize.x
                    ));
            }
            
        }

        public Glyph getGlyph(char c) {
            var code = Encoding.ASCII.GetBytes(new[] { c })[0];

            return (from o in glyphs
                    where o.id == code
                    select o).FirstOrDefault();
        }
    }
}

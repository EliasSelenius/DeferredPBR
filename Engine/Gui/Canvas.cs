using System.Collections.Generic;
using Nums;
using OpenTK.Graphics.OpenGL4;

namespace Engine.Gui {

    /*
        Canvas class
            takes care of all gui related rendering


        todo:
            - rect
                - border
                - border radius
                - 
            - line
            - MSAA

    */
    public class Canvas {

        public int width, height;

        readonly Dictionary<Font, Mesh<textVertex>> textBatches = new();
        Mesh<textVertex> getTextBatch(Font font) {
            if (!textBatches.ContainsKey(font)) textBatches[font] = new Mesh<textVertex>();
            return textBatches[font];
        }

        Mesh<textVertex> rectBatch = new();

        public Canvas(int w, int h) {
            (width, height) = (w, h);
        }

        public void resize(int w, int h) {
            (width, height) = (w, h);
        }


        int initialSeed = 0;
        public void render() {
            beginFrame();
            text(vec2.zero, Font.arial, 16, "fps: " + Renderer.fps, in color.white);

            /*
            if (Keyboard.isPressed(key.G)) initialSeed++;
            int seed = initialSeed;       

            var a = new[] {
                "Hello",
                "Foo",
                "Bar",
                "Universe",
                "Turn",
                "I Dont wana fall",
                "Music", 
                "Hello World",
                "world",
                "planet",
                "space",
                "graphics", "hello", "world", "universe", "is", "four", "music", "programing", "random", "that", 
                "cool", "red", "green", "blue", "terminator", "beat", "artist", "tool", "internett", "openGL", "basic", "advanced", "whiskey", "antilope", 
                "artificial", "intelligence", "movie", "camera", "game", "object", "wheat", "farm", "car"
            };
            var colors = new[] {
                color.white,
                color.gray,
                color.silver,
                color.rgb(1, 0, 1),
                color.rgb(1, 1, 0),
                color.rgb(0, 0, 1),
                color.rgb(0, 1, 1)
            };

            text(vec2.zero, Font.arial, (int)(height), "Text", color.rgb(1, 1, 0));

            var c = math.range(seed++, 100, 1000);
            text((0, 16), Font.arial, 16, "count: " + c, in color.white);\
            for (int i = 0; i < c; i++) {
                text((math.range(seed++, 0, width), math.range(seed++, 0, height)), Font.arial, (int)math.range(seed++, 10, 30), math.pick(seed++, a), math.pick(seed++, colors));

            }


            var s = new vec2(width, height) / 10f;
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 10; j++) {
                    var pos = s * new vec2(i, j);
                    rect(pos, s, color.rgba(i / 10f, j / 10f, pos.distTo(Mouse.position) / 1000f, 0.8f));
                    text(pos, Font.arial, 22, pos.ToString(), in color.silver);
                }
            }
            */

            rect(150, 100, color.silver);
            rect(100, 100, color.white);


            endFrame();
        }

        public void beginFrame() {
            // clear text batches
            foreach (var batch in textBatches) {
                batch.Value.data.clear();
            }

            // clear rect batch
            rectBatch.data.clear();

            // update camera projection
            OpenTK.Mathematics.Matrix4.CreateOrthographic(width, height, 0, 10, out OpenTK.Mathematics.Matrix4 res);
            var p = res.toNums();
            Renderer.updateCameraProjection(ref p);

        }

        public void endFrame() {
            Renderer.whiteTexture.bind(TextureUnit.Texture0);
            rectBatch.updateBuffers();
            rectBatch.render();


            foreach (var batch in textBatches) {
                batch.Key.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
                batch.Value.updateBuffers();
                batch.Value.render();
            }
        }

        public void text(vec2 pos, Font font, int fontSize, string text, in color color) {

            pos -= new vec2(width, height) / 2f;
            pos.y = -pos.y;

            var mesh = getTextBatch(font);
            Text.genText(text, pos, fontSize, in color, font, mesh.data);
        }

        public void rect(vec2 pos, vec2 size, in color color) {
            pos -= new vec2(width, height) / 2f;
            pos.y = -pos.y;


            size.y = -size.y;

            color.color2vec(in color, out vec4 vertcolor);

            uint count = (uint)rectBatch.data.vertices.Count;

            rectBatch.data.vertices.Add(new textVertex {
                color = vertcolor,
                position = pos
            });

            rectBatch.data.vertices.Add(new textVertex {
                color = vertcolor,
                position = pos + (size.x, 0)
            });

            rectBatch.data.vertices.Add(new textVertex {
                color = vertcolor,
                position = pos + size
            });

            rectBatch.data.vertices.Add(new textVertex {
                color = vertcolor,
                position = pos + (0, size.y)
            });


            rectBatch.data.addTriangles(new uint[] {
                count + 0, count + 2, count + 1,
                count + 0, count + 3, count + 2
            });

        }

    }

    
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct textVertex : VertexData {

        public vec2 position;
        public vec2 uv;
        public vec4 color;

        public vec3 getPosition() => new vec3(position.x, position.y, 0f);
        public void setPosition(vec3 value) => position = value.xy;

        public vec2 getTexcoord() => uv;
        public void setTexcoord(vec2 value) => uv = value;

        public vec4 getColor() => color;
        public void setColor(vec4 value) => color = value;
    }
}
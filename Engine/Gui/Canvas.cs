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
        Mesh<textVertex> rectBatch = new();


        public Canvas(int w, int h) {
            (width, height) = (w, h);
        }

        public void resize(int w, int h) {
            (width, height) = (w, h);
        }



        public void dispatchFrame() {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Renderer.textShader.use();
            Renderer.whiteTexture.bind(TextureUnit.Texture0);
            
            // update camera projection
            OpenTK.Mathematics.Matrix4.CreateOrthographic(width, height, 0, 10, out OpenTK.Mathematics.Matrix4 res);
            var p = res.toNums();
            Renderer.updateCameraProjection(ref p);
            
            // render rects
            Renderer.whiteTexture.bind(TextureUnit.Texture0);
            rectBatch.updateBuffers();
            rectBatch.render();

            // render text
            foreach (var batch in textBatches) {
                batch.Key.atlas.bind(OpenTK.Graphics.OpenGL4.TextureUnit.Texture0);
                batch.Value.updateBuffers();
                batch.Value.render();
            }

            // clear text batches
            foreach (var batch in textBatches) {
                batch.Value.data.clear();
            }

            // clear rect batch
            rectBatch.data.clear();
        }

        Mesh<textVertex> getTextBatch(Font font) {
            if (!textBatches.ContainsKey(font)) textBatches[font] = new Mesh<textVertex>();
            return textBatches[font];
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
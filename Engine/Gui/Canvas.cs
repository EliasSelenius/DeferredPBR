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
        public ivec2 size => new ivec2(width, height);

        readonly Dictionary<Font, Mesh<textVertex>> textBatches = new();
        
        readonly Mesh<rectVertex> rectBatch = new();


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




        /*
            canvas coordinate system:
             0, 0 --------- 1, 0
              |              |
              |              |
              |              |
              |              |
             0, 1 --------- 1, 1        


            buffer coordinate system:
             -w, h  --------  w, h
               |               |
               |               |
               |       0       |
               |               |
               |               |
             -w, -h  -------  w, -h   
             where
                w = width/2
                h = height/2


            convert from canvas to buffer coord:
                pos -= new vec2(width, height) / 2f;
                pos.y = -pos.y;
        */
        private void convertCoord(ref vec2 coord) => coord.y = -(coord -= new vec2(width, height) / 2f).y;


        public void text(vec2 pos, Font font, int fontSize, string text, in color color) {
            convertCoord(ref pos);
            var mesh = getTextBatch(font);
            Text.genText(text, pos, fontSize, in color, font, mesh.data);
        }

        public void rect(vec2 pos, vec2 size, in color color) {
            convertCoord(ref pos);
            size.y = -size.y;

            color.color2vec(in color, out vec4 vertcolor);

            uint count = (uint)rectBatch.data.vertices.Count;

            vertex(pos, (0, 0), vertcolor);
            vertex(pos + (size.x, 0), (1, 0), vertcolor);
            vertex(pos + size, (1, 1), vertcolor);
            vertex(pos + (0, size.y), (0, 1), vertcolor);

            rectBatch.data.addTriangles(new uint[] {
                count + 2, count + 1, count + 0,
                count + 3, count + 2, count + 0
            });

        }

        void test(vec2 pos, vec2 size, float borderRadius, in color color) {
            convertCoord(ref pos);
            size.y = -size.y;


            /*
            
            */

            color.color2vec(in color, out vec4 vertcolor);

            vertex(pos + borderRadius, vec2.zero, in vertcolor);
            for (int i = 0; i < 4; i++) {
                vec2 p = pos;
                for (int j = 0; j < 32; j++) {
                    //vertex(p + new vec2(math.cos()))
                }
            }


        }
        void vertex(vec2 pos, vec2 uv, in vec4 color) {
            rectBatch.data.vertices.Add(new rectVertex {
                position = pos,
                uv = uv,
                color = color
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


    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct rectVertex : VertexData {

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
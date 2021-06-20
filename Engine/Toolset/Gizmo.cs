using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Toolset {
    public static class Gizmo {
        static Shader shader;

        static Batch points = new(PrimitiveType.Points);
        static Batch lines = new(PrimitiveType.Lines);

        static vec4 currentColor = vec4.one;

        static Gizmo() {
            shader = Assets.getShader("gizmo");
        }

        public static void color(in vec4 color) => currentColor = color;
        public static void color(in color color) => color.color2vec(in color, out currentColor);

        public static void point(vec3 pos) {
            points.vertex(pos, currentColor);
            points.index(points.vertices.Count - 1);
        }
        public static void line(vec3 start, vec3 end) {
            lines.vertex(start, currentColor);
            lines.vertex(end, currentColor);

            lines.index(lines.vertices.Count - 2);
            lines.index(lines.vertices.Count - 1);
        }

        public static void bezier(vec3 p0, vec3 p1, vec3 p2) {
            const int res = 20;
            float t = 0;
            
            line(p0, p1);
            line(p1, p2);
            for (int i = 0; i < res; i++) {
                line(math.bezier(p0, p1, p2, t), math.bezier(p0, p1, p2, t += 1f / res));
            }
        }

        public static void circle(in vec3 pos, in vec3 normal, float radius) {
            Utils.getTangentsOfNormal(in normal, out vec3 left, out vec3 up);

            const float res = 40; // 26;
            float angle = 0;
            const float d = math.tau / res;

            for (int i = 0; i < res; i++) {
                var p1 = pos + left * math.cos(angle) * radius + up * math.sin(angle) * radius;
                angle += d;
                var p2 = pos + left * math.cos(angle) * radius + up * math.sin(angle) * radius;
                line(p1, p2);
            }
        }

        public static void sphere(in vec3 pos, float radius) {
            circle(in pos, in vec3.unitx, radius);
            circle(in pos, in vec3.unity, radius);
            circle(in pos, in vec3.unitz, radius);
        }

        internal static void dispatchFrame() {
            shader.use();

            points.render();
            lines.render();
        }

        class Batch {
            public PrimitiveType primitiveType;
            public List<posColorVertex> vertices = new();
            public List<uint> indices = new();

            int vao, vbo, ebo;

            public Batch(PrimitiveType pType) {
                primitiveType = pType;

                vbo = GLUtils.createBuffer();
                ebo = GLUtils.createBuffer();
                vao = GLUtils.createVertexArray<posColorVertex>(vbo, ebo);
            }

            public void render() {
                GLUtils.bufferdata(vbo, vertices.ToArray());
                GLUtils.bufferdata(ebo, indices.ToArray());
                
                GL.BindVertexArray(vao);
                GL.DrawElements(primitiveType, indices.Count, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);

                clear();
            }

            void clear() {
                vertices.Clear();
                indices.Clear();
            }

            public void vertex(in vec3 pos, in vec4 color) => vertices.Add(new posColorVertex { position = pos, color = color });
            public void index(int i) => indices.Add((uint)i);
        }
    }

}
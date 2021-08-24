using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Nums;

namespace Engine {
    public static partial class GLUtils {

        static readonly Dictionary<System.Type, Attrib[]> attributes = new();

        private class Attrib {
            public readonly int index;
            public readonly int compSize;
            public readonly VertexAttribPointerType type;
            public readonly bool normalized;
            public readonly int stride;
            public readonly int offset;


            public Attrib(int i, System.Type fieldType, bool norm, int str, int ofs) {
                index = i;

                    if (fieldType == typeof(float)) (compSize, type) = (1, VertexAttribPointerType.Float);
                else if (fieldType == typeof(double)) (compSize, type) = (1, VertexAttribPointerType.Double);
                else if (fieldType == typeof(int)) (compSize, type) = (1, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec2)) (compSize, type) = (2, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec2)) (compSize, type) = (2, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec2)) (compSize, type) = (2, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec3)) (compSize, type) = (3, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec3)) (compSize, type) = (3, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec3)) (compSize, type) = (3, VertexAttribPointerType.Int);
                else if (fieldType == typeof(vec4)) (compSize, type) = (4, VertexAttribPointerType.Float);
                else if (fieldType == typeof(dvec4)) (compSize, type) = (4, VertexAttribPointerType.Double);
                else if (fieldType == typeof(ivec4)) (compSize, type) = (4, VertexAttribPointerType.Int);
                else throw new System.Exception("Type not supported");

                normalized = norm;
                stride = str;
                offset = ofs;

            }

            public void apply() {
                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(index, compSize, type, normalized, stride, offset);
            }

        }

        private static void setupAttribPointers<VertType>() where VertType : struct {
            var type = typeof(VertType);
            if (!attributes.ContainsKey(type)) {
                var attribs = new List<Attrib>();
                int i = 0;
                int offset = 0;
                foreach (var field in type.GetFields()) {
                    var a = new Attrib(i, field.FieldType, false, System.Runtime.InteropServices.Marshal.SizeOf<VertType>(), offset);
                    attribs.Add(a);
                    offset += a.compSize * a.type switch {
                        VertexAttribPointerType.Float => sizeof(float),
                        VertexAttribPointerType.Double => sizeof(double),
                        VertexAttribPointerType.Int => sizeof(int),
                        _ => throw new System.Exception("invalid Type")
                    };
                    i++;
                }
                attributes.Add(type, attribs.ToArray());
            }


            foreach (var a in attributes[type]) {
                a.apply();
            }

        }


        public static int createVertexArray<VertType>(int vbo, int ebo) where VertType : struct {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            setupAttribPointers<VertType>();
            GL.BindVertexArray(0);
            return vao;
        }

        public static int createVertexArray<VertType>(int vbo) where VertType : struct {
            int vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            setupAttribPointers<VertType>();
            GL.BindVertexArray(0);
            return vao;
        }
    }
}
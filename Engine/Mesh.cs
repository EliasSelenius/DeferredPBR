using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Engine {
    public class Mesh<VertType> where VertType : struct, VertexData {
        public Meshdata<VertType> data;
        int vbo, ebo, vao;

        private void initBuffers() {
            vbo = GLUtils.createBuffer();
            ebo = GLUtils.createBuffer();
            vao = GLUtils.createVertexArray<VertType>(vbo, ebo);
        }

        public Mesh() {
            this.data = new Meshdata<VertType>();
            this.initBuffers();
        }

        public Mesh(Meshdata<VertType> data) {
            this.data = data;
            this.initBuffers();
            this.updateBuffers();
        }

        public void updateBuffers() {
            GLUtils.bufferdata(vbo, data.vertices.ToArray());
            GLUtils.bufferdata(ebo, data.indices.ToArray());
        }

        public void render() {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, data.indices.Count, DrawElementsType.UnsignedInt, 0);
        }

        public void render(IMaterial[] materials) {
            GL.BindVertexArray(vao);

            int offset = 0;
            foreach (var group in data.groups) {
                materials[group.Key].use();
                GL.DrawElements(PrimitiveType.Triangles, group.Value, DrawElementsType.UnsignedInt, offset * sizeof(uint));
                offset += group.Value;
            }
        }

        private void delete() {
            if (vao == 0) return;

            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);

            data = null;

            vao = 0;
        }

        ~Mesh() {
            if (vao != 0) throw new Exception("Memory leak detected! vao: " + vao);
        }
    }


    class Model {
        public Mesh<Vertex>[] meshes;
        public Material[] materials;
        public Node[] rootNodes;

        public class Node {
            public readonly Node[] children;
            public readonly Node parent;

            public readonly Transform transform;

            public Mesh<Vertex> mesh;
            public Material[] materials;
        }

        public void render() {

        }

    }
}

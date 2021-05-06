using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Engine {
    public class Meshdata<VertType> where VertType : struct, VertexData {
        public List<VertType> vertices { get; private set; } = new List<VertType>();
        public List<uint> indices { get; private set; } = new List<uint>();

        // groups: <int, int> = <materialIndex, indicesCount>
        public Dictionary<int, int> groups = new Dictionary<int, int>();

        public Meshdata() { }

        public Meshdata(IEnumerable<VertType> verts, IEnumerable<uint> inds) {
            vertices.AddRange(verts);
            addTriangles(inds);
        }

        public void clear() {
            groups.Clear();
            vertices.Clear();
            indices.Clear();
        }


        public void mutate(Func<VertType, int, VertType> f) {
            for (int i = 0; i < vertices.Count; i++) vertices[i] = f(vertices[i], i);   
        }
        public void mutate(Func<VertType, VertType> f) {
            for (int i = 0; i < vertices.Count; i++) vertices[i] = f(vertices[i]);   
        }

        public void addTriangles(IEnumerable<uint> ind) => addTriangles(0, ind);
        public void addTriangles(int material, IEnumerable<uint> ind) {
            var indLength = ind.Count();
            if (groups.ContainsKey(material)) groups[material] += indLength;
            else groups[material] = indLength;

            int offset = 0;
            foreach (var group in groups) {
                if (group.Key == material) {
                    this.indices.InsertRange(offset, ind);
                    break;
                }
                offset += group.Value;
            }
        }


        public void add(Meshdata<VertType> addition, in vec3 offset, int materialIndexOffset = 0) {
            uint vertCount = (uint)this.vertices.Count;

            // add vertices
            for (int i = 0; i < addition.vertices.Count; i++) {
                var v = addition.vertices[i];
                v.setPosition(v.getPosition() + offset);
                this.vertices.Add(v);
            }

            // add triangles
            foreach (var g in addition.groups) {
                this.addTriangles(g.Key + materialIndexOffset, addition.indices.Select(x => vertCount + x));
            }


        }


        public void genNormals() {
            // set all normals to zero
            for (int i = 0; i < vertices.Count; i++) {
                var v = this.vertices[i];
                v.setNormal(vec3.zero);
                this.vertices[i] = v;
            }

            void addNorm(int i, in vec3 no) {
                var v = vertices[i];
                // v.normal += no;
                v.setNormal(v.getNormal() + no);
                vertices[i] = v;
            }

            // add every triangles contribution to every vertex normal
            for (int i = 0; i < indices.Count; i += 3) {
                int i1 = (int)indices[i],
                    i2 = (int)indices[i + 1],
                    i3 = (int)indices[i + 2];

                var v3 = vertices[i3].getPosition();
                var no = (vertices[i1].getPosition() - v3).cross(vertices[i2].getPosition() - v3);

                addNorm(i1, in no);
                addNorm(i2, in no);
                addNorm(i3, in no);
            }

            // normalize vertex normals
            for (int i = 0; i < vertices.Count; i++) {
                var v = vertices[i];
                // v.normal.normalize();
                v.setNormal(v.getNormal().normalized());
                vertices[i] = v;
            }
        }

        public void flipIndices() {
            for (int i = 0; i < indices.Count; i+=3) {
                var a = indices[i];
                indices[i] = indices[i + 2];
                indices[i + 2] = a;
            }
        }

        public void getBoundaries(out vec3 maxbounds, out vec3 minbounds) {
            maxbounds = minbounds = vec3.zero;

            for (int i = 0; i < vertices.Count; i++) {
                var v = vertices[i].getPosition();
                minbounds.x = math.min(v.x, minbounds.x);
                minbounds.y = math.min(v.y, minbounds.y);
                minbounds.z = math.min(v.z, minbounds.z);

                maxbounds.x = math.max(v.x, maxbounds.x);
                maxbounds.y = math.max(v.y, maxbounds.y);
                maxbounds.z = math.max(v.z, maxbounds.z);
            }
        }
    }


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
}

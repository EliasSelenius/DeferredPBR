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

        public void render(PBRMaterial[] materials) {
            GL.BindVertexArray(vao);

            int offset = 0;
            foreach (var group in data.groups) {
                materials[group.Key].updateUniforms();
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





    public static class MeshFactory<T> where T : struct, VertexData {
        public static Meshdata<T> randomMesh() {
            return null;
        }

        public static Meshdata<T> genQuad() {
            var res = genPlane(1, 1);
            res.mutate(v => {
                v.setPosition(v.getPosition().xzy);
                return v;
            });
            res.flipIndices();
            res.genNormals();
            //res.bufferdata();
            return res;
        }

        public static Meshdata<T> genCube(int res, float scale) {
            var r = _genCube(res, scale);
            r.genNormals();
            //r.bufferdata();
            return r;
        }

        private static Meshdata<T> _genCube(int res, float scale) {
            var m = new Meshdata<T>();

            res += 1;

            var vertfunc = new Func<vec3, vec3>[] {
                v => v,
                v => v.zyx * new vec3(1, -1, 1),
                v => v.yzx,
                v => v.yxz * new vec3(-1, 1, 1),
                v => v.zxy,
                v => v.xzy * new vec3(1, 1, -1)
            };


            for (int f = 0; f < 6; f++) {
                int i = res * res * f;
                var ind = new List<uint>();
                for (int ix = 0; ix < res; ix++) {
                    for (int iy = 0; iy < res; iy++) {

                        float x = math.map(ix, 0, res - 1, -0.5f, 0.5f);
                        float y = math.map(iy, 0, res - 1, -0.5f, 0.5f);                    
                        var pos = vertfunc[f](new vec3(x, 0.5f, y));

                        var v = new T();
                        v.setPosition(pos);
                        m.vertices.Add(v);

                        if (ix < res - 1 && iy < res - 1) {
                            ind.Add((uint)i);
                            ind.Add((uint)i + 1);
                            ind.Add((uint)i + (uint)res + 1);

                            ind.Add((uint)i);
                            ind.Add((uint)i + (uint)res + 1);
                            ind.Add((uint)i + (uint)res);
                        }

                        i++;
                    }
                }
                m.addTriangles(0, ind);
            }

            return m;
        }

        public static Meshdata<T> genSphere(int res, float scale) {
            var m = _genCube(res, 1f);
            m.mutate((v, i) => {
                v.setPosition(v.getPosition().normalized() * scale);
                return v;
            });

            m.genNormals();
            //m.bufferdata();
            return m;
        }

        public static Meshdata<T> genPlane(int res, float scale, float uvScale = 1f) {
            var mesh = new Meshdata<T>();
            var ind = new List<uint>();
            int i = 0;
            for (int ix = 0; ix <= res; ix++) {
                for (int iz = 0; iz <= res; iz++) {
                    float x = math.map(ix, 0, res, -0.5f, 0.5f) * scale;
                    float z = math.map(iz, 0, res, -0.5f, 0.5f) * scale;
                    
                    var v = new T();
                    v.setPosition(new vec3(x, 0, z));
                    v.setTexcoord((new vec2(ix, iz) / res) * uvScale);

                    mesh.vertices.Add(v);

                    if (ix < res && iz < res) {
                        ind.Add((uint)i);
                        ind.Add((uint)i + 1);
                        ind.Add((uint)(i + res + 1));

                        ind.Add((uint)i + 1);
                        ind.Add((uint)(i + res + 2));
                        ind.Add((uint)(i + res + 1));
                    }
                    i++;
                }   
            }

            mesh.addTriangles(0, ind);
            mesh.genNormals();
            //mesh.bufferdata();
            return mesh;
        }
    }






    public class MeshRenderer : Component, IRenderer {
        public Mesh<Vertex> mesh;
        public PBRMaterial[] materials;

        public void render() {
            gameobject.calcModelMatrix(out mat4 mat);
            GLUtils.setUniformMatrix4(Renderer.geomPass.id, "model", ref mat);
            mesh.render(materials);
        }

        /*public void render(PBRMaterial material) {
            gameobject.calcModelMatrix(out mat4 mat);
            GLUtils.setUniformMatrix4(Renderer.geomPass.id, "model", ref mat);
            material.updateUniforms();
            mesh.render();
        }*/

        public void render(int shaderID) {
            gameobject.calcModelMatrix(out mat4 mat);
            GLUtils.setUniformMatrix4(shaderID, "model", ref mat);
            mesh.render();
        }

        protected override void onEnter() {
            scene.renderers.Add(this);
        }

        protected override void onLeave() {
            scene.renderers.Remove(this);
        }
    }
}

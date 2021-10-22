using Nums;
using System.Collections.Generic;
using System;

namespace Engine {
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
}

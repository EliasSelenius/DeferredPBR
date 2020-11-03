using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Collections.Generic;
using System.Linq;
using System;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex {
    public vec3 position;
    public vec3 normal;
    public vec2 uv;
    //public vec3 color;
}



class Mesh : Mesh<Vertex> {

    public override void genNormals() {
        // set all normals to zero
        for (int i = 0; i < vertices.Count; i++) {
            var v = this.vertices[i];
            v.normal = vec3.zero;
            this.vertices[i] = v;
        }

        void addNorm(int i, in vec3 no) {
            var v = vertices[i];
            v.normal += no;
            vertices[i] = v;
        }

        // add every triangles contribution to every vertex normal
        for (int i = 0; i < indices.Count; i += 3) {
            int i1 = (int)indices[i],
                i2 = (int)indices[i + 1],
                i3 = (int)indices[i + 2];

            var v3 = vertices[i3].position;
            var no = (vertices[i1].position - v3).cross(vertices[i2].position - v3);

            addNorm(i1, in no);
            addNorm(i2, in no);
            addNorm(i3, in no);
        }

        // normalize vertex normals
        for (int i = 0; i < vertices.Count; i++) {
            var v = vertices[i];
            v.normal.normalize();
            vertices[i] = v;
        }
    }
}
class Mesh<VertType> where VertType : struct {

    public List<VertType> vertices { get; private set; } = new List<VertType>();
    public List<uint> indices { get; private set; } = new List<uint>();

    // groups: <int, int> = <materialIndex, indicesCount>
    public Dictionary<int, int> groups = new Dictionary<int, int>();

    int vbo, ebo, vao;

    public Mesh() {
        vbo = GLUtils.createBuffer();
        ebo = GLUtils.createBuffer();
        vao = GLUtils.createVertexArray<VertType>(vbo, ebo);
    }

    //public static Mesh<V> copy<V>(Mesh<V> other) where V : struct => new Mesh<V>(other.vertices, other.indices);
    //public static Mesh<V> copy<V, O>(Mesh<O> other, Func<O, V> castFunc) where V : struct where O : struct => new Mesh<V>(other.vertices.Select(x => castFunc(x)).ToArray(), other.indices);

    public void mutate(Func<VertType, int, VertType> f) {
        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] = f(vertices[i], i);
        }
    }

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

    public virtual void genNormals() {
        throw new NotImplementedException();
    }

    public void flipIndices() {

    }

    public void bufferdata() {
        GLUtils.bufferdata(vbo, vertices.ToArray());
        GLUtils.bufferdata(ebo, indices.ToArray());
    }

    public void render(PBRMaterial[] materials) {
        GL.BindVertexArray(vao);
        //GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);

        int offset = 0;
        foreach (var group in groups) {
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

        vertices = null;
        indices = null;

        vao = 0;
    }

    ~Mesh() {
        if (vao != 0) System.Console.WriteLine("Memory leak detected! vao:" + vao);
    }

}

static class MeshFactory {
    public static Mesh<Vertex> randomMesh() {
        return null;
    }

    public static Mesh genCube(int res, float scale) {
        var r = _genCube(res, scale);
        r.genNormals();
        r.bufferdata();
        return r;
    }

    private static Mesh _genCube(int res, float scale) {
        var m = new Mesh();

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
                    m.vertices.Add(new Vertex {
                        position = vertfunc[f](new vec3(x, 0.5f, y))
                    });

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

    public static Mesh genSphere(int res, float scale) {
        var m = _genCube(res, 1f);
        m.mutate((v, i) => {
            v.position.normalize();
            v.position *= scale;
            return v;
        });

        m.genNormals();
        m.bufferdata();
        return m;
    }
}


class MeshRenderer {
    public Mesh mesh;
    public PBRMaterial[] materials;

    public void render() {
        mesh.render(materials);
    }
}
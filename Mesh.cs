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


class Mesh<VertType> where VertType : struct {

    public List<VertType> vertices { get; private set; } = new List<VertType>();
    public List<uint> indices { get; private set; } = new List<uint>();

    int vbo, ebo, vao;

    public Mesh() {
        vbo = GLUtils.createBuffer();
        ebo = GLUtils.createBuffer();
        vao = GLUtils.createVertexArray<VertType>(vbo, ebo);
    }

    //public static Mesh<V> copy<V>(Mesh<V> other) where V : struct => new Mesh<V>(other.vertices, other.indices);
    //public static Mesh<V> copy<V, O>(Mesh<O> other, Func<O, V> castFunc) where V : struct where O : struct => new Mesh<V>(other.vertices.Select(x => castFunc(x)).ToArray(), other.indices);

    public void addTriangles(IEnumerable<uint> ind) {
        this.indices.AddRange(ind);
    }

    public void bufferdata() {
        GLUtils.bufferdata(vbo, vertices.ToArray());
        GLUtils.bufferdata(ebo, indices.ToArray());
    }

    public void render() {
        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
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
}
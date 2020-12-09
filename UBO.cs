
using OpenTK.Graphics.OpenGL4;

public class UBO {
    public readonly int id;
    public readonly int bindingPoint;
    public readonly string name;

    static int bindingPoint_count = 0;

    public UBO(string name, int bytesize) {
        this.name = name;
        id = GLUtils.createBuffer(bytesize);
        bindingPoint = bindingPoint_count++;

        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, id);
    }
}
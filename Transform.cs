using Nums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

class Transform {
    public vec3 position;
    public vec3 scale = vec3.one;
    public quat rotation;

    public mat4 getMatrix() {
        var m = mat4.identity;
        
        m.m11 = scale.x;
        m.m22 = scale.y;
        m.m33 = scale.z;

        //m *= Utils.createRotation(rotation.x, rotation.y, rotation.z);

        m *= new mat4(quat.toMatrix(rotation));

        m.row4.xyz = position;

        return m;
    }
    public void setMatrix(mat4 m) {
        position = m.row4.xyz;

        scale.x = m.row1.xyz.length;
        scale.y = m.row2.xyz.length;
        scale.z = m.row3.xyz.length;

        var rm = new mat3(m);
        // the divisions here normalizes the vectors
        rm.row1 /= scale.x;
        rm.row2 /= scale.y;
        rm.row3 /= scale.z;

        rotation = quat.fromMatrix(rm);
    }


    public void applyUniforms() {
        var m = getMatrix();
        int loc = GL.GetUniformLocation(Renderer.geomPass.id, "model");
        //GL.UniformMatrix4(loc, 1, false, ref m.row1.x);
        GLUtils.setUniformMatrix4(loc, ref m);
    }
}
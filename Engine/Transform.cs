using Nums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine {
    public class Transform {
        public vec3 position;
        public vec3 scale = vec3.one;
        public quat rotation = quat.identity;

        public vec3 left => quat.leftVector(rotation);
        public vec3 right => -left;
        public vec3 up => quat.upVector(rotation);
        public vec3 down => -up;
        public vec3 forward => quat.forwardVector(rotation);
        public vec3 backward => -forward;

        public void set(Transform transform) {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.scale = transform.scale;
        }

        public void getMatrix(out mat4 m) {
            m = mat4.identity;
            
            m.m11 = scale.x;
            m.m22 = scale.y;
            m.m33 = scale.z;

            //m *= Utils.createRotation(rotation.x, rotation.y, rotation.z);

            quat.toMatrix(rotation, out mat3 m3);
            m *= new mat4(m3);

            m.row4.xyz = position;
        }
        public void setMatrix(in mat4 m) {
            position = m.row4.xyz;

            scale.x = m.row1.xyz.length;
            scale.y = m.row2.xyz.length;
            scale.z = m.row3.xyz.length;

            var rm = new mat3(m);
            // the divisions here normalizes the vectors
            rm.row1 /= scale.x;
            rm.row2 /= scale.y;
            rm.row3 /= scale.z;

            quat.fromMatrix(rm, out rotation);
        }


        public void rotate(quat q) => rotation *= q;
        public void rotate(vec3 axis, float angle) => rotate(quat.fromAxisangle(axis, angle));

    }
}
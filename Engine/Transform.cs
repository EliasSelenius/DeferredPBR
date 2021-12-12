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

        public void transform(in mat4 matrix) {
            this.getMatrix(out mat4 t);
            t *= matrix;
            this.setMatrix(in t);
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

            // mirroring
            if (m.row1.xyz.cross(m.row2.xyz).dot(m.row3.xyz) < 0) {
                scale.x *= -1;
            }

            var rm = new mat3(m);
            // the divisions here normalizes the vectors
            rm.row1 /= scale.x;
            rm.row2 /= scale.y;
            rm.row3 /= scale.z;

            quat.fromMatrix(rm, out rotation);

            // TODO: create mat4.decompose(out vec3 translation, out vec3 scale, out quat rotation) in Nums
        }


        public void rotate(quat q) {
            rotation *= q;
            rotation.normalize();
        }
        public void rotate(vec3 axis, float angle) => rotate(quat.fromAxisangle(axis, angle));

        public void lookat(in vec3 point, in vec3 up) {
            Utils.invert(math.lookat(position, point, up), out mat4 m);
            m.row1.xyz = -m.row1.xyz;
            m.row3.xyz = -m.row3.xyz;
            quat.fromMatrix(new mat3(m), out rotation);
        }

    }
}
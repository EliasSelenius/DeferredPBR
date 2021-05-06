
using Nums;

/*
    must haves for quaternion struct
     - to/from matrix conversion
     - to/from axis angle
     - multiplication
     - Slerp

*/

namespace Engine {

    public struct quat {
        // w + xi + yj + zk
        public float x, y, z, w;
        
        public static readonly quat identity = new quat(0, 0, 0, 1);

        public quat(float _x, float _y, float _z, float _w) => (x, y, z, w) = (_x, _y, _z, _w);

        public static vec3 leftVector(in quat q) => new vec3(1 - 2 * (q.y*q.y + q.z*q.z), 2 * (q.x*q.y - q.z*q.w), 2 * (q.x*q.z + q.y*q.w));
        public static vec3 upVector(in quat q) => new vec3(2 * (q.x*q.y + q.z*q.w), 1 - 2 * (q.x*q.x + q.z*q.z), 2 * (q.y*q.z - q.x*q.w));
        public static vec3 forwardVector(in quat q) => new vec3(2 * (q.x*q.z - q.y*q.w), 2 * (q.y*q.z + q.x*q.w), 1 - 2 * (q.x*q.x + q.y*q.y));


        /// <summary>note: assumes axis is normalized</summary>
        public static quat fromAxisangle(in vec3 axis, float angle) {        
            float ha = angle / 2f,
                sin = math.sin(ha);
            return new quat {
                x = axis.x * sin,
                y = axis.y * sin,
                z = axis.z * sin,
                w = math.cos(ha)
            };
        }

        public static void toMatrix(in quat q, out mat3 res) {
            float xx = q.x * q.x,
                xy = q.x * q.y,
                xz = q.x * q.z,
                xw = q.x * q.w,

                yy = q.y * q.y,
                yz = q.y * q.z,
                yw = q.y * q.w,

                zz = q.z * q.z,
                zw = q.z * q.w;

            /*return new mat3(1 - 2 * (yy + zz), 2 * (xy - zw), 2 * (xz + yw),
                            2 * (xy + zw), 1 - 2 * (xx + zz), 2 * (yz - xw),
                            2 * (xz - yw), 2 * (yz + xw), 1 - 2 * (xx + yy));*/
            
            res = new mat3( yy + zz, xy - zw, xz + yw,
                            xy + zw, xx + zz, yz - xw,
                            xz - yw, yz + xw, xx + yy ) * 2;
            res.diagonal = 1 - res.diagonal;
        }

        public static void fromMatrix(in mat3 m, out quat q) {
            var trace = m.trace;
            q = new quat();
            if (trace > 0) {
                float s = 0.5f / math.sqrt(trace + 1.0f);
                q.w = 0.25f / s;
                q.x = (m[2,1] - m[1,2]) * s;
                q.y = (m[0,2] - m[2,0]) * s;
                q.z = (m[1,0] - m[0,1]) * s;
            } else {
                if (m[0,0] > m[1,1] && m[0,0] > m[2,2]) {
                    float s = 2.0f * math.sqrt(1.0f + m[0,0] - m[1,1] - m[2,2]);
                    q.w = (m[2,1] - m[1,2]) / s;
                    q.x = 0.25f * s;
                    q.y = (m[0,1] + m[1,0]) / s;
                    q.z = (m[0,2] + m[2,0]) / s;
                } else if (m[1,1] > m[2,2]) {
                    float s = 2.0f * math.sqrt(1.0f + m[1,1] - m[0,0] - m[2,2]);
                    q.w = (m[0,2] - m[2,0] ) / s;
                    q.x = (m[0,1] + m[1,0] ) / s;
                    q.y = 0.25f * s;
                    q.z = (m[1,2] + m[2,1] ) / s;
                } else {
                    float s = 2.0f * math.sqrt( 1.0f + m[2,2] - m[0,0] - m[1,1] );
                    q.w = (m[1,0] - m[0,1] ) / s;
                    q.x = (m[0,2] + m[2,0] ) / s;
                    q.y = (m[1,2] + m[2,1] ) / s;
                    q.z = 0.25f * s;
                }
            }
        }

        public quat conj() => new quat(-x, -y, -z, w);


        public static quat operator *(in quat l, in quat r) {
            // (a + ib + jc + kd) * (e + if + jg + kh)
            float a = l.w,
                b = l.x,
                c = l.y,
                d = l.z,
                
                e = r.w,
                f = r.x,
                g = r.y,
                h = r.z;

            return new quat {
                w = a*e - b*f - c*g - d*h,
                x = b*e + a*f + c*h - d*g,
                y = a*g - b*h + c*e + d*f,
                z = a*h + b*g - c*f + d*e
            };
        }

        public override string ToString() => $"({w} + {x}i + {y}j + {z}k)";
    }
}

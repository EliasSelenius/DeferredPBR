using Nums;

namespace Engine {
    public static class Physics {

        public static bool rayIntersectsSphere(in vec3 p1, in vec3 dir, in vec3 p2, float radius) {
            var hypv = p2 - p1;
            var dot = dir.dot(hypv);
            return math.sqrt(hypv.sqlength - dot * dot) - radius < 0;
        }


        public static void sphere2sphere_Intersection(in vec3 p1, float r1, in vec3 p2, float r2, out intersection intersection) {
            var direction = p2 - p1;
            var distance = direction.length;
            var normdir = direction / distance;

            intersection = new intersection(p1 + normdir * r1, p2 - normdir * r2, -(distance - r1 - r2));

            //var isintersecting = (p2 - p1).sqlength < (r1 + r2) * (r1 + r2);
        }

        public static void sphere2AABB_Intersection(in vec3 p1, float r1, in vec3 p2, in vec3 s2, out intersection intersection) {
            // NOTE: hardcoded to intersect with an infinite plane at y 0
            
            var i1 = p1;
            i1.y -= r1;
            var i2 = i1;
            i2.y = 0;

            intersection = new intersection(in i1, in i2, -i1.y);
        }

        public static void sphere2box_Intersection() {

        }

        public static void AABB2AABB_Intersection(in vec3 p1, in vec3 s1, in vec3 p2, in vec3 s2, out intersection intersection) {
            throw new System.NotImplementedException();
        }


    }

}

using System.Collections.Generic;
using Nums;

namespace Engine {

    public readonly ref struct collision {
        public readonly Collider c1;
        public readonly Collider c2;
        public readonly intersection intersection;
    }

    public readonly ref struct intersection {
        public readonly vec3 point1;
        public readonly vec3 point2;
        public readonly float depth;
    }

    public static class Physics {
        
        
        public static void sphere2sphere_Intersection(in vec3 p1, float r1, in vec3 p2, float r2) {

        }

        public static void sphere2AABB_Intersection(in vec3 p1, float r1, in vec3 p2, in vec3 s2) {

        }

        public static void sphere2box_Intersection() {

        }

        public static void AABB2AABB_Intersection(in vec3 p1, in vec3 s1, in vec3 p2, in vec3 s2) {

        }


    }

    interface collideswith<T> where T : Collider {
        void intersects(T other, out intersection intersection);
    }

    public abstract class Collider : Component, collideswith<SphereCollider>, collideswith<AABBCollider>, collideswith<BoxCollider> {
        public abstract void intersects(Collider other, out intersection intersection);
        public abstract void intersects(SphereCollider other, out intersection intersection);
        public abstract void intersects(AABBCollider other, out intersection intersection);
        public abstract void intersects(BoxCollider other, out intersection intersection);
    }
}
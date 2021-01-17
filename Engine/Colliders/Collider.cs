
using System.Collections.Generic;
using Nums;

using System;

namespace Engine {

    public readonly ref struct collision {
        public readonly Collider c1;
        public readonly Collider c2;
        public readonly intersection intersection;

        public collision(Collider c1, Collider c2, in intersection intersection) {
            this.c1 = c1;
            this.c2 = c2;
            this.intersection = intersection;
        }
    }

    public readonly ref struct intersection {
        public readonly vec3 point1;
        public readonly vec3 point2;
        public readonly float depth;

        public intersection(in vec3 p1, in vec3 p2, float d) {
            point1 = p1;
            point2 = p2;
            depth = d;
        }

    }

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

        protected override void onEnter() {
            scene.colliders.add(this);
        }

        protected override void onLeave() {
            scene.colliders.remove(this);
        }

        public abstract bool intersectsRay(in vec3 pos, in vec3 dir);

        public abstract void intersects(Collider other, out intersection intersection);
        public abstract void intersects(SphereCollider other, out intersection intersection);
        public abstract void intersects(AABBCollider other, out intersection intersection);
        public abstract void intersects(BoxCollider other, out intersection intersection);
    }


    public class ColliderCollection {
        readonly List<Collider> colliders = new List<Collider>();

        public void add(Collider collider) => colliders.Add(collider);
        public void remove(Collider collider) => colliders.Remove(collider);

        public void testCollisions(ColliderCollection other) {

            // small optimalization when testing against itself
            if (this == other) {
                for (int i = 0; i < colliders.Count; i++) {
                    for (int j = i + 1; j < other.colliders.Count; j++) {
                        var c1 = colliders[i];
                        var c2 = other.colliders[j];
                        if (c1 == c2) continue;
                        test(c1, c2);
                    }
                }
            } else {
                for (int i = 0; i < colliders.Count; i++) {
                    for (int j = 0; j < other.colliders.Count; j++) {
                        var c1 = colliders[i];
                        var c2 = other.colliders[j];
                        if (c1 == c2) continue;
                        test(c1, c2);
                    }
                }
            }

            void test(Collider c1, Collider c2) {

                c1.intersects(c2, out intersection ints);
                if (ints.depth > 0) {
                    var collision = new collision(c1, c2, ints);
                    System.Console.WriteLine("Collision depth: " + collision.intersection.depth);
                }

            }
        }

        public Collider raycast(in vec3 pos, in vec3 dir) {
            for (int i = 0; i < colliders.Count; i++) {
                var collider = colliders[i];
                if (collider.intersectsRay(in pos, in dir)) return collider;
            }
            return null;
        }

    }

}
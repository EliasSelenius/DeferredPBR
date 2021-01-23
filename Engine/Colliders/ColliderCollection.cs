
using System.Collections.Generic;
using Nums;

namespace Engine {
    public class ColliderCollection {
        readonly List<Collider> colliders = new List<Collider>();

        public void add(Collider collider) => colliders.Add(collider);
        public void remove(Collider collider) => colliders.Remove(collider);

        public void testCollisions(ColliderCollection other) {

            // optimalization when testing against itself
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
                    var collision1 = new collision(c1, c2, ints);
                    var ints2 = new intersection(in ints.point2, in ints.point1, ints.depth);
                    var collision2 = new collision(c2, c1, ints2);
                    
                    c1.gameobject.getComponent<Rigidbody>()?.handleCollisionDynamics(in collision2);
                    c2.gameobject.getComponent<Rigidbody>()?.handleCollisionDynamics(in collision1);
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
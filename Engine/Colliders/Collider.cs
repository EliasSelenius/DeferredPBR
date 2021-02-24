using Nums;

using System;


namespace Engine {

    // TODO: collision and intersection structs should have perspective


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

}
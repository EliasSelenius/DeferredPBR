
using Nums;

namespace Engine {
    public class Rigidbody : Component {
        public float mass = 1f;
        public vec3 velocity = vec3.zero;
        public vec3 centerOfMass = vec3.zero;

        public void addForce(vec3 force) {
            velocity += force / mass;
        }

        protected override void onUpdate() {
            transform.position += velocity * app.deltaTime;
        }

    }
}
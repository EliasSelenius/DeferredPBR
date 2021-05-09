
using Nums;

namespace Engine {
    public class Rigidbody : Component {
        public float mass = 1f;
        public vec3 centerOfMass = vec3.zero;

        public vec3 velocity = vec3.zero;
        public quat angularVelocity = quat.identity;

        public vec3 momentum => velocity * mass; 

        public void addForce(in vec3 force) {
            velocity += force / mass;
        }

        public void addForce(in vec3 force, in vec3 offset) {
            addForce(force);

            var axis = (offset - centerOfMass).cross(force);
            var angle = axis.length;
            axis /= angle;
            addTorque(in axis, angle);
        }

        public void addTorque(in vec3 axis, float angle) {
            addTorque(quat.fromAxisangle(axis, angle));
        }

        public void addTorque(in quat torque) {
            // TODO: how do we convert from torque to angularacceleration using F = ma, mass is not acounted for here:
            angularVelocity *= torque;
        }

        protected override void onUpdate() {

            // gravity
            //velocity.y -= 10 * Application.deltaTime;

            transform.position += velocity * Application.deltaTime;
            transform.rotate(angularVelocity);
        }

        internal void handleCollisionDynamics(in collision collision) {            
            var normal = collision.intersection.point2 - collision.intersection.point1;
            transform.position += normal;
            normal /= collision.intersection.depth;
            
            velocity = velocity.reflect(normal);
            //addForce(-momentum * app.deltaTime, in collision.intersection.point1);
        }
    }
}
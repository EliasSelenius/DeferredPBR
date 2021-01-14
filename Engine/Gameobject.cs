using System.Collections.ObjectModel;
using System.Collections.Generic;
using Nums;
using System.Linq;



namespace Engine {
    public class Gameobject {
        public readonly Transform transform = new();
        
        public Scene scene { get; private set; }

        public Gameobject parent { get; private set; }

        List<Gameobject> _children = new List<Gameobject>();
        public readonly ReadOnlyCollection<Gameobject> children;

        List<Component> _components = new List<Component>();
        public readonly ReadOnlyCollection<Component> components;

        public bool isRootObject => parent == null;
        public bool isParent => children.Count > 0;
        public bool isChild => parent != null;

        public Gameobject() {
            children = _children.AsReadOnly();
            components = _components.AsReadOnly();
        }

        public void calcModelMatrix(out mat4 m) {
            transform.getMatrix(out m);
            if (parent != null) {
                parent.calcModelMatrix(out mat4 p);
                m *= p;
            }
        }

#region add/get components 

        public void addComponent(Component comp) {
            if (comp.gameobject != null) throw new System.Exception("Component is already attached to a object");

            _components.Add(comp);
            comp.gameobject = this;
        }

        public T getComponent<T>() where T : Component => (T)_components.Find(c => c is T);
        

#endregion


#region add/remove children

        public void addChild(Gameobject child) {
            // make sure the child is not already a child
            if (child.parent is not null) child.parent.removeChild(child);

            // make sure the child is in the same scene
            if (child.scene != scene) {
                if (scene is null) child.leaveScene();
                else child.enterScene(scene);
            }

            child.parent = this;
            _children.Add(child);
        }

        public void removeChild(Gameobject child) {
            // make sure the child actually is a child before removing it
            if (child.parent != this) return;

            child.parent = null;
            _children.Remove(child);
        }

#endregion

#region enter/leave scene and destroy

        public void enterScene(Scene s) {
            if (scene == s) return;
            if (scene is not null) leaveScene();

            for (int i = 0; i < children.Count; i++) children[i].enterScene(s);

            scene = s;
            scene._addGameobject(this);
            
            // notify components that we have entered a scene
            for (int i = 0; i < components.Count; i++) components[i].enter();
        }
        public void leaveScene() {
            if (scene is null) return;

            for (int i = 0; i < children.Count; i++) children[i].leaveScene();

            for (int i = 0; i < components.Count; i++) components[i].leave();

            scene._removeGameobject(this);
            scene = null;
            
        }

        public void destroy() {
            leaveScene();

        }

#endregion



    }

    public abstract class Component {
        public Gameobject gameobject { get; internal set; }
        public Transform transform => gameobject.transform;
        public Scene scene => gameobject.scene;

        internal void enter() {
            scene.update_event += onUpdate;
            onEnter();
        }
        internal void leave() {
            scene.update_event -= onUpdate;
            onLeave();
        }


        protected virtual void onUpdate() {}
        protected virtual void onEnter() {}
        protected virtual void onLeave() {}
        protected virtual void onDestroy() {}


        /* TODO: life hook cycles
            start
            update
            enter
            leave
            destroy
            onCollision

        */

    }

    
}

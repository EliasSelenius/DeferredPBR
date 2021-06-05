using System.Collections.ObjectModel;
using System.Collections.Generic;
using Nums;
using System.Linq;



namespace Engine {
    public class Gameobject {
        public readonly Transform transform = new();
        
        public Scene scene { get; private set; }

        public Gameobject parent { get; private set; }
        public Gameobject rootParent => parent?.rootParent ?? this;

        List<Gameobject> _children = new List<Gameobject>();
        public readonly ReadOnlyCollection<Gameobject> children;


        List<Component> _components = new List<Component>();
        public readonly ReadOnlyCollection<Component> components;

        public bool isRootObject => parent == null;
        public bool isParent => children.Count > 0;
        public bool isChild => parent != null;

        public Gameobject(params Component[] comps) {
            children = _children.AsReadOnly();
            components = _components.AsReadOnly();

            addComponents(comps);
        }


        public void calcModelMatrix(out mat4 m) {
            transform.getMatrix(out m);
            if (parent != null) {
                parent.calcModelMatrix(out mat4 p);
                m *= p;
            }
        }

        public void calcWorldPosition(out vec3 wpos) {
            if (parent == null) {
                wpos = transform.position;
                return;
            }

            var pos = new vec4(transform.position.x, transform.position.y, transform.position.z, 1);
            parent.calcModelMatrix(out mat4 p);
            wpos.x = p.col1.dot(pos);
            wpos.y = p.col2.dot(pos);
            wpos.z = p.col3.dot(pos);
        }

#region add/get/require components 

        private void attachComponent(Component comp) {
            if (comp.gameobject != null) throw new System.Exception("Component is already attached to a object");

            _components.Add(comp);
            comp.gameobject = this;
        }

        public void addComponent(Component comp) {
            attachComponent(comp);
            comp.start();
        }

        public void addComponents(params Component[] comps) {
            foreach (var c in comps) attachComponent(c);
            foreach (var c in comps) c.start();
        }

        public T requireComponent<T>() where T : Component, new() {
            var c = getComponent<T>();
            if (c is null) {
                c = new T();
                addComponent(c);
            }
            return c;
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

        public void disable() {
            if (scene != null) scene.update_event -= onUpdate;
        }

        internal void start() {
            onStart();
        }

        internal void enter() {            
            scene.update_event += onUpdate;
            onEnter();
        }
        internal void leave() {
            scene.update_event -= onUpdate;
            onLeave();
        }

        internal void editorRender() {
            onEditorRender();
        }


        protected virtual void onStart() {}
        protected virtual void onUpdate() {}
        protected virtual void onEnter() {}
        protected virtual void onLeave() {}
        protected virtual void onDestroy() {}
        protected virtual void onEditorRender() {}


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

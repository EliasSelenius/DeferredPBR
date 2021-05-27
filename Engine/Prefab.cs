
using System.Collections.Generic;
using System;

namespace Engine {
    public class Prefab {
        public readonly Transform transform = new();
        readonly List<Comp> components = new();
        readonly List<Prefab> children = new();

        
        static Func<Component> getConstructor<T>() where T : Component, new() => () => new T();

        public void addComponent(Type type, Dictionary<string, object> fields) => components.Add(new Comp(type, fields)); 
        public void addComponent<T>(Dictionary<string, object> fields) => addComponent(typeof(T), fields); 

        public void addChild(Prefab child) => children.Add(child);

        public Gameobject createInstance() {
            Gameobject g = new();
            g.transform.set(transform);
            foreach (var comp in components) g.addComponent(comp.create());
            foreach (var child in children) g.addChild(child.createInstance());
            return g;
        }

        public static Prefab fromInstance(Gameobject gameobject) {
            Prefab p = new();
            p.transform.set(gameobject.transform);
            foreach (var c in gameobject.components) p.addComponent(c.GetType(), new Dictionary<string, object>());
            foreach (var c in gameobject.children) p.addChild(fromInstance(c));
            return p;
        }

        class Comp {
            public readonly Type type;
            public readonly Dictionary<string, object> fields;

            public Comp(Type type, Dictionary<string, object> fields) {
                this.type = type;
                this.fields = fields;
            }

            public Component create() {
                var res = type.GetConstructor(Array.Empty<Type>()).Invoke(null) as Component;
                foreach (var item in fields) {
                    var f = type.GetField(item.Key);
                    if (f == null) throw new Exception("The field \"" + item.Key + "\" does not exist on type \"" + type.Name + "\"");
                    f.SetValue(res, item.Value);
                }
                return res;
            }
        }
    }
}
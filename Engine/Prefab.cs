
using System.Collections.Generic;
using System;

namespace Engine {
    public class Prefab {
        public readonly Transform transform = new();
        readonly List<Comp> components = new();
        readonly List<Prefab> children = new();

        public void addComponent<T>(Dictionary<string, object> fields) => components.Add(new Comp(typeof(T), fields)); 

        public void addChild(Prefab child) => children.Add(child);
        

        public Gameobject createInstance() {
            Gameobject g = new();
            g.transform.set(transform);
            foreach (var comp in components) g.addComponent(comp.create());
            foreach (var child in children) g.addChild(child.createInstance());
            return g;
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
                foreach (var item in fields) type.GetField(item.Key).SetValue(res, item.Value);
                return res;
            }
        }
    }
}
using System.IO;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using Nums;
using System.Xml;

namespace Engine {

    public static class app {
        public static GameWindow window { get; private set; }

        public static void Main() {
            window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            window.Size = (1600, 900);

            window.Load += Assets.load;
            window.Load += Renderer.load;
            window.Load += load;
            window.UpdateFrame += update;
            window.RenderFrame += Renderer.drawframe;
            window.Resize += Renderer.windowResize;
            window.Run();
        }

        static void load() {
            window.VSync = VSyncMode.Off;
            window.CursorGrabbed = true;

            // gen triangle
            /*{
                entity = new Entity();
                entity.transform.position = (0,0,4);
                entity.mesh = new Mesh<Vertex>();
                entity.mesh.vertices.AddRange(new Vertex[] {
                    new Vertex {
                        position = (0, 0.5f, 0),
                    },
                    new Vertex {
                        position = (0.5f, -0.5f, 0),
                    },
                    new Vertex {
                        position = (-0.5f, -0.5f, 0),
                    }
                }); 
                entity.mesh.addTriangles(new uint[] {
                    0, 1, 2
                });
                entity.mesh.bufferdata();
            }
            

            // gen random mesh
            {

                var vs = new Vertex[30];
                for (int i = 0; i < vs.Length; i++) {
                    vs[i] = new Vertex {
                        position = (math.range(-10, 10), math.range(-10, 10),math.range(-10, 10)),
                        //color = (math.range(0, 1), math.range(0, 1),math.range(0, 1))
                    };
                }

                var ind = new uint[30];
                for (int i = 0; i < ind.Length; i++) {
                    ind[i] = (uint)((math.rand() * .5f + .5f) * vs.Length);
                }

                var randomMesh = new Mesh<Vertex>();
                randomMesh.addTriangles(ind);
                randomMesh.vertices.AddRange(vs);
                randomMesh.bufferdata();
                var c = new Entity {
                    mesh = randomMesh
                };
                c.transform.position = (4, 0,0);
                c.transform.scale *= 0.1f;
                entity.addChild(c);
            }*/



        }


        static void update(FrameEventArgs e) {
            Scene.active.update();
        }
    }
}

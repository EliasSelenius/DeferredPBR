using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Xml;
using OpenTK.Graphics.OpenGL4;
using Nums;

namespace Engine {

    public class Scene {

        public static Scene active = new Scene();

        public Skybox skybox;
        public Camera camera = new Camera();

        List<Gameobject> _gameobjects = new List<Gameobject>();
        public readonly ReadOnlyCollection<Gameobject> gameobjects;
        
        public readonly List<Pointlight> pointlights = new List<Pointlight>();
        public readonly List<Dirlight> dirlights = new List<Dirlight>();

        internal readonly List<MeshRenderer> renderers = new List<MeshRenderer>();

        internal event System.Action update_event;

        public Scene() {

            gameobjects = _gameobjects.AsReadOnly();           


            {

                var co = Collada.fromFile("data/models/Ships.dae").toPrefabs();

                foreach (var item in co) {
                    item.Value.createInstance().enterScene(this);
                    //item.Value.transform.position.y = - 60;
                }


                


                { // lights

                    dirlights.Add(new Dirlight {
                        dir = new Nums.vec3(1,1,1).normalized(),
                        color = Nums.vec3.one * 1f
                    });

                    /*
                    dirlights.Add(new Dirlight {
                        color = (0.2f, 0.2f, 1)
                    });

                    dirlights.Add(new Dirlight {
                        dir = new Nums.vec3(0,0,-1),
                        color = (0,1,0)
                    });

                    dirlights.Add(new Dirlight {
                        dir = new Nums.vec3(-1,1,-1).normalized(),
                        color = (1,0.2f,0.2f)
                    });*/

                    pointlights.Add(new Pointlight {
                        position = (3, 3, 3),
                        color = (10, 10, 10)
                    });

        /*
                    pointlights.Add(new Pointlight {
                        position = (6, 6, 3),
                        color = (3, 30, 3)
                    });*/
                }

                { // textured plane
                    var m = MeshFactory<Vertex>.genPlane(10, 10f, 10000);

                    var g = new Gameobject();
                    g.addComponent(new MeshRenderer {
                        mesh = m,
                        materials = new[] { new PBRMaterial {
                            albedoMap = Assets.getTexture2D("mossyBrick.png"),
                            albedo = 1f,
                            roughness = 0.5f,
                            //metallic = 1f
                    } }
                    });

                    g.transform.position.z += 20;
                    g.transform.scale.xz *= 2000;

                    g.enterScene(this);

                }

                { // material spectrum
                    var mesh = MeshFactory<Vertex>.genSphere(100, 1f);

                    mesh.mutate((v, i) => {
                        v.position += v.position * Nums.math.range(0, 0.01f);
                        return v;
                    });
                    mesh.genNormals();
                    //mesh.bufferdata();

                    for (float r = 0; r <= 1f; r += 0.1f) {
                        for (float m = 0; m <= 1f; m += 0.1f) {
                            var mat = new PBRMaterial {
                                albedo = Nums.vec3.one,
                                roughness = r,
                                metallic = m
                            };
                            Gameobject g = new();
                            g.addComponent(new MeshRenderer {
                                mesh = mesh,
                                materials = new[] { mat }
                            });

                            g.transform.position.xy = new Nums.vec2(r, m) * 25;
                            g.enterScene(this);
                        }
                    }
                }

            
                { // planet
                    var mesh = MeshFactory<Vertex>.genSphere(100, 1f);
                    mesh.mutate((v, i) => {
                        var p = v.position;
                        v.position *= 100f;

                        for (int o = 1; o <= 4; o++) {
                            v.position += p * Nums.math.gradnoise(new Nums.vec3(o,o,o) * 10f + p * 5f * o) * 10f / o;
                        }

                        return v;
                    });
                    //mesh.flipIndices();
                    mesh.genNormals();
                    mesh.bufferdata();


                    Gameobject planet = new();
                    planet.addComponent(new MeshRenderer {
                        mesh = mesh,
                        materials = new[] { 
                            new PBRMaterial { 
                                roughness = 0.5f,
                                metallic = 1f,
                                albedo = 1f
                            }
                        }
                    });
                    planet.transform.position = (0, 0, 340);

                    planet.enterScene(this);
                }

                { // quad

                    var g = new Gameobject();
                    g.addComponent(new MeshRenderer {
                        mesh = MeshFactory<Vertex>.genQuad(),
                        materials = new[] { PBRMaterial.defaultMaterial }
                    });

                    g.transform.position.yz = (4, 10);
                    g.enterScene(this);
                }
            
                { // prefab test

                    var prefab = new Prefab();
                    prefab.transform.position = 4;
                    prefab.addComponent<MeshRenderer>(new Dictionary<string, object> { 
                        { "mesh", MeshFactory<Vertex>.genCube(1, 1f) },
                        { "materials", new[] { PBRMaterial.defaultMaterial }}
                    });

                    prefab.createInstance().enterScene(this);

                }

            }
        }

        internal void _addGameobject(Gameobject obj) => _gameobjects.Add(obj);
        internal void _removeGameobject(Gameobject obj) => _gameobjects.Remove(obj);

        internal void renderGeometry() {
            camera.updateUniforms();
            foreach (var r in renderers) r.render();
        }

        internal void renderLights() {
            Renderer.lightPass_dirlight.use();
            foreach (var light in dirlights) {
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightDir"), light.dir.x, light.dir.y, light.dir.z);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_dirlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                Lights.dirlightMesh.render();
            }
            

            Renderer.lightPass_pointlight.use();
            foreach (var light in pointlights) {

                vec3 v = (camera.viewMatrix.transpose * new vec4(light.position.x, light.position.y, light.position.z, 1.0f)).xyz;
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightPosition"), 1, ref v.x);
                GL.Uniform3(GL.GetUniformLocation(Renderer.lightPass_pointlight.id, "lightColor"), light.color.x, light.color.y, light.color.z);
                var m = light.calcModelMatrix();
                GLUtils.setUniformMatrix4(Renderer.lightPass_pointlight.id, "model", ref m);
                Lights.pointlightMesh.render();
            }
        }

        internal void update() {
            camera.move();
            //pointlights[0].position = camera.transform.position + camera.transform.forward;
            pointlights[0].position = new vec3(math.sin((float)Renderer.time), 0.1f, math.cos((float)Renderer.time)) * 10;
            //entities[0].children[3].transform.rotate(Nums.vec3.unity, 0.01f);

            update_event();
        }
    }
}
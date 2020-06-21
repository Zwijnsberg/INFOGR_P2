using System.Collections.Generic;
using OpenTK;
using Template;

// using meshes

namespace INFOGR2019Tmpl8
{
    class SceneGraph
    {
        public List<SceneGraph> children = new List<SceneGraph>();
        public Mesh me; 
        public Mesh parent;

        public SceneGraph(Mesh Me, Mesh Parent, Mesh[] Children, Matrix4 camera)
        {
            me = Me;
            parent = Parent;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    SceneGraph child = new SceneGraph(Children[i], me, null, camera);
                    children.Add(child);
                }
            }

            Render(camera);
        }

        public void Render(Matrix4 cam)
        {

        }
    }
}

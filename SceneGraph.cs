using System.Collections.Generic;
using System;
using OpenTK;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Template;


// using meshes

namespace INFOGR2019Tmpl8
{
    class SceneGraph
    {
		// store the children and the parent of this current mesh

        public List<SceneGraph> children = new List<SceneGraph>();
        public Mesh me; 
        public Mesh parent;

		// transform matrix of this mesh
		public Matrix4 transformX;


		// constructor of scene graph, defining the mesh and its parent and children
		public SceneGraph(Mesh Me)
        {
            me = Me;
            parent = me.parent;
			

            if (me.kids != null)
            {
                for (int i = 0; i < me.kids.Length; i++)
                {
                    SceneGraph child = new SceneGraph(me.kids[i]);
                    children.Add(child);
                }
            }
        }


		// render the mesh using the supplied shader and matrix
		public void Render(Matrix4 camera)
		{

			Matrix4 transform = transformX;
			me.Render(me.shader, transform * camera, transform, me.texture);		

			// call render function of the children here?
			foreach (SceneGraph child in children)
            {
				//child.me.modelViewMatrix *= me.modelViewMatrix; 
				child.Render(transform * camera);
            }
		}

		
		// layout of a single vertex
		[StructLayout( LayoutKind.Sequential )]
		public struct ObjVertex
		{
			public Vector2 TexCoord;
			public Vector3 Normal;
			public Vector3 Vertex;
		}

	}
}

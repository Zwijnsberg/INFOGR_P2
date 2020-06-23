using System.Collections.Generic;
using System;
using OpenTK;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Template;
using System.Drawing.Drawing2D;

// using meshes

namespace INFOGR2019Tmpl8
{
    class SceneGraph
    {
        public List<SceneGraph> children = new List<SceneGraph>();
        public Mesh me; 
        public Mesh parent;
		public Vector3 pos;
		public float rotation;
		public Vector3 scale;

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

		// initialization; called during first render
		

		// render the mesh using the supplied shader and matrix
		public void Render(Matrix4 camera)
		{
			Matrix4 transform = Matrix4.CreateTranslation(pos) * Matrix4.CreateRotationY(rotation) * Matrix4.CreateScale(scale);
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

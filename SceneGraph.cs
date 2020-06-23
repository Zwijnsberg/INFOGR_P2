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
        public List<SceneGraph> children = new List<SceneGraph>();
        public Mesh me; 
        public Mesh parent;

		public Matrix4 scale;
		public Matrix4 rotation;
		public Matrix4 position;


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

		public Matrix4 Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		public Matrix4 Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}

		public Matrix4 Position
		{
			get { return position; }
			set { position = value; }
		}
		// initialization; called during first render


		// render the mesh using the supplied shader and matrix
		public void Render(Matrix4 camera)
		{
			Mesh mesh = me;
			Shader shader = mesh.shader;
			Matrix4 transform = mesh.modelViewMatrix; // *camera;
			Matrix4 toWorld = mesh.toWorld;
			Texture texture = mesh.texture;

			//REMOVE: me.Render(shader, transform, toWorld, texture);

			// on first run, prepare buffers
			me.Prepare(shader);

			// safety dance
			GL.PushClientAttrib(ClientAttribMask.ClientVertexArrayBit);

			// enable texture
			int texLoc = GL.GetUniformLocation(shader.programID, "pixels");
			GL.Uniform1(texLoc, 0);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, texture.id);

			// enable shader
			GL.UseProgram(shader.programID);

			// pass transform to vertex shader
			GL.UniformMatrix4(shader.uniform_mview, false, ref transform);
			GL.UniformMatrix4(shader.uniform_2wrld, false, ref toWorld);

			// enable position, normal and uv attributes
			GL.EnableVertexAttribArray(shader.attribute_vpos);
			GL.EnableVertexAttribArray(shader.attribute_vnrm);
			GL.EnableVertexAttribArray(shader.attribute_vuvs);

			// bind interleaved vertex data
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.BindBuffer(BufferTarget.ArrayBuffer, me.vertexBufferId);
			GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, Marshal.SizeOf(typeof(ObjVertex)), IntPtr.Zero);

			// link vertex attributes to shader parameters 
			GL.VertexAttribPointer(shader.attribute_vuvs, 2, VertexAttribPointerType.Float, false, 32, 0);
			GL.VertexAttribPointer(shader.attribute_vnrm, 3, VertexAttribPointerType.Float, true, 32, 2 * 4);
			GL.VertexAttribPointer(shader.attribute_vpos, 3, VertexAttribPointerType.Float, false, 32, 5 * 4);

			// bind triangle index data and render
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, me.triangleBufferId);
			GL.DrawArrays(PrimitiveType.Triangles, 0, me.triangles.Length * 3);

			// bind quad index data and render
			if (me.quads.Length > 0)
			{
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, me.quadBufferId);
				GL.DrawArrays(PrimitiveType.Quads, 0, me.quads.Length * 4);
			}

			// restore previous OpenGL state
			GL.UseProgram(0);
			GL.PopClientAttrib();
			

			// call render function of the children here?
			foreach (SceneGraph child in children)
            {
				//child.me.modelViewMatrix *= me.modelViewMatrix; 
				child.Render(camera);
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

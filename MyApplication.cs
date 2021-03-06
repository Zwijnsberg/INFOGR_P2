using System.Diagnostics;
using INFOGR2019Tmpl8;
using OpenTK;
//using OpenTK.Graphics.ES10;
using OpenTK.Graphics.OpenGL;

namespace Template
{
	class Light
	{
		public int lightID;
		public Shader shader;

		public Light(int lightID, Shader shader)
		{
			this.lightID = lightID;
			this.shader = shader;
		}

		public void setLight(float x, float y, float z)
		{
			GL.UseProgram(shader.programID);
			GL.Uniform3(lightID, x, y, z);
		}
	}

	class MyApplication
	{
		// member variables
		public Surface screen;                  // background surface for printing etc.
		Mesh car, flag, floor;                       // a mesh to draw using OpenGL
		const float PI = 3.1415926535f;         // PI
		float a = 0;
		float b = 0;
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood;                           // texture to use for rendering
		Texture silver;
		Texture dark;
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		bool useRenderTarget = true;

		// initialize
		public void Init()
		{
			// create shaders
			shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
			postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
			// load a texture
			wood = new Texture("../../assets/wood.jpg");
			silver = new Texture("../../assets/silver3.jpg");
			dark = new Texture("../../assets/dark4.jpg");

			// load car and flag
			flag = new Mesh("../../assets/teapot.obj", car, shader, dark);
			car = new Mesh("../../assets/db8.obj", floor, shader, silver );
			floor = new Mesh("../../assets/floor.obj", null, shader, wood );

			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
			//set the light
			Light light1 = new Light(GL.GetUniformLocation(shader.programID, "lightPos"), shader);
			light1.setLight(3, 3, 3);
			//set the ambient light color
			int ambientID = GL.GetUniformLocation(shader.programID, "ambientColor");
			GL.UseProgram(shader.programID);
			GL.Uniform3(ambientID, 0.5f, 0.4f, 0.3f);

		}

		// tick for background surface
		public void Tick()
		{
			screen.Clear( 0 );
			screen.Print( "hello world", 2, 2, 0xffff00 );
		}

		// tick for OpenGL rendering code
		public void RenderGL()
		{
			// measure frame duration
			float frameDuration = timer.ElapsedMilliseconds;
			timer.Reset();
			timer.Start();

			// prepare matrix for vertex shader
			float angle90degrees = PI / 2;
			Vector3 cameraPos = new Vector3(0, 50f, 0);
			int cameraID = GL.GetUniformLocation(shader.programID, "cameraPos");
			GL.UseProgram(shader.programID);
			GL.Uniform3(cameraID, cameraPos);

			Matrix4 Tcar = Matrix4.CreateScale(0.5f) * Matrix4.CreateRotationY(10f) * Matrix4.CreateTranslation(new Vector3(0, 0, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 9 + 0) * Matrix4.CreateTranslation(new Vector3(0, 0, 0));
			Matrix4 Tflag = Matrix4.CreateScale(0.08f) * Tcar * Matrix4.CreateTranslation(new Vector3(0, 2, 0));
			Matrix4 toWorld = Tflag;
			Matrix4 Tfloor = Matrix4.CreateScale( 4.0f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), 0 );
			Matrix4 Tcamera = Matrix4.CreateTranslation( -cameraPos ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);

			// defining the model view matrix for the mesh object
			flag.modelViewMatrix = Tflag * Tcamera * Tview;
			flag.toWorld = toWorld;
			car.modelViewMatrix = Tcar * Tcamera * Tview ;
			car.toWorld = toWorld;
			floor.modelViewMatrix = Tfloor * Tcamera * Tview;
			floor.toWorld = toWorld;

			SceneGraph floor_sg = new SceneGraph(floor);
			floor_sg.pos = new Vector3(0, 0, 0);
			floor_sg.rotation = a;
			floor_sg.scale = new Vector3(4, 2, 2);

			// update rotation
			a += 0.0005f * frameDuration;
			if ( a > 2 * PI ) a -= 2 * PI;

			b = a * a;
			if (a > 6 * PI) a -= 10 * PI;

			if ( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene via Scenegraph
				floor_sg.Render(Tcamera * Tview);


				// render scene to render target
				/*
				flag.Render(shader, Tflag * Tcamera * Tview, toWorld, dark);
				car.Render( shader, Tcar*Tcamera*Tview, toWorld, silver);
				floor.Render( shader, Tfloor * Tcamera * Tview, toWorld, wood ); */


				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene via SceneGraph
				floor_sg.Render(Tcamera);

				// render scene directly to the 
				/*
				flag.Render(shader, Tflag * Tcamera * Tview, toWorld, dark);
				car.Render(shader, Tcar * Tcamera * Tview, toWorld, silver);
				floor.Render(shader, Tfloor * Tcamera * Tview, toWorld, wood); */
			}
		}
	}
}
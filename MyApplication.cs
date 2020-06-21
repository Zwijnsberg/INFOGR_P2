using System.Diagnostics;
using INFOGR2019Tmpl8;
using OpenTK;
//using OpenTK.Graphics.ES10;
using OpenTK.Graphics.OpenGL;

namespace Template
{
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
			// load car and flag
			flag = new Mesh("../../assets/teapot.obj");
			car = new Mesh( "../../assets/db8.obj" );
			floor = new Mesh( "../../assets/floor.obj" );
			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create shaders
			shader = new Shader( "../../shaders/vs.glsl", "../../shaders/fs.glsl" );
			postproc = new Shader( "../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl" );
			// load a texture
			wood = new Texture( "../../assets/wood.jpg" );
			silver = new Texture("../../assets/silver3.jpg");
			dark = new Texture("../../assets/dark4.jpg");
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
			//set the light
			int lightID = GL.GetUniformLocation(shader.programID, "lightPos");
			GL.UseProgram(shader.programID);
			GL.Uniform3(lightID, 0.0f, 10.0f, 0.0f);
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

			Matrix4 Tcar = Matrix4.CreateScale(0.5f) * Matrix4.CreateRotationY(10f) * Matrix4.CreateTranslation(new Vector3(a, 0, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 9 + a) * Matrix4.CreateTranslation(new Vector3(0, 0, a));
			Matrix4 Tflag = Matrix4.CreateScale(0.08f) * Tcar * Matrix4.CreateTranslation(new Vector3(0, 2, 0));
			Matrix4 toWorld = Tflag;
			Matrix4 Tfloor = Matrix4.CreateScale( 4.0f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), a );
			Matrix4 Tcamera = Matrix4.CreateTranslation( new Vector3( 0, -14.5f, 0 ) ) * Matrix4.CreateFromAxisAngle( new Vector3( 1, 0, 0 ), angle90degrees );
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);

			// defining the model view matrix for the mesh object
			flag.modelViewMatrix = Tflag * Tcamera * Tview;
			car.modelViewMatrix = Tcar * Tcamera * Tview ;
			floor.modelViewMatrix = Tfloor * Tcamera * Tview;

			Mesh[] carChildren = {flag};

			SceneGraph carSg = new SceneGraph(null, car, carChildren, Tcamera);

			// update rotation
			a += 0.0005f * frameDuration;
			if( a > 2 * PI ) a -= 2 * PI;

			b = a * a;
			if (a > 6 * PI) a -= 10 * PI;

			if ( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene to render target
				flag.Render(shader, Tflag * Tcamera * Tview, toWorld, dark);
				car.Render( shader, Tcar*Tcamera*Tview, toWorld, silver);
				floor.Render( shader, Tfloor * Tcamera * Tview, toWorld, wood );


				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene directly to the screen
				flag.Render(shader, Tflag * Tcamera * Tview, toWorld, dark);
				car.Render(shader, Tcar * Tcamera * Tview, toWorld, silver);
				floor.Render(shader, Tfloor * Tcamera * Tview, toWorld, wood);
			}
		}
	}
}
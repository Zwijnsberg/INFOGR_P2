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
		Stopwatch timer;                        // timer for measuring frame duration
		Shader shader;                          // shader to use for rendering
		Shader postproc;                        // shader to use for post processing
		Texture wood;                           // texture to use for rendering
		Texture silver;
		Texture dark;
		RenderTarget target;                    // intermediate render target
		ScreenQuad quad;                        // screen filling quad for post processing
		bool useRenderTarget = true;

		// all the global variables for moving the camera, controlled from the template class
		public static float moveX;                   
		public static float moveY;
		public static float moveZ;
		public static float rotate;

		// initialize
		public void Init()
		{
			moveX = 0;
			moveY = 0;
			moveZ = -14.5f;
			rotate = 0;


			// create shaders
			shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
			postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
			// load a texture
			wood = new Texture("../../assets/track1.png");
			silver = new Texture("../../assets/silver3.jpg");
			dark = new Texture("../../assets/dark4.jpg");

			// load car and flag
			flag = new Mesh("../../assets/propeller1.obj", car, null, shader, dark);
			car = new Mesh("../../assets/db8.obj", floor, new Mesh[] { flag }, shader, silver );
			floor = new Mesh("../../assets/floor.obj", null, null, shader, wood );

			// initialize stopwatch
			timer = new Stopwatch();
			timer.Reset();
			timer.Start();
			// create the render target
			target = new RenderTarget( screen.width, screen.height );
			quad = new ScreenQuad();
			//set the light
			int lightID = GL.GetUniformLocation(shader.programID, "lightPos");
			GL.UseProgram(shader.programID);
			GL.Uniform3(lightID, 0.0f, 10.0f, 0.0f);
			//set the ambient light color
			int ambientID = GL.GetUniformLocation(shader.programID, "ambientColor");
			GL.UseProgram(shader.programID);
			GL.Uniform3(ambientID, 0.8f, 0.2f, 0.3f);

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

			Matrix4 Tcamera = Matrix4.CreateTranslation(new Vector3(moveX, moveZ, moveY)) * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), angle90degrees);
			Matrix4 Tview = Matrix4.CreatePerspectiveFieldOfView(1.2f, 1.3f, .1f, 1000);

			// defining the model view matrix for the mesh object
			Matrix4 Tcar = Matrix4.CreateScale(0.5f) * Matrix4.CreateRotationY(10f) * Matrix4.CreateTranslation(new Vector3(a, 0, 0)) * Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), 9 + a +rotate) * Matrix4.CreateTranslation(new Vector3(0, 0, a));
			Matrix4 turn = Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), a*3);
			car.modelViewMatrix = Tcar * Tcamera * Tview;

			Matrix4 Tflag = Matrix4.CreateFromAxisAngle(new Vector3(1, 1, 1), 2f) * Matrix4.CreateScale(0.002f)* Matrix4.CreateTranslation(new Vector3(0, 0, -2.4f)) * turn * Tcar * Matrix4.CreateTranslation(new Vector3(0, 1.35f, 0));
			flag.modelViewMatrix = Tflag * Tcamera * Tview;
			flag.toWorld = Tflag;
			car.toWorld = Tflag;

			Matrix4 Tfloor = Matrix4.CreateScale( 3.5f ) * Matrix4.CreateFromAxisAngle( new Vector3( 0, 1, 0 ), rotate );
			floor.modelViewMatrix = Tfloor * Tcamera * Tview;
			floor.toWorld = Tflag;

			Matrix4 toWorld = Tflag;

			// Defining the Scene Graphs
			SceneGraph floor_sg = new SceneGraph(floor);

			SceneGraph car_sg = new SceneGraph(car);

			// update rotation
			a += 0.0012f * frameDuration;
			if( a > 5 * PI ) a -= 2 * PI;


			if (rotate > 2 * PI) rotate -= 2 * PI;


			if ( useRenderTarget )
			{
				// enable render target
				target.Bind();

				// render scene via Scenegraph
				floor_sg.Render(Tcamera);
				car_sg.Render(Tcamera);

				// render quad
				target.Unbind();
				quad.Render( postproc, target.GetTextureID() );
			}
			else
			{
				// render scene via SceneGraph
				floor_sg.Render(Tcamera);
				car_sg.Render(Tcamera);

			}
		}
	}
}
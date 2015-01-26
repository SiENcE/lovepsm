/* 
 * LovePSM is a proof of concept reimplementation of the LOVE2D runtime for Sony PSM (Playstation Vita SDK)
 */
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Imaging;	// Font
using Sce.PlayStation.Core.Input;
using Sce.PlayStation.Core.Audio;

using Sce.PlayStation.HighLevel.GameEngine2D;
using Sce.PlayStation.HighLevel.GameEngine2D.Base;

using LuaInterface;

namespace LovePSM
{

/**
 * LuaSample
 */
public static class Graphics
{
    static bool loop = true;
    static Lua lua;
	static Scene scene;
	static Vector4 setColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
	static Vector4 setBackgroundColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

    public static void Main(string[] args)
    {
		Timer.Init();
        scene = Init();
		lua.GetFunction("love.load").Call();

        while (loop) {
			Sce.PlayStation.Core.Environment.SystemEvents.CheckEvents();
			// it is not needed but you can set external input data if you want
//			List<TouchData> touch_data_list = Touch.GetData(0);
//			Input2.Touch.SetData( 0, touch_data_list );

//			GamePadData pad_data = GamePad.GetData(0);
//			Input2.GamePad.SetData( 0, pad_data );
			var gamePadData = GamePad.GetData(0);
			if ((gamePadData.ButtonsDown & GamePadButtons.Start) != 0) {
				lua.GetFunction("love.keypressed").Call( "return" );
			}

			lua.GetFunction("love.update").Call(Timer.DeltaTime);
			Director.Instance.Update();
				

			lua.GetFunction("love.draw").Call();

			Timer.StartFrame();
			Director.Instance.Render();
			Director.Instance.GL.Context.SwapBuffers();
			Director.Instance.PostSwap(); // you must call this after SwapBuffers

			scene.RemoveAllChildren(false);

			Timer.EndFrame();
        }
			
		Director.Terminate();

		System.Console.WriteLine( "Bye!" );
        Term();
    }

    public static Scene Init()
    {
		// create our own context
		Sce.PlayStation.Core.Graphics.GraphicsContext context = new Sce.PlayStation.Core.Graphics.GraphicsContext();

		// maximum number of sprites you intend to use (not including particles)
		uint sprites_capacity = 500;
		
		// maximum number of vertices that can be used in debug draws
		uint draw_helpers_capacity = 400;

		// initialize GameEngine2D's singletons, passing context from outside
		Director.Initialize( sprites_capacity, draw_helpers_capacity, context );
			
		Director.Instance.GL.Context.SetClearColor( setBackgroundColor );

		// set debug flags that display rulers to debug coordinates
//		Director.Instance.DebugFlags |= DebugFlags.DrawGrid;
		// set the camera navigation debug flag (press left alt + mouse to navigate in 2d space)
		Director.Instance.DebugFlags |= DebugFlags.Navigate; 

		// create a new scene
		var scene = new Scene();

		// set the camera so that the part of the word we see on screen matches in screen coordinates
		scene.Camera.SetViewFromViewport();

		// handle the loop ourself
		Director.Instance.RunWithScene( scene, true );

        // Set up LuaInterface
        lua = new Lua();
		lua.NewTable("love");
		((LuaTable) lua["love"])["load"] = null;
		((LuaTable) lua["love"])["draw"] = null;
		((LuaTable) lua["love"])["update"] = null;
		((LuaTable) lua["love"])["quit"] = null;

        // Register C# keyboard functions
		lua.NewTable("love.keyboard");
		lua.RegisterFunction("love.keyboard.isDown", null, typeof(Graphics).GetMethod("love_keyboard_isDown"));
		// NEW API ONLY FOR THIS PORT
		lua.NewTable("love.touch");
        // Register C# function
		lua.RegisterFunction("love.touch.getPos", null, typeof(Graphics).GetMethod("love_touch_getpos"));
		
		// Register C# audio functions
		lua.NewTable("love.audio");

        // Register C# graphic functions
		lua.NewTable("love.graphics");
		lua.RegisterFunction("love.graphics.setCaption", null, typeof(Graphics).GetMethod("love_graphics_setCaption"));
        lua.RegisterFunction("love.graphics.newImage", null, typeof(Graphics).GetMethod("love_graphics_newImage"));
		lua.RegisterFunction("love.graphics.setColor", null, typeof(Graphics).GetMethod("love_graphics_setColor"));
		lua.RegisterFunction("love.graphics.setBackgroundColor", null, typeof(Graphics).GetMethod("love_graphics_setBackgroundColor"));
		lua.RegisterFunction("love.graphics.rectangle", null, typeof(Graphics).GetMethod("love_graphics_rectangle"));
		lua.RegisterFunction("love.graphics.newQuad", null, typeof(Graphics).GetMethod("love_graphics_newQuad"));
        lua.RegisterFunction("love.graphics.draw", null, typeof(Graphics).GetMethod("love_graphics_draw", new Type[]{typeof(TextureInfo), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float)} ));
		lua.RegisterFunction("love.graphics.drawq", null, typeof(Graphics).GetMethod("love_graphics_drawq"));
			
		// Read Lua file
        lua.DoFile("/Application/assets/main.lua");

		return scene;
    }

    public static void Term()
    {
        // Close LuaInterface
        lua.Dispose();
		Timer.Term();
    }
		
	public static Vector2 love_touch_getpos()
	{
		if ( Input2.Touch00.Down )
		{
			Vector2 position = scene.Camera.GetTouchPos();
			position.Y = 544-position.Y;
			return position;
/*
			Vector2 position = scene.Camera.GetTouchPos();
			Console.WriteLine("x:" + position.X + " y:" + position.Y);
				
			Bounds2 left_up  = new Bounds2( new Vector2(  0,0),  new Vector2(286,160) );
			Bounds2 up       = new Bounds2( new Vector2(287,0),  new Vector2(360,160) );
			Bounds2 right_up = new Bounds2( new Vector2(648,0),  new Vector2(312,160) );
	
			Bounds2 left  = new Bounds2( new Vector2(  0,161),  new Vector2(288,220) );
			Bounds2 right = new Bounds2( new Vector2(649,161),  new Vector2(311,220) );
				
			Bounds2 left_down  = new Bounds2( new Vector2(  0,380),  new Vector2(288,163) );
			Bounds2 down       = new Bounds2( new Vector2(289,380),  new Vector2(360,163) );
			Bounds2 right_down = new Bounds2( new Vector2(649,380),  new Vector2(311,163) );
*/
		}

		return new Vector2(0,0);
	}

	public static Boolean love_keyboard_isDown( String key )
	{
		Console.WriteLine(key);
	
		if (key == "left")
			if (Input2.GamePad0.Left.Down)
				return true;
		if (key == "right")
			if (Input2.GamePad0.Right.Down)
				return true;
		if (key == "up")
			if (Input2.GamePad0.Up.Down)
				return true;
		if (key == "down")
			if (Input2.GamePad0.Down.Down)
				return true;

		return false;
	}

	// Graphics
	public static void love_graphics_setCaption( String caption )
	{
		Console.WriteLine("setCaption is not implemented yet!");
	}
		
	public static void love_graphics_drawq( TextureInfo drawable, String quad, float x, float y, float r, float sx, float sy, float ox, float oy )
	{
		Console.WriteLine("drawq is not implemented yet!");
	}
		
	public static void love_graphics_newQuad( float x, float y, float width, float height, float sw, float sh )
	{
		Console.WriteLine("newQuad is not implemented yet!");
	}

	public static void love_graphics_rectangle( String mode, float x, float y, float width, float height )
	{
		Console.WriteLine("love_graphics_rectangle... " + mode + " is not implemented yet!");
/*
		scene.AdHocDraw += () => 
		{
			Director.Instance.DrawHelpers.DrawBounds2Fill( new Bounds2(new Vector2(x, y), new Vector2(width, height)) );
		};
*/
	}

	public static TextureInfo love_graphics_newImage( String filename )
	{
		Console.WriteLine("love_graphics_newImage... " + filename);
		// create a new TextureInfo object, used by sprite primitives
		var texture_info = new TextureInfo( new Texture2D("/Application/assets/" + filename, false ) );
		return texture_info;
	}
		
	public static void love_graphics_setBackgroundColor(float r, float g, float b)
	{
		setBackgroundColor = new Vector4(r/ 255.0f, g/ 255.0f, b/ 255.0f, 1.0f);
		Director.Instance.GL.Context.SetClearColor( setBackgroundColor );
	}
	
	public static void love_graphics_setColor(float r, float g, float b, float a)
	{
		setColor = new Vector4(r/255.0f, g/255.0f, b/255.0f, a/255.0f);
	}

	public static void love_graphics_draw( TextureInfo drawable, float x, float y, float r, float sx, float sy, float ox, float oy )
	{
		// create a new sprite
		var sprite = new SpriteUV() { TextureInfo = drawable};

		// make the texture 1:1 on screen
		sprite.Quad.S = drawable.TextureSizef; 

		// center the sprite around its own .Position 
		// (by default .Position is the lower left bit of the sprite)
		sprite.CenterSprite( TRS.Local.TopLeft );

		// our scene only has 2 nodes: scene->sprite
		scene.AddChild( sprite );

		sprite.Position = new Vector2(x-ox,544-y+oy);
		sprite.Pivot = new Vector2(ox,-oy);
		sprite.Scale = new Vector2(sx,sy);
		sprite.Rotate(-r);
		sprite.BlendMode = BlendMode.Normal;
		sprite.Color = setColor;
	}
}

} // Sample

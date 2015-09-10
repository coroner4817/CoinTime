using System;
using CocosSharp;
using CoinTimeGame.Scenes;


namespace CoinTime
{
	public class GameAppDelegate : CCApplicationDelegate
	{
		static CCDirector director;
		static CCWindow mainWindow;

		//游戏运行时第一个运行这个
		public override void ApplicationDidFinishLaunching (CCApplication application, CCWindow mainWindow)
		{
			GameAppDelegate.mainWindow = mainWindow;

			//CCDirector是处理scene这一级别的
			//导演
			director = new CCDirector ();

			//手机游戏开发时，要把PreferMultiSampling设为false
			//因为手机处理器不够强大
			//这个抗锯齿算法很费cpu
			application.PreferMultiSampling = false;

			//asset的读取地址
			application.ContentRootDirectory = "Content";
			application.ContentSearchPaths.Add ("animations");
			application.ContentSearchPaths.Add ("fonts");
			application.ContentSearchPaths.Add ("images");
			application.ContentSearchPaths.Add ("levels");
			application.ContentSearchPaths.Add ("sounds");

			CCSize windowSize = mainWindow.WindowSizeInPixels;

			// Use the SNES resolution:
			//设置游戏画面的大小
			float desiredWidth = 256.0f;
			float desiredHeight = 224.0f;
			CCScene.SetDefaultDesignResolution (desiredWidth, desiredHeight, CCSceneResolutionPolicy.ShowAll);
            
			//加入导演
			mainWindow.AddSceneDirector (director);

			//导演引入第一个scene
			var scene = new LevelSelectScene (mainWindow);
			director.RunWithScene (scene);
		}

		//返回键把游戏最小化
		public override void ApplicationDidEnterBackground (CCApplication application)
		{
			application.Paused = true;
		}

		public override void ApplicationWillEnterForeground (CCApplication application)
		{
			application.Paused = false;
		}

		// for this game (with only 2 scenes) we're just going to handle moving between them here
		//去不同的场景
		public static void GoToGameScene()
		{
			var scene = new GameScene (mainWindow);
			director.ReplaceScene (scene);
		}

		public static void GoToLevelSelectScene()
		{
			var scene = new LevelSelectScene (mainWindow);
			director.ReplaceScene (scene);
		}

	}
}

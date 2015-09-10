using System;
using CocosSharp;
using Microsoft.Xna.Framework;
using CoinTimeGame.Entities;
using System.Collections.Generic;

namespace CoinTimeGame.Scenes
{
	public class LevelSelectScene : CCScene
	{
		//default value=0
		int pageNumber;

		CCLayer mainLayer;

		//all element use CCSprite
		CCSprite background;
		CCSprite logo;

		//custon button
		Button navigateLeftButton;
		Button navigateRightButton;

		List<Button> levelButtons = new List<Button> ();

		public LevelSelectScene (CCWindow mainWindow) : base(mainWindow)
		{
			CreateLayers ();

			CreateBackground ();

			CreateLogo ();

			CreateLevelButtons ();

			CreateNavigationButtons ();
		}

		private void CreateBackground()
		{
			background = new CCSprite ("ui/background.png");
			background.PositionX = ContentSize.Center.X;
			background.PositionY = ContentSize.Center.Y;
			background.IsAntialiased = false;
			mainLayer.AddChild (background);
		}


		private void CreateLogo()
		{
			background = new CCSprite ("ui/logo.png");
			background.PositionX = ContentSize.Center.X;
			const float offsetFromMiddle = 72;
			background.PositionY = ContentSize.Center.Y + offsetFromMiddle;
			background.IsAntialiased = false;
			mainLayer.AddChild (background);
		}

		private void CreateNavigationButtons()
		{
			const float horizontalDistanceFromEdge = 36;
			const float verticalDistanceFromEdge = 28;

			navigateLeftButton = new Button (mainLayer);
			navigateLeftButton.ButtonStyle = ButtonStyle.LeftArrow;
			navigateLeftButton.PositionX = horizontalDistanceFromEdge;
			navigateLeftButton.PositionY = verticalDistanceFromEdge;
			navigateLeftButton.Name = "NavigateLeftButton";
			navigateLeftButton.Clicked += HandleNavigateLeft;
			mainLayer.AddChild(navigateLeftButton);

			navigateRightButton = new Button (mainLayer);
			navigateRightButton.ButtonStyle = ButtonStyle.RightArrow;
			navigateRightButton.PositionX = ContentSize.Width - horizontalDistanceFromEdge;
			navigateRightButton.PositionY = verticalDistanceFromEdge;
			navigateRightButton.Name = "NavigateLeftButton";
			navigateRightButton.Clicked += HandleNavigateRight;

			mainLayer.AddChild(navigateRightButton);

			UpdateNavigationButtonVisibility ();
		}

		private void UpdateNavigationButtonVisibility ()
		{
			//一共两页，页码大于0时左返回
			navigateLeftButton.Visible = pageNumber > 0;
			//当下一页还有页码时右返回
			navigateRightButton.Visible = (1+pageNumber) * 6 < LevelManager.Self.NumberOfLevels;
		}


		private void HandleNavigateLeft(object sender, EventArgs args)
		{
			pageNumber--;
			UpdateNavigationButtonVisibility ();

			DestroyLevelButtons ();
			CreateLevelButtons ();
		}


		private void HandleNavigateRight(object sender, EventArgs args)
		{
			pageNumber++;
			UpdateNavigationButtonVisibility ();

			DestroyLevelButtons ();
			CreateLevelButtons ();
		}


		private void CreateLayers()
		{
			mainLayer = new CCLayer ();
			this.AddChild (mainLayer);

		}

		private void CreateLevelButtons()
		{
			const int buttonsPerPage = 6;
			//得到本页第一个button的序号
			int levelIndex0Based = buttonsPerPage * pageNumber;

			//本页显示的最后一个button的序号
			int maxLevelExclusive = System.Math.Min (levelIndex0Based + 6, LevelManager.Self.NumberOfLevels);
			int buttonIndex = 0;

			float centerX = this.ContentSize.Center.X;
			const float topRowOffsetFromCenter = 16;
			float topRowY = this.ContentSize.Center.Y + topRowOffsetFromCenter;
			const float spacing = 54;

			for (int i = levelIndex0Based; i < maxLevelExclusive; i++)
			{
				var button = new Button (mainLayer);

				// Make it 1-based for non-programmers
				button.LevelNumber = i + 1;

				button.ButtonStyle = ButtonStyle.LevelSelect;

				//因为是横屏，所以坐标的原点在横屏的左下角
				button.PositionX = centerX - spacing + (buttonIndex % 3) * spacing;
				button.PositionY = topRowY - spacing * (buttonIndex / 3);

				button.Name = "LevelButton" + i;
				button.Clicked += HandleButtonClicked;
				levelButtons.Add (button);
				mainLayer.AddChild (button);

				//range from 0-5
				buttonIndex++;
			}
		}

		private void DestroyLevelButtons()
		{
			for (int i = levelButtons.Count - 1; i > -1; i--)
			{
				mainLayer.RemoveChild (levelButtons [i]);
				levelButtons [i].Dispose ();
			}
		}

		private void HandleButtonClicked(object sender, EventArgs args)
		{
			// levelNumber is 1-based, so subtract 1:
			//sender is the button clicked
			var levelIndex = (sender as Button).LevelNumber - 1;

			//把要前往的等级scene的level保存下来
			LevelManager.Self.CurrentLevel = levelIndex;

			CoinTime.GameAppDelegate.GoToGameScene ();
			// go to game screen
		}

	}
}


using System;
using System.Linq;
using CocosSharp;
using System.Collections.Generic;


namespace CoinTimeGame.Entities
{
	public enum ButtonStyle
	{
		//all kinds of button
		LevelSelect,
		LeftArrow,
		RightArrow
	}


	public class Button : CCNode
	{
		
		CCSprite sprite;
		CCLabel label;
		CCLayer owner;

		ButtonStyle buttonStyle;

		//field
		public ButtonStyle ButtonStyle
		{
			get
			{
				//get only return
				return buttonStyle;
			}
			set
			{
				buttonStyle = value;

				//set can do something base on the value
				switch (buttonStyle)
				{
				case ButtonStyle.LevelSelect:
					sprite.Texture = new CCTexture2D ("ui/buttonup.png");
					sprite.IsAntialiased = false;
					sprite.FlipX = false;
					break;
	
				case ButtonStyle.LeftArrow:
					sprite.Texture = new CCTexture2D ("ui/arrowup.png");
					sprite.IsAntialiased = false;
					//mirror flip arrow
					sprite.FlipX = true;
					break;
				case ButtonStyle.RightArrow:
					sprite.Texture = new CCTexture2D ("ui/arrowup.png");
					sprite.IsAntialiased = false;

					sprite.FlipX = false;
					break;
				}

				//在指定大小的长方形里显示
				sprite.TextureRectInPixels = 
					new CCRect (0, 0,
					sprite.Texture.PixelsWide,
					sprite.Texture.PixelsHigh);
			}
		}

		int levelNumber;

		//EventHandler for the click event, when user click, 
		// button.Clicked += HandleButtonClicked;
		// Clicked = HandleButtonClicked
		// 触发对应的事件
		public event EventHandler Clicked;

		public int LevelNumber
		{
			get
			{
				return levelNumber;
			}
			set
			{
				levelNumber = value;

				label.Text = levelNumber.ToString ();
			}
		}

		public Button(CCLayer layer)
		{
			// Give it a default texture, may get changed in ButtonStyle
			sprite = new CCSprite ("ui/buttonup.png");
			sprite.IsAntialiased = false;
			this.AddChild (sprite);

			label = new CCLabel("", "fonts/alphbeta.ttf", 24, CCLabelFormat.SystemFont);
			label.IsAntialiased = false;
			this.AddChild (label);

			//touch event
			var touchListener = new CCEventListenerTouchAllAtOnce ();
			touchListener.OnTouchesBegan = HandleTouchesBegan;

			//layer add click event listener
			//when user click layer, touchListener will be launched
			layer.AddEventListener (touchListener);

		}

		private void HandleTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
		{
			if (this.Visible)
			{
				// did the user actually click within the CCSprite bounds?
				//get the first touch
				var firstTouch = touches.FirstOrDefault ();

				if (firstTouch != null)
				{
					//if the touch is inside the button
					bool isTouchInside = sprite.BoundingBoxTransformedToWorld.ContainsPoint (firstTouch.Location);

					if (isTouchInside && Clicked != null)
					{
						//do Clicked
						//Clicked对应在调用button的类里面定义的方法
						//this 是把这个按钮的类传递过去, e 是传递一些参数
						Clicked (this, null);
					}
				}
			}
		}
	}
}


using System;
using CocosSharp;
using System.Collections.Generic;

namespace CoinTimeGame.Entities
{
	public class TouchScreenInput : IDisposable
	{
		CCEventListenerTouchAllAtOnce touchListener;
        CCLayer owner;
		int horizontalMovementTouchId = -1;
		bool touchedOnRightSide = false;

        public float HorizontalRatio
        {
            get;
            private set;
        }

		public bool WasJumpPressed
		{
			get;
			private set;
		}


		public TouchScreenInput(CCLayer owner)
		{
            this.owner = owner;

			touchListener = new CCEventListenerTouchAllAtOnce ();
			touchListener.OnTouchesMoved = HandleTouchesMoved;
			touchListener.OnTouchesBegan = HandleTouchesBegan;
			owner.AddEventListener (touchListener);

		}

		private void HandleTouchesBegan(List<CCTouch> touches, CCEvent touchEvent)
		{
			//判断是否touch在屏幕右半面
			foreach (var item in touches)
			{
				if (item.Location.X > owner.ContentSize.Center.X)
				{
					touchedOnRightSide = true;
				}
			}
		}

		private void HandleTouchesMoved (List<CCTouch> touches, CCEvent touchEvent)
		{
			DetermineHorizontalRatio (touches);

		}

		public void Dispose()
		{
			owner.RemoveEventListener (touchListener);
		}

		void DetermineHorizontalRatio(List<CCTouch> touches)
		{
			CCTouch horizontalMovementTouch = null;

			//touch.Id = -1时，这个touch是一个invalid touch
			if (horizontalMovementTouchId != -1)
			{
				foreach (var item in touches)
				{
					if (item.Id == horizontalMovementTouchId)
					{
						horizontalMovementTouch = item;
						break;
					}
				}
			}

			if (horizontalMovementTouch == null)
			{
				// Couldn't find one or we have a -1 ID. Let's set
				// the ID to -1 to indicate we don't have a valid touch:
				horizontalMovementTouchId = -1;
			}

			if (horizontalMovementTouch == null)
			{
				// let's see if we can find one that is to the left of the screen
				//第一次时，先从touch list 中找到一个在左屏幕的滑动，把horizontalMovementTouch更新了
				foreach (var item in touches)
				{
					if (item.Location.X < owner.ContentSize.Center.X)
					{
						horizontalMovementTouch = item;
						horizontalMovementTouchId = item.Id;
					}
				}
			}

			if (horizontalMovementTouch != null)
			{
				//计算出HorizontalRatio, 水平运动的系数
				float quarterWidth = owner.ContentSize.Width / 4;
				HorizontalRatio = (horizontalMovementTouch.Location.X - quarterWidth) / quarterWidth ;

				//左屏幕被分为两部分，每部分大小为一个quarterWidth
				//如果touch在左边的quarter则HorizontalRatio为负，人物向左走
				//如果touch在右边的quarter则HorizontalRatio为正，人物向右走

				//当按在右屏幕时,touchedOnRightSide=true，player跳起
				HorizontalRatio = Math.Min (1, HorizontalRatio);
				HorizontalRatio = Math.Max (-1, HorizontalRatio);

				//如果touch离左屏幕的中心线太近，则忽略此touch
				const float deadZone = .15f;
				if (Math.Abs (HorizontalRatio) < deadZone)
				{
					HorizontalRatio = 0;
				}
			}
			else
			{
				// for emulator testing, we'll turn this off, but we need it on eventually on device:
//				HorizontalRatio = 0;
			}
		}

        public void UpdateInputValues()
        {
			//当按右屏幕时跳起
			WasJumpPressed = touchedOnRightSide;
			touchedOnRightSide = false;
            // todo: process "jump"
            //throw new NotImplementedException();
        }
    }
}


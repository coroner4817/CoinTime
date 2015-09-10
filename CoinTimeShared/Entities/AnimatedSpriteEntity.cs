using System;
using System.Linq;
using CocosSharp;
using System.Collections.Generic;
using CoinTimeGame.ContentRuntime.Animations;
using CoinTimeGame.ContentLoading.Animations;
using CoinTimeGame.ContentLoading;

namespace CoinTimeGame.Entities
{
	public class AnimatedSpriteEntity : CCNode
	{
		TimeSpan timeIntoAnimation;

		CCSprite sprite;

		static Dictionary<string, List<Animation>> animationCache = 
			new Dictionary<string, List<Animation>>();

		protected List<Animation> animations;

		Animation currentAnimation;

		public bool IsLoopingAnimation
		{
			get;
			set;
		}

		public CCRect BoundingBoxWorld
		{
			get
			{
				return this.sprite.BoundingBoxTransformedToWorld;
			}
		}

		protected Animation CurrentAnimation
		{
			get
			{
				return currentAnimation;
			}
			set
			{ 
				if (currentAnimation != value)
				{
					currentAnimation = value;
					// restart the animation:
					timeIntoAnimation = TimeSpan.Zero;
					//timeIntoAnimation = TimeSpan.Zero = 00:00:00
					// Update the sprite immediately:
					PerformSpriteAnimation (0);
				}
			}
		}

		public AnimatedSpriteEntity ()
		{
			//父类的构造方法
			//父类在子类继承之前就已经实例化了

			CreateSprite ();

			Schedule (PerformSpriteAnimation);
		}

		void CreateSprite()
		{
			IsLoopingAnimation = true;

			// The entire game will use mastersheet.png:
			sprite = new CCSprite ("mastersheet.png");

			sprite.IsAntialiased = false;

			this.AddChild (sprite);
			sprite.TextureRectInPixels = new CCRect (1024, 0, 100, 100);
			sprite.ContentSize = new CCSize (100, 100);

		}

		protected void LoadAnimations(string fileName)
		{
			//把动画读到dictionary里，如果dic中已经存在则直接读取
			if (animationCache.ContainsKey (fileName))
			{
				animations = animationCache [fileName];
			}
			else
			{
				animations = new List<Animation> ();
				AnimationChainListSave acls = XmlDeserializer.Self.XmlDeserialize<AnimationChainListSave> (fileName);

				foreach (var animationSave in acls.AnimationChains)
				{
					//从animationSave(AnimationChainSave类)中读取animation，一个一个添加到animation 的list里
					animations.Add (Animation.FromAnimationSave (animationSave));
				}


				animationCache.Add (fileName, animations);
			}

			// This prevents the sprite from temporarily showing
			// the entire PNG for a split second.
			if (animations != null && animations.Count > 0)
			{
				CurrentAnimation = animations [0];
			}

		}

		//一直被运行
		void PerformSpriteAnimation(float time)
		{
			//times是schedule运行的总时间
			//CurrentAnimation不为空，因为一开始LoadAnimations时就有将它赋值
			if (CurrentAnimation != null)
			{
				
				double secondsIntoAnimation = timeIntoAnimation.TotalSeconds + time;
				double remainder = secondsIntoAnimation % CurrentAnimation.Duration.TotalSeconds;

				//remainder是动画剩余的时间，secondsIntoAnimation是开始动画的时间
				bool passedEnd = remainder < secondsIntoAnimation;


				if (passedEnd && !IsLoopingAnimation)
				{
					// we're not looping so set the time to the duration minus half of the last frame
					// to minimize rounding errors.
					// This is somewhat inefficient, but we're probably dealing with a small number of entities
					// so we'll just swallow the inefficiency to keep the code simpler. The alternative is to store
					// off that this entity is no longer animating until either the IsLoopingAnimation is set to true
					// or the CurrentAnimation is changed.
					remainder = CurrentAnimation.Duration.TotalSeconds - CurrentAnimation.Frames.Last().Duration.TotalSeconds/2.0;
				}

				//remainder是动画所剩余的时间
				timeIntoAnimation = TimeSpan.FromSeconds (remainder);

				//得到某个时间点这个动画的形态，存到rect里
				var rectangle = CurrentAnimation.GetRectangleAtTime (timeIntoAnimation);

				//读取某一个时间点这个动画的frame的FlipHorizontal属性
				bool flipHorizontally = CurrentAnimation.GetIfFlippedHorizontallyAtTime (timeIntoAnimation);

				//更新entity的动画
				sprite.FlipX = flipHorizontally;
				//更新entity的形态的rect
				sprite.TextureRectInPixels = rectangle;

				sprite.ContentSize = rectangle.Size;
			}
		}

		public bool Intersects(AnimatedSpriteEntity other)
		{
			//返回是否两个entity的rect有重叠
			return this.sprite.BoundingBoxTransformedToWorld.IntersectsRect (other.sprite.BoundingBoxTransformedToWorld);
		}
	}
}


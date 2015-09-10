using System;
using System.Collections.Generic;
using System.Linq;
using CocosSharp;
using CoinTimeGame.ContentLoading.Animations;

namespace CoinTimeGame.ContentRuntime.Animations
{
	//重写这个类的意义在于
	//可以和重写的XmlDeserializer结合
	//从特定的动画文件achx中读取动画
	//这个类没有构造函数，所以编译器自动会添加一个空的构造函数
	public class Animation
	{
		// The frames in this animation
		//frames list中是这个动画中每一帧的图像（rect），持续时间，水平翻转
		//每一帧都是一个frame
		List<AnimationFrame> frames = new List<AnimationFrame>();

		public IEnumerable<AnimationFrame> Frames
		{
			get
			{
				return frames;
			}
		}

		public string Name
		{
			get;
			set;
		}

		// The length of the entire animation
		//这个动画的整个时间
		public TimeSpan Duration
		{
			//只能get说明只能读取，不可更改
			//更改是set
			get
			{
				double totalSeconds = 0;
				foreach (var frame in frames)
				{
					totalSeconds += frame.Duration.TotalSeconds;
				}

				return TimeSpan.FromSeconds (totalSeconds);
			}
		}

		public static Animation FromAnimationSave(AnimationChainSave animationSave)
		{
			//这个类没有构造函数，所以编译器自动会添加一个空的构造函数
			Animation toReturn = new Animation ();

			toReturn.Name = animationSave.Name;

			//读取frame(AnimationFrameSave)中的所有动画数据
			foreach (var frame in animationSave.Frames)
			{
				CCRect rectangle;

				rectangle = new CCRect (
					frame.LeftCoordinate, 
					frame.TopCoordinate, 
					frame.RightCoordinate - frame.LeftCoordinate, 
					frame.BottomCoordinate - frame.TopCoordinate);

				var duration = TimeSpan.FromSeconds (frame.FrameLength);

				//toReturn.frames是一个frame的集合
				//每一个frame是动作的一步
				//AddFrame就是把读取到的frame添加到这个list里
				toReturn.AddFrame (rectangle, duration, flipHorizontal:frame.FlipHorizontal);
			}

			return toReturn;
		}

		//得到某一时刻这个动画的frame
		private AnimationFrame GetAnimationFrameAtTime(TimeSpan timeSpan)
		{
			AnimationFrame currentFrame = null;

			// See if we can find the frame
			TimeSpan accumulatedTime = new TimeSpan(0);
			//遍历找到那个时间点的frame
			foreach(var frame in frames)
			{
				if (accumulatedTime + frame.Duration >= timeSpan)
				{
					currentFrame = frame;
					break;
				}
				else
				{
					accumulatedTime += frame.Duration;
				}
			}

			// If no frame was found, then try the last frame, 
			// just in case timeIntoAnimation somehow exceeds Duration
			if (currentFrame == null)
			{
				currentFrame = frames.LastOrDefault ();
			}

			return currentFrame;
		}


		public CCRect GetRectangleAtTime (TimeSpan timeSpan)
		{
			//得到某一个时间点这个动画的帧的rect
			//读取frame中的rect field
			var currentFrame = GetAnimationFrameAtTime (timeSpan);

			// If we found a frame, return its rectangle, otherwise
			// return an empty rectangle (one with no width or height)
			if (currentFrame != null)
			{
				return currentFrame.SourceRectangle;
			}
			else
			{
				//空rect
				return CCRect.Zero;
			}
		}

		public bool GetIfFlippedHorizontallyAtTime(TimeSpan timeSpan)
		{
			//读取某一个时间点这个动画的frame的FlipHorizontal属性

			var currentFrame = GetAnimationFrameAtTime (timeSpan);

			if (currentFrame != null)
			{
				return currentFrame.FlipHorizontal;
			}
			else
			{
				return false;
			}
		}

		// Adds a single frame to this animation.
		public void AddFrame(CCRect rectangle, TimeSpan duration, bool flipHorizontal = false)
		{
			AnimationFrame newFrame = new AnimationFrame()
			{
				SourceRectangle = rectangle,
				Duration = duration,
				FlipHorizontal = flipHorizontal
			};

			frames.Add(newFrame);
		}
	}
}


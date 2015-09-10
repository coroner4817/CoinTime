using System;
using CocosSharp;
using CoinTimeGame.ContentRuntime.Animations;
using System.Collections.Generic;
using CoinTimeGame.Entities.Data;
using System.Linq;

namespace CoinTimeGame.Entities
{
	public enum LeftOrRight
	{
		Left,
		Right
	}

	//继承于物理实体类,同样适用于enemy
	public class Player : PhysicsEntity
	{
		Animation walkLeftAnimation;
		Animation walkRightAnimation;

		public bool IsOnGround
		{
			get;
			private set;
		}

		public Player ()
		{
			//LoadAnimations可以用是因为Player : PhysicsEntity， PhysicsEntity : AnimatedSpriteEntity, LoadAnimations是AnimatedSpriteEntity的方法
			//读取achx中所有的AnimationChain
			LoadAnimations ("Content/animations/playeranimations.achx");
			//achx文件本质是xml
			walkLeftAnimation = animations.Find (item => item.Name == "WalkLeft");
			walkRightAnimation = animations.Find (item => item.Name == "WalkRight");
			//初始化动作为向左走
			CurrentAnimation = walkLeftAnimation;
		}


		public void PerformActivity(float seconds)
		{
			//seconds是每一帧的时间
			//计算位置和速度
			ApplyMovementValues (seconds);

			//根据计算结果计算动画
			AssignAnimation ();

			//避免VelocityY的下落速度过快（> -160）
			this.VelocityY = System.Math.Max (this.VelocityY, PlayerMovementCoefficients.MaxFallingSpeed);
		}

		private void AssignAnimation()
		{
			if (VelocityX > 0)
			{
				CurrentAnimation = walkRightAnimation;
			}
			else if (VelocityX < 0)
			{
				CurrentAnimation = walkLeftAnimation;
			}
			// if 0 do nothing
		}

		public void ApplyInput(float horizontalMovementRatio, bool jumpPressed)
		{
			//模拟重力加速度，一定向下为负
            AccelerationY = PlayerMovementCoefficients.GravityAcceleration; //-420

			//水平运动的系数是与用户按屏幕的位置有关的，touchInputScreen中有计算 horizontalMovementRatio
			VelocityX = horizontalMovementRatio * PlayerMovementCoefficients.MaxHorizontalSpeed;

			if (jumpPressed && IsOnGround)
			{
				//向上跳起，更新参数，下一个schedule时起跳
				VelocityY = PlayerMovementCoefficients.JumpVelocity;
			}
		}

		public void ReactToCollision(CCPoint reposition)
		{
			//reposition是player与实体瓦片作用产生的位移向量，不是player的所有位移向量

			//判断player是否在地面上
			//当player在地面上时，和地面瓦片一直作用，所以reposition.Y 一直为0.05左右
			//但是当player起跳时，没有实体瓦片和player作用，所以reposition.Y为0, IsOnGround为false
			IsOnGround = reposition.Y > 0;

			//更新Collision后player的速度
			ProjectVelocityOnSurface (reposition);
		}
	}
}


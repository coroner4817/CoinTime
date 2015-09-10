using System;
using CocosSharp;

namespace CoinTimeGame.Entities
{
	//PhysicsEntity继承于一个动画sprite类AnimatedSpriteEntity
	public class PhysicsEntity : AnimatedSpriteEntity
	{
		//float的default value是0.0f
		protected float VelocityX;
		protected float VelocityY;

		protected float AccelerationX;
		protected float AccelerationY;

		public PhysicsEntity ()
		{
		}


		protected void ApplyMovementValues(float seconds)
		{
			float halfSecondsSquared = (seconds * seconds) / 2.0f;

			//通过加速度公式计算entity的位置和速度
			this.PositionX += 
				this.VelocityX * seconds + this.AccelerationX * halfSecondsSquared;
			this.PositionY += 
				this.VelocityY * seconds + this.AccelerationY * halfSecondsSquared;

			this.VelocityX += this.AccelerationX * seconds;
			this.VelocityY += this.AccelerationY * seconds; 

		}

		protected void ProjectVelocityOnSurface(CCPoint reposition)
		{
			if (reposition.X != 0 || reposition.Y != 0)
			{
				var repositionNormalized = reposition;

				//vector归一化
				repositionNormalized.Normalize ();

				CCPoint velocity = new CCPoint (VelocityX, VelocityY);

				//计算velocity和repositionNormalized的点积，得到瓦片位移向量作用后的位置
				var dot = CCPoint.Dot (velocity, repositionNormalized);

				// falling into the collision, rather than out of
				//如果dot小于0，则entity掉入瓦片中(掉入被撞击的物体中)
				//掉入是正常现象
				//之后把entity的速度更改下
				//如果是在地面，速度会被改为0
				//不太懂这个算法
				if (dot < 0)
				{
					velocity -= repositionNormalized * dot;

					VelocityX = velocity.X;
					VelocityY = velocity.Y;

				}
			}
		}

	}
}


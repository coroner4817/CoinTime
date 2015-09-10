using System;
using CocosSharp;
using CoinTime;

namespace CoinTimeGame.Scenes
{
	public partial class GameScene
	{
		void PerformCollision(float seconds)
		{
			PerformPlayerVsCoinsCollision ();

			PerformPlayerVsDoorCollision ();

			PerformPlayerVsEnvironmentCollision ();

			PlayerVsDamageDealersCollision ();

			//enemy和环境（实体瓦片)的碰撞
			PerformEnemiesVsEnvironmentCollision ();
		}

		bool PerformPlayerVsCoinsCollision()
		{
			bool grabbedAnyCoins = false;

			// Reverse loop since items may get removed from the list
			//遍历看每一个coin有没有被player撞到
			for (int i = coins.Count - 1; i > -1; i--)
			{
				
				if (player.Intersects (coins [i]))
				{
					//销毁这个coin
					var coinToDestroy = coins [i];

					DestroyCoin (coinToDestroy);

					grabbedAnyCoins = true;
				}
			}

			//当用户吃掉所有的coin时，door开启
			if (grabbedAnyCoins && coins.Count == 0)
			{
				// User got all the coins, so open the door
				if (door != null)
				{
					door.IsOpen = true;
				}
			}
				
			return grabbedAnyCoins;
		}

		void PerformPlayerVsDoorCollision()
		{
			//如果门打开，且player进入门中
			if (door != null && door.IsOpen && player.Intersects (door))
			{
				try
				{
					//判断是不是最后一个level
					bool isLastLevel = (LevelManager.Self.CurrentLevel + 1 == LevelManager.Self.NumberOfLevels);

					if(isLastLevel)
					{
						//如果是最后一关，则进入level选择页面
						GameAppDelegate.GoToLevelSelectScene();
					}
					else
					{
						//否则销毁这一关所有的entity，加载下一关
						DestroyLevel ();
						LevelManager.Self.CurrentLevel++;
						GoToLevel(LevelManager.Self.CurrentLevel);
					}
				}
				catch(Exception e)
				{
					int m = 3;
				}
			}
		}

		void PerformPlayerVsEnvironmentCollision ()
		{
			CCPoint positionBeforeCollision = player.Position;
			CCPoint reposition = CCPoint.Zero;

			//PerformCollisionAgainst这个函数不断的更新瓦片地图中的实体瓦片对于player的影响
			//地面 - 支持，墙 - 反弹.....
			//不断更新player的位置
			if (levelCollision.PerformCollisionAgainst (player))
			{
				////reposition只是player与实体瓦片作用产生的位移向量，不是player的所有位移向量
				reposition = player.Position - positionBeforeCollision;
			}
				

			//判断瓦片作用之后player是否还在地上
			//更新player被瓦片作用之后的速度
			player.ReactToCollision (reposition);
		}

		void PlayerVsDamageDealersCollision()
		{
			//遍历每一个damageDealer,看是否碰到
			for (int i = 0; i < damageDealers.Count; i++)
			{
				if (player.BoundingBoxWorld.IntersectsRect (damageDealers [i].BoundingBoxWorld))
				{
					//一碰就死，然后重新开始这一关
					HandlePlayerDeath ();
					break;
				}
			}
		}

		void PerformEnemiesVsEnvironmentCollision()
		{
			//enemy和环境（实体瓦片)的碰撞

			for (int i = 0; i < enemies.Count; i++)
			{
				var enemy = enemies [i];
				CCPoint positionBeforeCollision = enemy.Position;
				CCPoint reposition = CCPoint.Zero;


				//PerformCollisionAgainst这个函数不断的更新瓦片地图中的实体瓦片对于enemy的影响
				//地面 - 支持，墙 - 反弹.....
				//不断更新enemy的位置
				if (levelCollision.PerformCollisionAgainst (enemy))
				{
					//reposition只是enemy与实体瓦片作用产生的位移向量，不是enemy的所有位移向量
					reposition = enemy.Position - positionBeforeCollision;
				}

				//更新enemy被实体瓦片作用之后的速度
				enemy.ReactToCollision (reposition);
			}
		}

	}
}


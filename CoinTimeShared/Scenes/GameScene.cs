using System;
using CocosSharp;
using CoinTimeGame.Entities;
using System.Collections.Generic;
using CoinTimeGame.TilemapClasses;
using CoinTime;

namespace CoinTimeGame.Scenes
{
	public partial class GameScene : CCScene
	{
		CCLayer gameplayLayer;
		CCLayer hudLayer;

		CCTileMap currentLevel;
		LevelCollision levelCollision;
		CCTileMapLayer backgroundLayer;
			
		TouchScreenInput input;

		float secondsLeft;

		Player player;
		Door door;

		const int secondsPerLevel = 30;
		Timer timer;

		//掉血的东西的list，刺加上敌人
		List<IDamageDealer> damageDealers = new List<IDamageDealer>();

		List<Enemy> enemies = new List<Enemy>();
		List<Coin> coins = new List<Coin>();

		public GameScene (CCWindow mainWindow) : base(mainWindow)
		{
			CreateLayers ();

			CreateHud ();

			GoToLevel (LevelManager.Self.CurrentLevel);

			//每一帧(frame schedule)都运行一次PerformActivity，不断刷新
			Schedule(PerformActivity);
		}

		private void CreateHud()
		{
			//显示的计时器
			timer = new Timer ();
			timer.PositionX = this.ContentSize.Center.X;
			timer.PositionY = this.ContentSize.Height - 20;
			hudLayer.AddChild (timer);

			var backButton = new Button (hudLayer);
			backButton.ButtonStyle = ButtonStyle.LeftArrow;
			backButton.Clicked += HandleBackClicked;
			backButton.PositionX = 30;
			backButton.PositionY = ContentSize.Height - 30;
			hudLayer.AddChild (backButton);

		}

		private void HandleBackClicked(object sender, EventArgs args)
		{
			GameAppDelegate.GoToLevelSelectScene ();
		}


		//这个PerformActivity被schedule，每帧运行一次
		//所以这个方法里的方法都不断被运行
		//float seconds是这个schedule运行的总时间
		//当一个动作开始后，在第一个schedule里更新参数，第二个schedule时实施动作
		private void PerformActivity(float seconds)
		{
			//不断刷新player的动作
			//这个player.PerformActivity不是一个被schedule的方法
			player.PerformActivity (seconds);


			for (int i = 0; i < enemies.Count; i++)
			{
				//不断刷新每个enemy的动作
				enemies [i].PerformActivity (seconds);
			}

			//判断用户的输入
			ApplyInput(seconds);

			//不断刷新entity和瓦片地图中的实体瓦片，还有entity和entity之间的碰撞对于entity的影响
			PerformCollision(seconds);

			//地图的滚动
			PerformScrolling ();

			PerformTimerActivity (seconds);
		}

		private void PerformTimerActivity(float seconds)
		{
			// This suffers from accumulation error:
			//一开始时设置secondsLeft=30
			secondsLeft -= seconds;
			timer.SecondsLeft = secondsLeft;

			//时间没了就死
			if (secondsLeft <= 0)
			{
				HandlePlayerDeath ();
			}
		}

		private void PerformScrolling ()
		{
			//让瓦片地图滚动起来
			//背景不动

			//计算player的有效位置，从而让瓦片地图跟着player滚动
			float effectivePlayerX = player.PositionX;

			// Effective values limit the scorlling beyond the level's bounds
			//this.ContentSize.Center.X 是屏幕的中心点
			//如果player超过中心点，则取player
			//如果player没有超过中心点，则取中心点,认为player的有效位置在中心点
			//所以在player走到地图最后之前，player最远只能走到屏幕的中点，不可能超过中点
			effectivePlayerX = System.Math.Max (player.PositionX, this.ContentSize.Center.X);

			//计算这个level的整个瓦片地图的长度
			//这一步是考虑如果瓦片地图减去中心的剩余长度
			//到了最后，地图只剩一点的时候
			//避免画面还向左滚动，造成右半屏幕只剩背景图片，没有实体瓦片地图
			//当player到了地图的最右边的时候，这个算法让程序认为player还在屏幕中心的左边，所以不会滚动
			float levelWidth = currentLevel.TileTexelSize.Width * currentLevel.MapDimensions.Column;
			effectivePlayerX = System.Math.Min (effectivePlayerX, levelWidth - this.ContentSize.Center.X);

			//计算y方向的有效位置
			//当player跳起时，仍认为他没有跳很高，这样y方向的滚动就很小或没有
			float effectivePlayerY = player.PositionY;
			float levelHeight = currentLevel.TileTexelSize.Height * currentLevel.MapDimensions.Row;
			effectivePlayerY = System.Math.Min(player.Position.Y, levelHeight - this.ContentSize.Center.Y);
			// We don't want to limit the scrolling on Y - instead levels should be large enough
			// so that the view never reaches the bottom. This allows the user to play
			// with their thumbs without them getting in the way of the game.


			//计算出地图layer应该移动到的位置

			//往左移是减
			//得到的positionX应该是负的
			//这样gameplayLayer的最左端就会移到这个在坐标系里是负的位置
			//就实现了gameplayLayer的左移
			float positionX = -effectivePlayerX + this.ContentSize.Center.X;
			//y方向往下移也是减
			//当player起跳时，gameplayLayer应该相应向下移动
			//所以原本gameplayLayer的中心会比之前有降低
			//所以是减
			float positionY = -effectivePlayerY + this.ContentSize.Center.Y;


			//gameplayLayer.PositionX是这个layer的左边界的在坐标系的位置
			//gameplayLayer.PositionY是这个laer的下边界在坐标系的位置
			//在最开始时，PositionX=0，PositionY= -150

			//更新gameplayLayer的位置左边，实现地图向左滚动或向下滚动
			gameplayLayer.PositionX = positionX;
			gameplayLayer.PositionY = positionY;

	
			// We don't want the background to scroll, 
			// so we'll make it move the opposite direction of the rest of the tilemap:
			//刚才background已经跟着gameplayLayer滚动过了
			//我们不想background动，所以再让他滚回来
			if (backgroundLayer != null)
			{
				backgroundLayer.PositionX = -positionX;
				backgroundLayer.PositionY = -positionY;
			}

			//更新TileLayersContainer的位置坐标
			currentLevel.TileLayersContainer.PositionX = positionX;
			currentLevel.TileLayersContainer.PositionY = positionY;
		}

		private void CreateLayers()
		{
			//gameplayLayer显示游戏动画
			gameplayLayer = new CCLayer ();
			this.AddChild (gameplayLayer);
			//hudLayer显示按钮，计时器等
			hudLayer = new CCLayer ();
			this.AddChild (hudLayer);
		}


		private void GoToLevel(int levelNumber)
		{
			LoadLevel (levelNumber);

			ProcessTileProperties ();

			//计时器剩余的时间
			secondsLeft = secondsPerLevel;
		}

		private void LoadLevel(int levelNumber)
		{
			//读取瓦片地图
			currentLevel = new CCTileMap ("level" + levelNumber + ".tmx");
			currentLevel.Antialiased = false;
			//backgroundLayer是游戏中不动的背景图, 每个currentLevel都有一个对应的backgroundLayer, currentLevel中的其他部分随画面滚动
			backgroundLayer = currentLevel.LayerNamed ("Background");

			// CCTileMap is a CCLayer, so we'll just add it under all entities
			this.AddChild (currentLevel);

			//levelCollision 是确定地图中哪些部分是游戏人物不可以进入的
			levelCollision = new LevelCollision ();
			levelCollision.PopulateFrom (currentLevel);

			// put the game layer after
			this.RemoveChild(gameplayLayer);
			this.AddChild(gameplayLayer);

			this.RemoveChild (hudLayer);
			this.AddChild (hudLayer);
		}

		private void ProcessTileProperties()
		{
			TileMapPropertyFinder finder = new TileMapPropertyFinder (currentLevel);

			//遍历每一个瓦片
			foreach (var propertyLocation in finder.GetPropertyLocations())
			{
				var properties = propertyLocation.Properties;
				if (properties.ContainsKey ("EntityType"))
				{
					float worldX = propertyLocation.WorldX;
					float worldY = propertyLocation.WorldY;

					//加上offset
					if (properties.ContainsKey ("YOffset"))
					{
						string yOffsetAsString = properties ["YOffset"];
						float yOffset = 0;
						float.TryParse (yOffsetAsString, out yOffset);
						worldY += yOffset;
					}

					//如果这个瓦片是entity，则添加到游戏layer
					bool created = TryCreateEntity (properties ["EntityType"], worldX, worldY);

					if (created)
					{
						//如果这个瓦片是entity，且在layer中添加成功的话,在这个layer中移除这个瓦片
						propertyLocation.Layer.RemoveTile (propertyLocation.TileCoordinates);
					}
				}
				else if (properties.ContainsKey ("RemoveMe"))
				{
					propertyLocation.Layer.RemoveTile (propertyLocation.TileCoordinates);
				}
			}

			//相当于玩家可以开始操作
			input = new TouchScreenInput(gameplayLayer);
		}

		private bool TryCreateEntity(string entityType, float worldX, float worldY)
		{
			CCNode entityAsNode = null;

			switch (entityType)
			{
			case "Player":
				player = new Player ();
				entityAsNode = player;
				break;
			case "Coin":
				Coin coin = new Coin ();
				entityAsNode = coin;
				coins.Add (coin);
				break;
			case "Door":
				door = new Door ();
				entityAsNode = door;
				break;

			//会减血的entities
			case "Spikes":
				var spikes = new Spikes ();
				this.damageDealers.Add (spikes);
				entityAsNode = spikes;
				break;
			case "Enemy":
				var enemy = new Enemy ();
				this.damageDealers.Add (enemy);
				this.enemies.Add (enemy);
				entityAsNode = enemy;
				break;
			}

			if(entityAsNode != null)
			{
				//在游戏layer中添加entity
				entityAsNode.PositionX = worldX;
				entityAsNode.PositionY = worldY;
				gameplayLayer.AddChild (entityAsNode);
			}

			return entityAsNode != null;
		}

		private void ApplyInput(float seconds)
		{
			//查看用户是否按屏幕的右半边，按的话player跳起
            input.UpdateInputValues();

			//更新弹跳的参数, 下一帧开始跳
            player.ApplyInput(input.HorizontalRatio, input.WasJumpPressed);
		}

		private void DestroyCoin(Coin coinToDestroy)
		{
			//销毁coin
			coins.Remove (coinToDestroy);
			gameplayLayer.RemoveChild (coinToDestroy);
			coinToDestroy.Dispose ();
		}

		private void DestroyDamageDealer(IDamageDealer damageDealer)
		{
			//移除damageDealer
			damageDealers.Remove (damageDealer);
			if (damageDealer is CCNode)
			{
				var asNode = damageDealer as CCNode;
				gameplayLayer.RemoveChild (asNode);
				asNode.Dispose ();
			}
		}

		private void DestroyLevel()
		{
			//销毁这一关，进入下一关

			//销毁player
			gameplayLayer.RemoveChild (player);
			player.Dispose ();

			//销毁door
			gameplayLayer.RemoveChild (door);
			door.Dispose ();

			//销毁coins
			for (int i = coins.Count - 1; i > -1; i--)
			{
				var coinToDestroy = coins [i];
				DestroyCoin (coinToDestroy);
			}

			//销毁敌人加上刺
			for (int i = damageDealers.Count - 1; i > -1; i--)
			{
				var damageDealer = damageDealers [i];

				DestroyDamageDealer (damageDealer);
			}

			// We can just clear the list - the contained entities are destroyed as damage dealers:
			enemies.Clear ();

			//停止Eventlistener to touchListener
			input.Dispose ();

			//销毁地图
			this.RemoveChild (currentLevel);
			currentLevel.Dispose ();

			// don't think we need to do anything with LevelCollision

		}

		private void HandlePlayerDeath()
		{
			DestroyLevel ();
			// player died, so start the level over
			GoToLevel (LevelManager.Self.CurrentLevel);
		}
	}
}


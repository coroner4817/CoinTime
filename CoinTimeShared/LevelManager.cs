using System;
using Microsoft.Xna.Framework;

namespace CoinTimeGame
{
	public class LevelManager
	{
		//()=>new LevelManager() 是相当于构造一个新的LevelManager
		//()=>指传递给LevelManager的参数是空
		//http://www.cnblogs.com/knowledgesea/p/3163725.html
		//lazy<T> 按需构造，只有当要访问self的数据时才构造，避免产生大的无用的对象占内存
		//当访问LevelManager.Self.NumberOfLevels时才会构造并读取tmx中的数据（比较耗时）

		static Lazy<LevelManager> self = 
			new Lazy<LevelManager>(()=>new LevelManager());

		public int NumberOfLevels
		{
			get;
			private set;
		}

		public int CurrentLevel
		{
			get;
			set;
		}

		public static LevelManager Self
		{
			//一层封装
			//其他类读取LevelManager中存储的数据
			get
			{
				return self.Value;
			}
		}

		private LevelManager()
		{
			DetermineAvailableLevels ();
		}

		private void DetermineAvailableLevels()
		{
			// This game relies on levels being named "levelx.tmx" where x is an integer beginning with
			// 1. We have to rely on XNA's TitleContainer which doesn't give us a GetFiles method - we simply
			// have to check if a file exists, and if we get an exception on the call then we know the file doesn't
			// exist. 
			NumberOfLevels = 0;

			while (true)
			{
				bool fileExists = false;

				try
				{
					using(var stream = TitleContainer.OpenStream("Content/levels/level" + NumberOfLevels + ".tmx"))
					{

					}
					// if we got here then the file exists!
					fileExists = true;
				}
				catch
				{
					// do nothing, fileExists will remain false
				}

				if (!fileExists)
				{
					break;
				}
				else
				{
					NumberOfLevels++;
				}
			}
		}
	}
}


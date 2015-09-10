using System;
using System.IO;
using System.Xml.Serialization;

namespace CoinTimeGame.ContentLoading
{

	//重写这个类的意义就在于我们可以通过文件名来读取文件
	//如果只是原生的Deserializer是没有到asset目录下找文件的功能的
	public class XmlDeserializer
	{
		//lazy<T>延时加载
		//lazy<T> 按需构造，只有当要访问self的数据时才构造，避免产生大的无用的对象占内存
		static Lazy<XmlDeserializer> self = new Lazy<XmlDeserializer>(
			() => new XmlDeserializer()
		);

		#if ANDROID
		//在读取文件之前必须set activity
		public Android.App.Activity Activity { get; set;}
		#endif

		public static XmlDeserializer Self
		{
			//一层封装，让其他类来使用这个类里的数据
			get
			{
				return self.Value;
			}
		}

		private XmlDeserializer()
		{

		}

		//T代表某一种类型
		//在调用时可以被人以类型替代
		public T XmlDeserialize<T>(string fileName)
		{
			//得到所需文件的stream
			using (var stream = GetStreamForFile (fileName))
			{
				return XmlDeserialize<T> (stream);
			}
		}
			
		public T XmlDeserialize<T>(Stream stream)
		{

			if (stream == null)
			{
				return default(T); // this happens if the file can't be found
			}
			else
			{
				//xml文件的反序列化用的是原生的函数
				//反序列化函数定义在序列化类XmlSerializer里
				//反序列化用的是自己写的
				XmlSerializer serializer = GetXmlSerializer<T>();

				//用泛类T定义返回的对象
				//用原生的反序列化函数把stream反序列化
				//得到想要的内容
				T objectToReturn;
				objectToReturn = (T)serializer.Deserialize(stream);

				return objectToReturn;
			}
		}

		XmlSerializer GetXmlSerializer<T>()
		{
			//得到一个以类型T为base类型的Serializer
			XmlSerializer newSerializer = new XmlSerializer(typeof(T));

			return newSerializer;
		}


		public Stream GetStreamForFile(string fileName)
		{
			#if ANDROID

			if(Activity == null)
			{
				throw new InvalidOperationException("Activity must first be set before deserializing on Android");
			}

			return Activity.Assets.Open(fileName);

			#else
			return Microsoft.Xna.Framework.TitleContainer.OpenStream (fileName);
			#endif
		}




	}
}


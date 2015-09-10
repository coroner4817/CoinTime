using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace CoinTimeGame.ContentLoading.Animations
{
	public class AnimationChainSave
	{
		public string Name;
		//每一个frame是动作的一步
		[XmlElementAttribute("Frame")]
		public List<AnimationFrameSave> Frames = new List<AnimationFrameSave>();

	}
}


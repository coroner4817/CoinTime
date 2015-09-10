using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace CoinTimeGame.ContentLoading.Animations
{
	[XmlType("AnimationChainArraySave")]
	public class AnimationChainListSave
	{

		//每一个chainlist是一个动作所有步的集合
		#region Fields
		[XmlElementAttribute("AnimationChain")]
		public List<AnimationChainSave> AnimationChains;
		#endregion
	}


}


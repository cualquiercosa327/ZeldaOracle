﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeldaOracle.Common.Scripting {
	/// <summary>An object contianing a collection of ZeldaAPI variables.</summary>
	public interface IVariableObject {

		/// <summary>Gets the collection of Zelda variables.</summary>
		Variables Variables { get; }
	}
}

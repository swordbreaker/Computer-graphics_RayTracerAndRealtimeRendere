// CMathLib.h

#pragma once

using namespace System;

namespace CMathLib {

	public ref class CMathF
	{
	public:
		static float SqrtF(float x);
		static float Atan2(float x, float y);
		static float Acos(float x);
		static float Round(float x);
		//float PowF(float a, float b);
	};
}
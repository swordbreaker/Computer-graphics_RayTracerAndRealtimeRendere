// This is the main DLL file.

#include "stdafx.h"
#include "math.h"
#include "CMathLib.h"

float CMathLib::CMathF::SqrtF(float x)
{
	return sqrtf(x);
}

float CMathLib::CMathF::Atan2(float x, float y)
{
	return atan2f(x, y);
}

float CMathLib::CMathF::Acos(float x)
{
	return acosf(x);
}

float CMathLib::CMathF::Round(float x)
{
	return roundf(x);
}
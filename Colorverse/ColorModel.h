#pragma once

#include "CoreMinimal.h"

namespace Liberess
{
	typedef unsigned int WORD;
	typedef unsigned long DWORD;
	typedef DWORD COLORREF;
 
	#define RGB(r,g,b)   ((COLORREF)(((uint8)(r) | ((WORD)((uint8)(g))<<8)) | (((DWORD)(uint8)(b))<<16)))

	class COLORVERSE_API ColorModel
	{
	public:
		// RGB to CMYK()는 RGB -> CMYK 값 변환 함수
		static void RGBtoCMYK(float r, float g, float b, uint8& c, uint8& m, uint8& y, uint8& k);
	
		// CCMYK to RGB()는 CMYK ->  RGB 값 변환 함수
		static COLORREF CMYKtoRGB(uint8 c, uint8 m, uint8 y, uint8 k);

		static void ToCMYK(float r, float g, float b, float* cmyk)
		{
			float k = FMath::Min(255-r,FMath::Min(255-g,255-b));
			float c = 255 * (255-r-k) / (255-k); 
			float m = 255 * (255-g-k) / (255-k); 
			float y = 255 * (255-b-k) / (255-k); 

			cmyk[0] = c;
			cmyk[1] = m;
			cmyk[2] = y;
			cmyk[3] = k;
		}

		static void ToRGB(float c, float m, float y, float k, float *rgb)
		{
			rgb[0] = -((c * (255-k)) / 255 + k - 255);
			rgb[1] = -((m * (255-k)) / 255 + k - 255);
			rgb[2] = -((y * (255-k)) / 255 + k - 255);
		}

		static float GetRValue(COLORREF rgb)
		{
			return (static_cast<uint8>(rgb)) / 255.0f;
		}

		static float GetGValue(COLORREF rgb)
		{
			return (static_cast<uint8>(static_cast<WORD>(rgb) >> 8)) / 255.0f;
		}

		static float GetBValue(COLORREF rgb)
		{
			return (static_cast<uint8>(rgb >> 16)) / 255.0f;
		}
	};
}


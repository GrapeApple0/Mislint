#pragma once
#include "decode.h"
#include "demux.h"
#include "mux_types.h"
#include "types.h"
//#include "png.h"

extern "C"
{
	__declspec(dllexport) WebPAnimDecoder* CallWebPAnimDecoderNewInternal(const WebPData* webp_data, const WebPAnimDecoderOptions* dec_options, int abi_version);
	__declspec(dllexport) int CallWebPAnimDecoderGetInfo(const WebPAnimDecoder* dec, WebPAnimInfo* info);
	__declspec(dllexport) int CallWebPAnimDecoderHasMoreFrames(const WebPAnimDecoder* dec);
	__declspec(dllexport) int CallWebPAnimDecoderGetNext(WebPAnimDecoder* dec, uint8_t** buf_ptr, int* timestamp_ptr);
	__declspec(dllexport) void CallWebPAnimDecoderDelete(WebPAnimDecoder* dec);
	__declspec(dllexport) int CleanupTransparentPixels(uint32_t* rgba, uint32_t width, uint32_t height);
	/*__declspec(dllexport) png_structp CallPNGCreateReadStruct(png_const_charp user_png_ver, png_voidp error_ptr, png_error_ptr error_fn, png_error_ptr warn_fn);
	__declspec(dllexport) png_infop CallPNGCreateInfoStruct(png_structp png_ptr);*/
}
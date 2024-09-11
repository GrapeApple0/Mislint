#include <iostream>
#include <windows.h>
#include <stdio.h>
#include "lib.h"

int main()
{
	return 0;
}

int width, height;
png_byte color_type;
png_byte bit_depth;

typedef struct png_buffer {
	unsigned char* data;    /* PNGのデータが入ったメモリへのポインタ */
	unsigned long data_len; /* PNGのデータの長さ。 */
	unsigned long data_offset;  /* 読み込み時用のオフセット */
} png_buffer;

void png_memread_func(png_structp png_ptr, png_bytep buf, png_size_t size) {
	/* 読み込むメモリの長さを表示。 */
	/* ストリームとして指定したデータを取得。 */
	png_buffer* png_buff = (png_buffer*)png_get_io_ptr(png_ptr);
	/* 読み込むメモリの長さとオフセット（今までに読み込んだメモリの長さ）が
	 * データの長さを越えていないかチェック。 */
	if (png_buff->data_offset + size <= png_buff->data_len) {
		/* メモリのコピー */
		memcpy(buf, png_buff->data + png_buff->data_offset, size);
		/* オフセットに今し方読み込んだメモリの長さを追加。 */
		png_buff->data_offset += size;
		/* オフセットを表示 */
		//printf("memRead - offset =\t%08ld bytes\n", png_buff->data_offset);
	}
	else {
		png_error(png_ptr, "png_mem_read_func failed");
	}
}

void read_png_file(char* filename) {
	png_buffer png_buf;

	png_structp png = png_create_read_struct(PNG_LIBPNG_VER_STRING, NULL, NULL, NULL);
	if (!png) abort();

	png_infop info = png_create_info_struct(png);
	if (!info) abort();

	if (setjmp(png_jmpbuf(png))) abort();

	//png_init_io(png, fp);
	png_set_read_fn(png, (png_voidp)&png_buf, (png_rw_ptr)png_memread_func);
	png_read_info(png, info);

	width = png_get_image_width(png, info);
	height = png_get_image_height(png, info);
	color_type = png_get_color_type(png, info);
	bit_depth = png_get_bit_depth(png, info);

	// Read any color_type into 8bit depth, RGBA format.
	// See http://www.libpng.org/pub/png/libpng-manual.txt

	if (bit_depth == 16)
		png_set_strip_16(png);

	if (color_type == PNG_COLOR_TYPE_PALETTE)
		png_set_palette_to_rgb(png);

	// PNG_COLOR_TYPE_GRAY_ALPHA is always 8 or 16bit depth.
	if (color_type == PNG_COLOR_TYPE_GRAY && bit_depth < 8)
		png_set_expand_gray_1_2_4_to_8(png);

	if (png_get_valid(png, info, PNG_INFO_tRNS))
		png_set_tRNS_to_alpha(png);

	// These color_type don't have an alpha channel then fill it with 0xff.
	if (color_type == PNG_COLOR_TYPE_RGB ||
		color_type == PNG_COLOR_TYPE_GRAY ||
		color_type == PNG_COLOR_TYPE_PALETTE)
		png_set_filler(png, 0xFF, PNG_FILLER_AFTER);

	if (color_type == PNG_COLOR_TYPE_GRAY ||
		color_type == PNG_COLOR_TYPE_GRAY_ALPHA)
		png_set_gray_to_rgb(png);

	png_read_update_info(png, info);

	png_bytep* row_pointers = (png_bytep*)malloc(sizeof(png_bytep) * height);
	for (int y = 0; y < height; y++) {
		row_pointers[y] = (png_byte*)malloc(png_get_rowbytes(png, info));
	}

	png_read_image(png, row_pointers);

	png_destroy_read_struct(&png, &info, NULL);
}

int CleanupTransparentPixels(uint32_t* rgba, uint32_t width, uint32_t height) {
	const uint32_t* const rgba_end = rgba + width * height;
	while (rgba < rgba_end) {
		const uint8_t alpha = (*rgba >> 24) & 0xff;
		if (alpha == 0) {
			*rgba = 0;
		}
		++rgba;
	}
	return 0;
}

// libpng

png_structp CallPNGCreateReadStruct(png_const_charp user_png_ver, png_voidp error_ptr, png_error_ptr error_fn, png_error_ptr warn_fn)
{
	return png_create_read_struct(user_png_ver, error_ptr, error_fn, warn_fn);
}

png_infop CallPNGCreateInfoStruct(png_structp png_ptr)
{
	return png_create_info_struct(png_ptr);
}

void CallPNGReadInfo(png_structp png_ptr, png_infop info_ptr)
{
	png_read_info(png_ptr, info_ptr);
}

png_buffer* CallPNGGetIOPtr(png_structp png_ptr)
{
	return (png_buffer*)png_get_io_ptr(png_ptr);
}

int CallPNGGetImageWidth(png_structp png_ptr, png_infop info_ptr)
{
	return png_get_image_width(png_ptr, info_ptr);
}

int CallPNGGetImageHeight(png_structp png_ptr, png_infop info_ptr)
{
	return png_get_image_height(png_ptr, info_ptr);
}

png_byte CallPNGGetColorType(png_structp png_ptr, png_infop info_ptr)
{
	return png_get_color_type(png_ptr, info_ptr);
}

png_byte CallPNGGetBitDepth(png_structp png_ptr, png_infop info_ptr)
{
	return png_get_bit_depth(png_ptr, info_ptr);
}

void CallPngSetStrip16(png_structp png_ptr)
{
	png_set_strip_16(png_ptr);
}

void CallPngSetPaletteToRGB(png_structp png_ptr)
{
	png_set_palette_to_rgb(png_ptr);
}

// libwebp
WebPAnimDecoder* CallWebPAnimDecoderNewInternal(const WebPData* webp_data, const WebPAnimDecoderOptions* dec_options, int abi_version)
{
	return WebPAnimDecoderNewInternal(webp_data, dec_options, abi_version);
}

int CallWebPAnimDecoderGetInfo(const WebPAnimDecoder* dec, WebPAnimInfo* info)
{
	return WebPAnimDecoderGetInfo(dec, info);
}

int CallWebPAnimDecoderHasMoreFrames(const WebPAnimDecoder* dec) {
	return WebPAnimDecoderHasMoreFrames(dec);
}

int CallWebPAnimDecoderGetNext(WebPAnimDecoder* dec, uint8_t** buf_ptr, int* timestamp_ptr) {
	return WebPAnimDecoderGetNext(dec, buf_ptr, timestamp_ptr);
}

void CallWebPAnimDecoderDelete(WebPAnimDecoder* dec) {
	return WebPAnimDecoderDelete(dec);
}
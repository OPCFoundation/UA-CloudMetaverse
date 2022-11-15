// Empty function file for solving static linking errors when building WSA applications 

#pragma once

#include <stddef.h>
#include <stdint.h>

#define EXTERN_API extern "C"

// JpegXR functions

EXTERN_API int JPEGXR_Decompress(void* source, int srcLength, void* dst, int dstLength) { return 0; }

// Basis Universal functions

EXTERN_API int BASIS_Transcode(void* source, int srcLength, int textureFormat, uint8_t* output, int* transcodedSize) { return 0; }

// ZStandard functions

EXTERN_API void* ZSTD_createDCtx() { return nullptr; }

EXTERN_API size_t ZSTD_freeDCtx(void* cctx) { return 0; }

EXTERN_API size_t ZSTD_decompressDCtx(void* ctx, void* dst, size_t dstCapacity, void* src, size_t srcSize) { return 0; }

EXTERN_API void* ZSTD_createDDict(void* dict, size_t dictSize) { return nullptr; }

EXTERN_API size_t ZSTD_freeDDict(void* ddict) { return 0; }

EXTERN_API size_t ZSTD_decompress_usingDDict(void* dctx, void* dst, size_t dstCapacity, void* src, size_t srcSize, void* ddict) { return 0; }

EXTERN_API void* ZSTD_getDecompressedSize(void* src, size_t srcSize) { return nullptr; }

EXTERN_API uint32_t ZSTD_isError(size_t code) { return 0; }

EXTERN_API void* ZSTD_getErrorName(size_t code) { return nullptr; }
#pragma once

#ifndef _AraTrustFinger_H_
#define _AraTrustFinger_H_

#define AraHANDLE void*
//return code
//General code
#define  GEN_SUCCEDED				   0    //OK 
#define  GEN_PARAM_NULL				   10

#define  GEN_PAR_ERR				-900
#define  GEN_MEM_ERR				-901
#define  GEN_FUC_FAIL				-902
#define  GEN_ILLEGAL_ERR			-903
#define  GEN_FIRMWARE_ERR			-904
#define  GEN_INIT_FAIL              -905
#define  GEN_OTHER_ERR				-910

//Device error code
#define  DEV_NOT_FIND				-100
#define  DEV_NOT_AUTHORIZE          -101
#define  DEV_NOT_INIT				-102
#define  DEV_NOT_OPEN				-103
#define  DEV_NOT_SUPPORT			-104

#define  DEV_LED_FAIL               -106
#define  DEV_BEGIN_FAIL				-110
#define  DEV_NOT_CALL_BEGIN	        -111
#define  DEV_GET_IMA_FAIL			-112
#define  DEV_GET_IMA_TIMEOUT		-113
#define  DEV_OCCUPY_OTHER		    -114
#define  SDK_INIT_DONE		        -115
#define  DEVICE_OPENED		        -117
#define  DEVICE_DESC_FAIL		    -118
#define  DEV_COMMUNICATE_FAIL       -119
#define  DEV_COMMUNICATE_UNSUPPORT  -120
#define  DEV_INSTRUCTION_ERROR      -121
#define  DEV_ENCODE_FAIL            -122
#define  DEV_DECODE_FAIL            -123
#define  DEV_TOO_DRY                -124
#define  DEV_TOO_WET                -125
#define  DEV_LITTLE_FEATURE         -126
#define  DEV_LIVE_CAPTURING         -127
#define  DEV_LIVE_NOT_BEGIN         -128
#define  DEV_LFD_FAIL               -129
#define  DEV_FAKE_FINGERPRINT       -130
#define  DEV_UNKNOWN_FINGERPRINT    -131
#define  DEV_NO_FINGERPRINT         -132
#define  DEV_SEGMENT_FINGER_FAIL    -133
#define  DEV_GET_DEVICETYPE_FAIL    -134
#define  DEV_SHOW_IMAGE_BYID_FAIL   -135
#define  DEV_OPERATION_TYPE_UNSUPPORT  -136
#define  DEV_TYPE_UNSUPPORT			-137
#define  DEV_LIVE_ENDLIVECAPTURE      11   
#define  DEV_SET_REGISTER_FAIL		-138
#define  DEV_GET_REGISTER_FAIL		-139

//Arithmetic
#define   ALG_INIT_FAIL		        -200 
#define   ALG_NO_WSQ                -201   
#define   ALG_RAWTOWSQ_FAIL         -202   
#define   ALG_NO_NFIQ               -203   
#define   ALG_RAWTOISO_FAIL         -204   
#define   ALG_RAWTOANSI_FAIL        -205   
#define   ALG_RAWTOJPG_FAIL         -206   
#define   ALG_NO_JPG                -207   
#define   ALG_WSQTORAW_FAIL         -208   

#define   ALG_IMG_BUF_NO_DATA		-210
#define   ALG_IMG_QUALITY_LOW		-211
#define   ALG_INVALID_BMP_DATA		-212   
#define   ALG_BIONE_NOT_INIT        -220
#define   ALG_EXTRACT_FEATURE_FAIL  -221
#define   ALG_GEN_TEMPLATE_FAIL     -222

typedef struct _ARAFP_DEVICEDESC
{
	char serialNumber[32]; /* Device serial number */
	char manufacturer[32]; /* manufacturer */
	char productName[64]; /* Device product name */
	char productModel[32]; /* Device product model */
	char fwVersion[32]; /* Device firmware version */
	char hwVersion[32]; /* Device revision */

	int imageWidth; /*Device capture max image width*/
	int imageHeight;/*Device capture max image height*/
	int Dpi; /*Device capture max image DPI*/
	unsigned short deviceId; /*Device id*/

	bool isUSBSupported; // device support USE connected type or not
	bool isUARTSupported; //device suppport UART or not
	bool isSPISupported; //device support SPI or not
	bool isLedSupported; //device support led or not

	bool isTrustMeSupported;
	bool isTrustAFISSupported;
	bool isTrustLinkSupported;
	unsigned char Reserved0;
	unsigned char Reserved1;
	unsigned char Reserved2;
}ARAFP_DevDesc;

#pragma pack (1)
typedef struct SEG_FPINFO_STRUCT
{
	int     m_centerY; 
	int     m_centerX;
	int     m_centerD;
	int     m_quality;
	int     m_ulY;
	int     m_ulX;
	int     m_urY;
	int     m_urX;
	int     m_llY;
	int     m_llX;
	int     m_lrY;
	int     m_lrX;
}SEG_FPInfo_Struct;

typedef struct
{
	unsigned int	m_unSubFingerWidth;
	unsigned int	m_unSubFingerHeight;
	unsigned int	m_unFingerTopLeftX;
	unsigned int    m_unFingerTopLeftY;
	unsigned int    m_unRollPostionX;
	unsigned int    m_unQuality;
	unsigned int    m_unFingerPos;
	unsigned int	m_unimgContrast;
	unsigned int	m_unFeatureLength;
	unsigned int	m_unFirLength;
	unsigned char	*pSegmentImagePtr;
	unsigned char	*pszFeatureData;
	unsigned char	*pszFirData;

	SEG_FPInfo_Struct m_segFpinfo;
}FP_SegmentImagDesc;

typedef struct
{
	unsigned int  m_unOperationType;
	unsigned int  m_unFeatureFormat;
	unsigned int  m_unDuration;
	unsigned int  m_unIQThreshold;
	unsigned int  m_unConThreshold;
	unsigned int  m_unCutImgW;
	unsigned int  m_unCutImgH;
}MultiFingerParam;

#pragma pack()

typedef enum EnumOperationTypeTag
{
	/********************************************************************
	*	FLAT FINGERS													*
	********************************************************************/
	ARAFPSCAN_FLAT_RIGHT_THUMB_FINGER	= 1,		//  RIGHT THUMB FINGER 
	ARAFPSCAN_FLAT_RIGHT_INDEX_FINGER	= 2,		//  RIGHT INDEX FINGER
	ARAFPSCAN_FLAT_RIGHT_MIDDLE_FINGER	= 3,		//  RIGHT MIDDLE FINGER
	ARAFPSCAN_FLAT_RIGHT_RING_FINGER	= 4,		//  RIGHT RING FINGER
	ARAFPSCAN_FLAT_RIGHT_LITTLE_FINGER	= 5,		//  RIGHT LITTLE FINGER
	ARAFPSCAN_FLAT_LEFT_THUMB_FINGER	= 6,		//  LEFT THUMB FINGER
	ARAFPSCAN_FLAT_LEFT_INDEX_FINGER	= 7,		//  LEFT INDEX FINGER
	ARAFPSCAN_FLAT_LEFT_MIDDLE_FINGER	= 8,		//  MIDDLE FINGER
	ARAFPSCAN_FLAT_LEFT_RING_FINGER		= 9,		//  LEFT RING FINGER
	ARAFPSCAN_FLAT_LEFT_LITTLE_FINGER	= 10,		//  LEFT LITTLE FINGER
	/********************************************************************
	*	ROLL FINGERS													*
	********************************************************************/
	ARAFPSCAN_ROLL_RIGHT_THUMB_FINGER	= 11,		//  ROLLED RIGHT THUMB FINGER
	ARAFPSCAN_ROLL_RIGHT_INDEX_FINGER	= 12,		//  ROLLED RIGHT INDEX FINGER
	ARAFPSCAN_ROLL_RIGHT_MIDDLE_FINGER	= 13,		//  ROLLED RIGHT MIDDLE FINGER
	ARAFPSCAN_ROLL_RIGHT_RING_FINGER	= 14,		//  ROLLED RIGHT RING FINGER
	ARAFPSCAN_ROLL_RIGHT_LITTLE_FINGER	= 15,		//  ROLLED RIGHT LITTLE FINGER
	ARAFPSCAN_ROLL_LEFT_THUMB_FINGER	= 16,		//  ROLLED LEFT THUMB FINGER
	ARAFPSCAN_ROLL_LEFT_INDEX_FINGER	= 17,		//  ROLLED LEFT INDEX FINGER
	ARAFPSCAN_ROLL_LEFT_MIDDLE_FINGER	= 18,		//  ROLLED LEFT MIDDLE FINGER
	ARAFPSCAN_ROLL_LEFT_RING_FINGER		= 19,		//  ROLLED LEFT RING FINGER
	ARAFPSCAN_ROLL_LEFT_LITTLE_FINGER	= 20,		//  ROLLED LEFT LITTLE FINGER

	/********************************************************************
	*	SLAPS															*
	********************************************************************/
	ARAFPSCAN_SLAP_2_THUMBS_FINGERS		= 21,		// THUMB LEFT + THUMB RIGHT
	ARAFPSCAN_SLAP_4_LEFT_FINGERS		= 22,		// LEFT HAND 4 Fingers:  INDEX+MIDDLE+RING+LITTLE 
	ARAFPSCAN_SLAP_4_RIGHT_FINGERS		= 23,		// RIGHT HAND 4 Fingers: INDEX+MIDDLE+RING+LITTLE 
	ARAFPSCAN_SLAP_ANY_NUM_FINGER		= 24,
	ARAFPSCAN_SLAP_ANY_TWO_FINGERS		= 25

}tEnumOperationType;

//unFeatureFormat
#define bmBIT(n)	(1<<n)
#define ARAFP_TYPE_FMR_NONE								bmBIT(0)
#define ARAFP_TYPE_FMR_BIONE							bmBIT(1)
#define ARAFP_TYPE_FMR_ISO_2005							bmBIT(2)
#define ARAFP_TYPE_FMR_IDCARD							bmBIT(3)
#define ARAFP_TYPE_FMR_ANSI								bmBIT(4)
#define ARAFP_TYPE_FMR_ISO_2011							bmBIT(5)
#define ARAFP_TYPE_FMR_FMCCS								bmBIT(6)
#define ARAFP_TYPE_FMR_BIONE_TL							bmBIT(7)

#define ARAFP_TYPE_FIR_2005_RAW							bmBIT(16)
#define ARAFP_TYPE_FIR_2005_PNG							bmBIT(17)
#define ARAFP_TYPE_FIR_2005_JPEG						bmBIT(18)
#define ARAFP_TYPE_FIR_2005_WSQ_10_1					bmBIT(19)
#define ARAFP_TYPE_FIR_2005_WSQ_15_1					bmBIT(20)
#define ARAFP_TYPE_FIR_2005_JPEG2000_COMPRESS_LV1		bmBIT(21)
#define ARAFP_TYPE_FIR_2005_JPEG2000_COMPRESS_LV2		bmBIT(22)
#define ARAFP_TYPE_FIR_2011_RAW							bmBIT(23)
#define ARAFP_TYPE_FIR_2011_PNG							bmBIT(24)
#define ARAFP_TYPE_FIR_2011_JPEG						bmBIT(25)
#define ARAFP_TYPE_FIR_2011_WSQ_10_1					bmBIT(26)
#define ARAFP_TYPE_FIR_2011_WSQ_15_1					bmBIT(27)
#define ARAFP_TYPE_FIR_2011_JPEG2000_COMPRESS_LV1		bmBIT(28)
#define ARAFP_TYPE_FIR_2011_JPEG2000_COMPRESS_LV2		bmBIT(29)


#define SEGMENTED_IMAG_MAX_WIDTH		800
#define SEGMENTED_IMAG_MAX_HEIGHT		800

#define ARAFP_MISSING_FINGER_SLAP4_INDEX				0x01	//index finger is missing	0001
#define ARAFP_MISSING_FINGER_SLAP4_MIDDLE				0x02	//middle finger is missing	0010
#define ARAFP_MISSING_FINGER_SLAP4_RING					0x04	//ring finger is missing	0100
#define ARAFP_MISSING_FINGER_SLAP4_LITTLE				0x08	//little finger is missing	1000

#define ARAFP_MISSING_INDEX_AND_MIDDLE_FINGER			0x03	//index middle finger is missing	0011
#define ARAFP_MISSING_INDEX_AND_RING_FINGER				0x05	//index ring finger is missing		0101
#define ARAFP_MISSING_MIDDLE_AND_RING_FINGER			0x06	//middle ring finger is missing		0110
#define ARAFP_MISSING_INDEX_AND_LITTLE_FINGER			0x09	//index little finger is missing	1001
#define ARAFP_MISSING_MIDDLE_AND_LITTLE_FINGER			0x0A	//middle little finger is missing	1010
#define ARAFP_MISSING_RING_AND_LITTLE_FINGER			0x0C	//ring little finger is missing		1100

//Function

int __stdcall ARAFPSCAN_GlobalInit();

int __stdcall ARAFPSCAN_GlobalFree();

int __stdcall ARAFPSCAN_GetDeviceCount(int *nDeviceCount);

int __stdcall ARAFPSCAN_OpenDevice(AraHANDLE *nHandle,int nIndex);

int __stdcall ARAFPSCAN_CloseDevice(AraHANDLE *nHandle);

int __stdcall ARAFPSCAN_GetDeviceDescription(const int deviceIndex, ARAFP_DevDesc *pDeviceDesc);

int __stdcall ARAFPSCAN_SetLedStatus(AraHANDLE nHandle, int nLedIndex, int pStatus);

int __stdcall ARAFPSCAN_GetLedStatus(AraHANDLE nHandle, int nLedIndex, int *pnStatus);

int __stdcall ARAFPSCAN_GetErrorInfo(int nErrorNo, char pszErrorInfo[256]);

int __stdcall ARAFPSCAN_Verify(AraHANDLE nHandle, int Security_Level, unsigned char * pFeatureData, unsigned char * pTemplateData, int *sc,int *presult);

int __stdcall ARAFPSCAN_EnableLFD(AraHANDLE nHandle, bool pEnableLFD, int LFD_Level);

typedef enum EnImgQualityTypeTag
{
	ARATEK_IQ = 0,
	NFIQ1     = 1,
	NFIQ2     = 2,
}EnImgQualityType;
int __stdcall ARAFPSCAN_GetNFIQ(HANDLE nHandle, unsigned char *paszImagePtr, int nWidth, int nHeight, EnImgQualityType enImgQualityType, BYTE *Quality);


typedef enum EnRawConvertImgTypeTag
{
	RAW_TO_BMP_TYPE = 0,
	RAW_TO_WSQ_TYPE = 1,
	RAW_TO_ISO_TYPE = 2,
	RAW_TO_ANSI_TYPE = 3,
	RAW_TO_JPEG_TYPE = 4,
	RAW_TO_JPEG2000_TYPE = 5,
	BMP_TO_RAW_TYPE = 6,
}EnRawConvertImgType;

typedef struct _RAWDATA_CONVERT_PARAM
{
	double bitrate;
	float Factor;
	int FingerPos;
	int Imgcompressalg;
}Rawdata_convert_param;

int __stdcall ARAFPSCAN_ConvertImage(AraHANDLE nHandle, unsigned char *pInputData, int nWidth, int nHeight, EnRawConvertImgType enConvertType, Rawdata_convert_param stConvetParam, unsigned char *pOutputData, int *pLenOutput);


/////////////////////////////////Multi/////////////////////////////////////////
/* nOccurredEventCode */
#define ALGORITHM_PROCESS_SUCCESS_FLAG		0	//The segmented image is successful and has the same number as the target
#define LIVE_CAPTURE_IMAGE_DATA_FLAG		1	//Real-time image collection/rolling upload image identification
#define DURATION_END_FLAG					2	//Duration Expiration Stop Sign
#define EXTRACT_FEATURE_END_FLAG			3	//Feature extraction failure flag
#define ROLL_IMAGE_STOP_FLAG				4	//Roll stopped
#define ROLL_IMAGE_AREA_SMALL_END_FLAG		5	//Rolling mining area is too small to stop
#define LFDCHECK_FAIL_END_FLAG				6	//Liveness detection failed stop sign
#define SEGMENT_HAND_CHECK_FAIL				7	//Failed to judge the left and right hands separately
#define ROLLED_IMAGE_DISCONTINUITY			8	//Rolling images are discontinuous
#define ROLLED_IMAGE_BACK					9	//Rollback
#define ROLLED_MULTI_FINGER					10	//multi-finger rolling
#define CAPTURE_STOP						11	//capture stop
#define CAPTURE_FAIL						12	//capture failed due to too low contrast
#define CONTRAST_LOW_RETRY				    13	//retry due to low contrast
#define IMAGE_QUALITY_LOW_RETRY				14	//retry due to low image quality

typedef int(__stdcall *ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback)(int nOccurredEventCode, unsigned char *pszFramePtr, int nFrameWidth, int nFrameHeight,
	FP_SegmentImagDesc *pSegmentImageList, unsigned int unNumSegmentImage);

int __stdcall ARAFPSCAN_MultiFingerStartAcquisition(HANDLE handle, MultiFingerParam MFparam, ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback captureEventsCallbackPtr);

int __stdcall ARAFPSCAN_MultiFingerStopAcquisition(AraHANDLE handle);

int __stdcall ARAFPSCAN_MultiFingerSetMissingFingers(unsigned char byMissingFingerMask);
//////////////////////////////////////////////////////////////////////////

#endif
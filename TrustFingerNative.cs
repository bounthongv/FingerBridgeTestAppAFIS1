using System;
using System.Runtime.InteropServices;

namespace FingerBridge.Native
{
    /// <summary>
    /// Provides native interop functionality for the TrustFinger fingerprint scanning device.
    /// This class wraps the native TrustFinger.dll functions for fingerprint capture, verification,
    /// and feature extraction operations.
    /// </summary>
    public static class TrustFingerNative
    {
        private const string DllName = "TrustFinger.dll";

        /// <summary>
        /// Initializes the TrustFinger SDK globally. This must be called before any other operations.
        /// </summary>
        /// <returns>
        /// 0 on success, -115 if SDK is already initialized, or other error codes on failure.
        /// </returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_GlobalInit();

        /// <summary>
        /// Releases all resources allocated by the TrustFinger SDK.
        /// Should be called when the application is shutting down.
        /// </summary>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_GlobalFree();

        /// <summary>
        /// Gets the number of connected TrustFinger devices.
        /// </summary>
        /// <param name="count">Reference to variable that will receive the device count.</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_GetDeviceCount(ref int count);

        /// <summary>
        /// Opens a connection to a TrustFinger device.
        /// </summary>
        /// <param name="handle">Reference to variable that will receive the device handle.</param>
        /// <param name="index">Zero-based index of the device to open.</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_OpenDevice(ref IntPtr handle, int index);

        /// <summary>
        /// Closes the connection to a TrustFinger device.
        /// </summary>
        /// <param name="handle">Reference to the device handle to close.</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_CloseDevice(ref IntPtr handle);

        /// <summary>
        /// Starts the multi-finger acquisition process.
        /// </summary>
        /// <param name="handle">Handle to the opened device.</param>
        /// <param name="param">Parameters controlling the acquisition process.</param>
        /// <param name="callback">Callback function to receive acquisition events and image data.</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_MultiFingerStartAcquisition(
            IntPtr handle,
            MultiFingerParam param,
            ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback callback
        );

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_MultiFingerStopAcquisition(IntPtr handle);

        /// <summary>
        /// Verifies two fingerprint templates for matching.
        /// </summary>
        /// <param name="algorithmType">The algorithm type to use (e.g., 4 for ISO2011).</param>
        /// <param name="templateOne">First fingerprint template to compare.</param>
        /// <param name="templateTwo">Second fingerprint template to compare.</param>
        /// <param name="similarity">Output parameter receiving the similarity score (0-100).</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_Verify(
            int algorithmType,
            [In] byte[] templateOne,
            [In] byte[] templateTwo,
            out int similarity);

        /// <summary>
        /// Extracts features from a raw fingerprint template using the specified format.
        /// </summary>
        /// <param name="rawTemplate">Raw fingerprint template data.</param>
        /// <param name="templateLength">Length of the raw template data.</param>
        /// <param name="featureBuffer">Buffer to receive the extracted features.</param>
        /// <param name="featureLength">Reference to the length of the feature buffer.</param>
        /// <param name="formatType">Format type for feature extraction (e.g., 4 for ISO2011).</param>
        /// <returns>0 on success, or error code on failure.</returns>
        [DllImport("TrustFinger.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_ExtractFeatureEx(
            [In] byte[] rawTemplate,
            int templateLength,
            [Out] byte[] featureBuffer,
            ref int featureLength,
            int formatType
        );

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_ExtractFeature(
            [In] byte[] rawTemplate,
            int templateLength,
            [Out] byte[] featureBuffer,
            ref int featureLength);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_GetSegmentImageList(
            IntPtr handle,
            out IntPtr segmentImageList,
            out uint numSegmentImage);
        // Helper method to perform verification and return a structured result
        [DllImport("TrustFinger.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_ExtractISO2011Feature(
            byte[] image,
            int width,
            int height,
            [Out] byte[] featureBuffer,
            ref int featureLength
        );
        [DllImport("TrustFinger.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_ConvertImgToFeature(
            [In] byte[] rawImage,
            int width,
            int height,
            [Out] byte[] featureBuffer,
            ref int featureLength,
            int formatType // try 4
        );

        [DllImport("TrustFinger.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int ARAFPSCAN_CaptureISOData(
            IntPtr deviceHandle,
            uint fingerIndex,
            [Out] byte[] templateBuffer,
            ref int templateLength,
            int timeout
        );

        /// <summary>
        /// Helper method to extract fingerprint features from a raw template.
        /// </summary>
        /// <param name="rawTemplate">The raw fingerprint template data.</param>
        /// <param name="formatType">The format type for feature extraction (defaults to 4 for ISO2011).</param>
        /// <returns>Byte array containing the extracted features.</returns>
        /// <exception cref="Exception">Thrown when feature extraction fails.</exception>
        public static byte[] ExtractFeatures(byte[] rawTemplate, int formatType = 4)
        {
            byte[] featureBuffer = new byte[1024]; // Adjust size if needed
            int featureLength = featureBuffer.Length;

            int result = ARAFPSCAN_ExtractFeatureEx(rawTemplate, rawTemplate.Length, featureBuffer, ref featureLength, formatType);
            if (result != 0 || featureLength <= 0)
                throw new Exception("Feature extraction failed");

            byte[] finalFeature = new byte[featureLength];
            Array.Copy(featureBuffer, finalFeature, featureLength);
            return finalFeature;
        }

        public static VerifyResult PerformVerification(byte[] capturedTemplate, byte[] enrolledTemplate, int threshold = 60)
        {
            try
            {
                var feature1 = ExtractFeatures(capturedTemplate, 4);
                var feature2 = ExtractFeatures(enrolledTemplate, 4);

                int similarity = 0;
                int result = ARAFPSCAN_Verify(4, feature1, feature2, out similarity);

                return new VerifyResult
                {
                    IsMatch = result == 0 && similarity >= threshold,
                    Similarity = similarity
                };
            }
            catch (Exception)
            {
                return new VerifyResult
                {
                    IsMatch = false,
                    Similarity = 0
                };
            }
        }

    }

    /// <summary>
    /// Parameters for multi-finger acquisition process.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MultiFingerParam
    {
        public uint OperationType;
        public uint FeatureFormat;
        public uint Duration;
        public uint IQThreshold;
        public uint ConThreshold;
        public uint CutImgW;
        public uint CutImgH;
    }

    /// <summary>
    /// Callback delegate for multi-finger acquisition events.
    /// </summary>
    /// <param name="eventCode">Event type code.</param>
    /// <param name="framePtr">Pointer to the frame data.</param>
    /// <param name="frameWidth">Width of the frame in pixels.</param>
    /// <param name="frameHeight">Height of the frame in pixels.</param>
    /// <param name="segmentImageList">Pointer to the list of segmented finger images.</param>
    /// <param name="numSegmentImage">Number of segmented images in the list.</param>
    /// <returns>0 for success, non-zero for error.</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int ARAFPSCAN_MultiFingerAcquisitionEventsManagerCallback(
        int eventCode,
        IntPtr framePtr,
        int frameWidth,
        int frameHeight,
        IntPtr segmentImageList,
        uint numSegmentImage
    );

    /// <summary>
    /// Represents the result of a fingerprint verification operation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class VerifyResult
    {
        /// <summary>Indicates whether the fingerprints match based on the verification threshold.</summary>
        public bool IsMatch { get; set; }
        /// <summary>The similarity score between the two fingerprints (0-100).</summary>
        public int Similarity { get; set; }
    }

    /// <summary>
    /// Contains information about a segmented fingerprint image, including position and quality metrics.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SEG_FPInfo_Struct
    {
        public int m_centerY;
        public int m_centerX;
        public int m_centerD;
        public int m_quality;
        public int m_ulY;
        public int m_ulX;
        public int m_urY;
        public int m_urX;
        public int m_llY;
        public int m_llX;
        public int m_lrY;
        public int m_lrX;
    }

    /// <summary>
    /// Describes a segmented fingerprint image, including dimensions, position, quality, and associated data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FP_SegmentImagDesc
    {
        /// <summary>Width of the segmented fingerprint image.</summary>
        public uint m_unSubFingerWidth;
        /// <summary>Height of the segmented fingerprint image.</summary>
        public uint m_unSubFingerHeight;
        /// <summary>X-coordinate of the top-left corner of the fingerprint.</summary>
        public uint m_unFingerTopLeftX;
        /// <summary>Y-coordinate of the top-left corner of the fingerprint.</summary>
        public uint m_unFingerTopLeftY;
        /// <summary>X-coordinate of the roll position.</summary>
        public uint m_unRollPostionX;
        /// <summary>Quality score of the fingerprint image.</summary>
        public uint m_unQuality;
        /// <summary>Position identifier of the finger.</summary>
        public uint m_unFingerPos;
        /// <summary>Contrast value of the image.</summary>
        public uint m_unimgContrast;
        /// <summary>Length of the extracted feature data.</summary>
        public uint m_unFeatureLength;
        /// <summary>Length of the FIR (Fingerprint Image Record) data.</summary>
        public uint m_unFirLength;
        /// <summary>Pointer to the segmented image data.</summary>
        public IntPtr pSegmentImagePtr;
        /// <summary>Pointer to the extracted feature data.</summary>
        public IntPtr pszFeatureData;
        /// <summary>Pointer to the FIR data.</summary>
        public IntPtr pszFirData;
        /// <summary>Detailed information about the segmented fingerprint.</summary>
        public SEG_FPInfo_Struct m_segFpinfo;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DevicePerformanceUtil
{
    public static DevicePerformanceLevel GetDevicePerformanceLevel()
    {
        if (SystemInfo.graphicsDeviceVendorID == 32902)
        {
            return DevicePerformanceLevel.VeryLow;
        }
        else 
        {
            if (SystemInfo.processorCount <= 2)
            {
                return DevicePerformanceLevel.VeryLow;
            }
            else
            {
                int graphicsMemorySize = SystemInfo.graphicsMemorySize;
                int systemMemorySize = SystemInfo.systemMemorySize;
                if (graphicsMemorySize >= 6000 && systemMemorySize >= 12000)
                    return DevicePerformanceLevel.Ultra;
                else if (graphicsMemorySize >= 5000 && systemMemorySize >= 10000)
                    return DevicePerformanceLevel.VeryHigh;
                else if (graphicsMemorySize >= 4000 && systemMemorySize >= 8000)
                    return DevicePerformanceLevel.High;
                else if (graphicsMemorySize >= 3000 && systemMemorySize >= 6000)
                    return DevicePerformanceLevel.Medium;
                else if (graphicsMemorySize >= 2000 && systemMemorySize >= 4000)
                    return DevicePerformanceLevel.Low;
                else
                    return DevicePerformanceLevel.VeryLow;
            }
        }
    }

    public static void ModifySettingsBasedOnPerformance(int veryLowQuality, int lowQuality, int midQuality, int highQuality, int veryHighQuality, int ultraQuality)
    {
        DevicePerformanceLevel level = GetDevicePerformanceLevel();
        switch (level)
        {
            case DevicePerformanceLevel.VeryLow:
                QualitySettings.SetQualityLevel(veryLowQuality, true);
                break;
            case DevicePerformanceLevel.Low:
                QualitySettings.SetQualityLevel(lowQuality, true);
                break;
            case DevicePerformanceLevel.Medium:
                QualitySettings.SetQualityLevel(midQuality, true);
                break;
            case DevicePerformanceLevel.High:
                QualitySettings.SetQualityLevel(highQuality, true);
                break;
            case DevicePerformanceLevel.VeryHigh:
                QualitySettings.SetQualityLevel(veryHighQuality, true);
                break;
            case DevicePerformanceLevel.Ultra:
                QualitySettings.SetQualityLevel(ultraQuality, true);
                break;
        }
    }

    public static void ModifySettingsBasedOnPerformance()
    {
        DevicePerformanceLevel level = GetDevicePerformanceLevel();
        switch (level)
        {
            case DevicePerformanceLevel.VeryLow:
                QualitySettings.SetQualityLevel((int) QualityLevel.VeryLow);
                break;
            case DevicePerformanceLevel.Low:
                QualitySettings.SetQualityLevel((int) QualityLevel.Low);
                break;
            case DevicePerformanceLevel.Medium:
                QualitySettings.SetQualityLevel((int) QualityLevel.Medium);
                break;
            case DevicePerformanceLevel.High:
                QualitySettings.SetQualityLevel((int) QualityLevel.High);
                break;
            case DevicePerformanceLevel.VeryHigh:
                QualitySettings.SetQualityLevel((int) QualityLevel.VeryHigh);
                break;
            case DevicePerformanceLevel.Ultra:
                QualitySettings.SetQualityLevel((int) QualityLevel.Ultra);
                break;
        }
    }

    public enum DevicePerformanceLevel
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Ultra

    }
    public enum QualityLevel
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Ultra
    }
}

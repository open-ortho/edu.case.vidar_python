using BFD9010.Scanner;
using Microsoft.Extensions.Logging;
using static BFD9010.Scanner.Scanner;
using static BFD9010.Scanner.VscsiTypes;

namespace BFD9010.FhirApi.Services;

/// <summary>
/// Scanner service that wraps the BFD9010.Scanner library
/// </summary>
public class ScannerService
{
    private _DIGITIZERINFO? _digitizerInfo;
    private ScannerData? _scannerData;
    private readonly ScanConfig _scanConfig;
    private readonly ILogger<ScannerService> _logger;
    private bool _isInitialized = false;

    public ScannerService(ILogger<ScannerService> logger)
    {
        _logger = logger;
        _scanConfig = ScanConfig.Load(null);
    }

    public bool IsInitialized => _isInitialized;

    public ScannerData? ScannerData => _scannerData;

    public ScanConfig ScanConfig => _scanConfig;

    /// <summary>
    /// Initialize the scanner
    /// </summary>
    public async Task<int> InitializeAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Initializing scanner...");
                
                var digitizerInfo = new _DIGITIZERINFO();
                var scannerData = new ScannerData();

                int status = Startup.InitializeDigitizer(ref digitizerInfo, ref scannerData);
                
                if (status == 0)
                {
                    _digitizerInfo = digitizerInfo;
                    _scannerData = scannerData;
                    _isInitialized = true;
                    _logger.LogInformation("Scanner initialized successfully: {Model}", scannerData.modelName);
                }
                else
                {
                    _logger.LogError("Scanner initialization failed with code: {Status}", status);
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during scanner initialization");
                return -1;
            }
        });
    }

    /// <summary>
    /// Perform a scan and return the image bytes
    /// </summary>
    public async Task<(int status, byte[]? imageBytes, string? errorMessage)> ScanAsync()
    {
        if (!_isInitialized || _digitizerInfo == null || _scannerData == null)
        {
            _logger.LogError("Scanner not initialized");
            return (-1, null, "Scanner not initialized");
        }

        return await Task.Run<(int, byte[]?, string?)>(() =>
        {
            try
            {
                _logger.LogInformation("Starting scan...");
                
                var digitizerInfo = _digitizerInfo.Value;
                var scannerData = _scannerData.Value;

                // Use a temporary file for the scan
                string tempDir = Path.Combine(Path.GetTempPath(), "bfd9010_scans");
                Directory.CreateDirectory(tempDir);
                
                var tempConfig = new ScanConfig
                {
                    OutputPath = tempDir,
                    OutputPrefix = $"scan_{DateTime.Now:yyyyMMdd_HHmmss}",
                    Offset0_BitDepth = _scanConfig.Offset0_BitDepth,
                    Offset2_DPI_X = _scanConfig.Offset2_DPI_X,
                    Offset4_Width = _scanConfig.Offset4_Width,
                    Offset8_Height = _scanConfig.Offset8_Height,
                    Offset12_Unknown = _scanConfig.Offset12_Unknown,
                    Offset16_Unknown = _scanConfig.Offset16_Unknown,
                    Offset20_Unknown = _scanConfig.Offset20_Unknown,
                    Offset24_BytesPerPixel = _scanConfig.Offset24_BytesPerPixel,
                    Offset28_Unknown = _scanConfig.Offset28_Unknown,
                    Offset30_Unknown = _scanConfig.Offset30_Unknown,
                    Offset32_Unknown = _scanConfig.Offset32_Unknown,
                    Offset36_Unknown = _scanConfig.Offset36_Unknown,
                    Offset40_OutputWidth = _scanConfig.Offset40_OutputWidth,
                    Offset44_OutputHeight = _scanConfig.Offset44_OutputHeight,
                    Offset48_Unknown = _scanConfig.Offset48_Unknown,
                    Offset56_DPI_Y = _scanConfig.Offset56_DPI_Y,
                    Offset60_Unknown = _scanConfig.Offset60_Unknown,
                    Offset64_Unknown = _scanConfig.Offset64_Unknown,
                    Offset68_Unknown = _scanConfig.Offset68_Unknown
                };

                int status = Scan.scan(digitizerInfo, scannerData, tempConfig);

                if (status == 0)
                {
                    // Find the generated file
                    string fileName = tempConfig.OutputPrefix + ".png";
                    string filePath = Path.Combine(tempDir, fileName);

                    if (File.Exists(filePath))
                    {
                        byte[] imageBytes = File.ReadAllBytes(filePath);
                        _logger.LogInformation("Scan completed successfully, image size: {Size} bytes", imageBytes.Length);
                        
                        // Clean up temp file
                        try { File.Delete(filePath); } catch { }
                        
                        return (0, imageBytes, null);
                    }
                    else
                    {
                        _logger.LogError("Scan completed but image file not found: {Path}", filePath);
                        return (-1, null, "Scan completed but image file not found");
                    }
                }
                else
                {
                    // Get detailed error information from the scanner
                    string errorMessage = GetVidarErrorMessage(status);
                    _logger.LogError("Scan failed with status: {Status} - {ErrorMessage}", status, errorMessage);
                    return (status, null, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during scan");
                return (-1, null, $"Exception during scan: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Get detailed error message from the scanner hardware
    /// </summary>
    private unsafe string GetVidarErrorMessage(int statusCode)
    {
        try
        {
            ushort num3 = 0;
            _VIDARERRORINFO errInfo = new _VIDARERRORINFO();
            Scanner.VscsiMethods.getVidarError(statusCode, ref errInfo, ref num3);

            // Return formatted error message with code and description
            return $"ERROR {errInfo.errorCode}: [{errInfo.errorMsg}]";
        }
        catch (Exception ex)
        {
            return $"Status code: {statusCode} (unable to retrieve error details: {ex.Message})";
        }
    }

    /// <summary>
    /// Calibrate the scanner
    /// </summary>
    public async Task<(int status, string? errorMessage)> CalibrateAsync()
    {
        if (!_isInitialized)
        {
            _logger.LogError("Scanner not initialized");
            return (-1, "Scanner not initialized");
        }

        return await Task.Run<(int, string?)>(() =>
        {
            try
            {
                _logger.LogInformation("Starting calibration...");
                int status = Calibrate.calibrate();
                
                if (status == 0)
                {
                    _logger.LogInformation("Calibration completed successfully");
                    return (0, null);
                }
                else
                {
                    string errorMessage = GetVidarErrorMessage(status);
                    _logger.LogError("Calibration failed with status: {Status} - {ErrorMessage}", status, errorMessage);
                    return (status, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during calibration");
                return (-1, $"Exception during calibration: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// Eject film from scanner
    /// </summary>
    public async Task<(int status, string? errorMessage)> EjectAsync()
    {
        if (!_isInitialized || _digitizerInfo == null)
        {
            _logger.LogError("Scanner not initialized");
            return (-1, "Scanner not initialized");
        }

        return await Task.Run<(int, string?)>(() =>
        {
            try
            {
                _logger.LogInformation("Ejecting film...");
                var digitizerInfo = _digitizerInfo.Value;
                int status = Eject.eject(digitizerInfo);
                
                if (status == 0)
                {
                    _logger.LogInformation("Film ejected successfully");
                    return (0, null);
                }
                else
                {
                    string errorMessage = GetVidarErrorMessage(status);
                    _logger.LogError("Film eject failed with status: {Status} - {ErrorMessage}", status, errorMessage);
                    return (status, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during eject");
                return (-1, $"Exception during eject: {ex.Message}");
            }
        });
    }
}

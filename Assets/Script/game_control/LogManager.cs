/*
 * ===========================================================================================
 * LogManager - 日誌管理器
 * ===========================================================================================
 * 
 * [功能說明]
 * 提供統一的日誌記錄功能，將訊息同時輸出至：
 * - Unity Console（透過 Debug.Log 系列方法）
 * - 本地日誌檔案（儲存於 Application.persistentDataPath）
 * 
 * [日誌檔案命名規則]
 * - 格式：{日期}-{序號}.log
 * - 範例：2024-01-15-1.log、2024-01-15-2.log
 * - 每天最多 10000 個檔案，超過則使用 overflow.log
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法]
 * -------------------------------------------------------------------------------------------
 * 
 * ● EnableLogging(bool enable)
 *   - 說明：啟用或停用日誌功能
 *   - 用法：LogManager.EnableLogging(false); // 停用日誌
 * 
 * ● Log(string message)
 *   - 說明：記錄一般訊息
 *   - 用法：LogManager.Log("遊戲開始");
 *   - 輸出：2024-01-15 14:30:00 - 遊戲開始
 * 
 * ● LogWarning(string message)
 *   - 說明：記錄警告訊息
 *   - 用法：LogManager.LogWarning("記憶體使用率過高");
 *   - 輸出：2024-01-15 14:30:00 - WARNING: 記憶體使用率過高
 * 
 * ● LogError(string message)
 *   - 說明：記錄錯誤訊息
 *   - 用法：LogManager.LogError("無法載入資源");
 *   - 輸出：2024-01-15 14:30:00 - ERROR: 無法載入資源
 * 
 * -------------------------------------------------------------------------------------------
 * [使用範例]
 * -------------------------------------------------------------------------------------------
 * 
 * // 記錄不同等級的訊息
 * LogManager.Log("玩家進入遊戲");
 * LogManager.LogWarning("網路延遲過高");
 * LogManager.LogError("存檔失敗");
 * 
 * // 在正式發布版本中停用日誌
 * #if !UNITY_EDITOR
 *     LogManager.EnableLogging(false);
 * #endif
 * 
 * ===========================================================================================
 */

using System;
using UnityEngine;


/// <summary>
/// 日誌管理器 - 提供統一的日誌記錄功能
/// </summary>
public static class LogManager
{
    /// <summary>是否啟用日誌功能</summary>
    private static bool isLoggingEnabled = true;
    
    /// <summary>日誌檔案路徑</summary>
    private static readonly string logPath = null;

    /// <summary>
    /// 靜態建構子 - 初始化日誌檔案路徑
    /// </summary>
    static LogManager()
    {
        // 尋找可用的日誌檔案名稱（避免覆蓋既有檔案）
        for (int index = 1; index <= 10000; index++)
        {
            string candidatePath = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "-" + index + ".log";
            if (!System.IO.File.Exists(candidatePath))
            {
                logPath = candidatePath;
                break;
            }
        }

        // 如果超過 10000 個檔案，使用 overflow.log
        logPath ??= Application.persistentDataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "-overflow.log";

        if (isLoggingEnabled)
        {
            Debug.Log($"[LogManager] Logging initialized. Log file path: {logPath}");
        }
        else
        {
            Debug.Log("[LogManager] Logging is disabled.。");
        }
    }

    /// <summary>
    /// 啟用或停用日誌功能
    /// </summary>
    /// <param name="enable">true 為啟用，false 為停用</param>
    public static void EnableLogging(bool enable)
    {
        isLoggingEnabled = enable;
    }

    /// <summary>
    /// 內部方法：將訊息寫入日誌檔案並輸出至 Console
    /// </summary>
    /// <param name="message">原始訊息內容</param>
    /// <param name="prefix">訊息前綴（如 "ERROR"、"WARNING"，一般訊息傳入 null）</param>
    /// <param name="debugAction">對應的 Debug 輸出方法（Debug.Log、Debug.LogWarning 等）</param>
    private static void WriteLog(string message, string prefix, Action<string> debugAction)
    {
        if (!isLoggingEnabled) return;

        // 格式化前綴
        string formattedPrefix = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}: ";
        
        // 組合完整的日誌訊息
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {formattedPrefix}{message}\n";
        
        // 寫入檔案
        System.IO.File.AppendAllText(logPath, logMessage);
        
        // 輸出至 Unity Console
        debugAction?.Invoke(logMessage);
    }

    /// <summary>
    /// 記錄一般訊息
    /// </summary>
    /// <param name="message">訊息內容</param>
    public static void Log(string message)
    {
        WriteLog(message, null, Debug.Log);
    }

    /// <summary>
    /// 記錄錯誤訊息
    /// </summary>
    /// <param name="message">錯誤訊息內容</param>
    public static void LogError(string message)
    {
        WriteLog(message, "ERROR", Debug.LogError);
    }

    /// <summary>
    /// 記錄警告訊息
    /// </summary>
    /// <param name="message">警告訊息內容</param>
    public static void LogWarning(string message)
    {
        WriteLog(message, "WARNING", Debug.LogWarning);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CSV解析器 - 接受配置对象，避免使用全局变量
/// </summary>
public static class CSVParser
{
    /// <summary>
    /// 解析CSV文本为对话行字典
    /// </summary>
    /// <param name="csvText">CSV文本内容</param>
    /// <param name="config">对话系统配置</param>
    /// <returns>解析后的对话行字典</returns>
    public static Dictionary<int, DialogueLine> ParseDialogueCSV(string csvText, DialogueConfig config)
    {
        var dialogueLines = new Dictionary<int, DialogueLine>();

        if (string.IsNullOrEmpty(csvText))
        {
            Debug.LogError("CSV文本为空或null");
            return dialogueLines;
        }

        if (config == null)
        {
            Debug.LogError("DialogueConfig配置对象为null");
            return dialogueLines;
        }

        // 按行分割，处理不同的换行符
        string[] rows = csvText.Split(new char[] { config.lineDelimiter, '\r' },
            StringSplitOptions.RemoveEmptyEntries);

        // 跳过表头行
        for (int i = config.headerRowIndex + 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            try
            {
                DialogueLine dialogueLine = ParseDialogueRow(row, i + 1, config);
                if (dialogueLine != null)
                {
                    dialogueLines[dialogueLine.index] = dialogueLine;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"解析CSV第{i + 1}行时出错: {ex.Message}\n行内容: {row}");
            }
        }

        Debug.Log($"成功解析CSV文件，共{dialogueLines.Count}行对话数据");
        return dialogueLines;
    }

    /// <summary>
    /// 解析单行CSV数据为对话行
    /// </summary>
    private static DialogueLine ParseDialogueRow(string row, int lineNumber, DialogueConfig config)
    {
        string[] cells = row.Split(config.csvDelimiter);

        // 检查列数
        if (cells.Length < config.minRequiredColumns)
        {
            Debug.LogWarning($"第{lineNumber}行列数不足，至少需要{config.minRequiredColumns}列，实际{cells.Length}列");
            return null;
        }

        // 清理每个单元格的数据
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = cells[i].Trim();
        }

        // 检查是否为有效的对话行
        if (!int.TryParse(cells[config.indexColumn], out int index))
        {
            Debug.LogWarning($"第{lineNumber}行编号列无法解析为数字: {cells[config.indexColumn]}");
            return null;
        }

        // 创建对话行对象
        string tachie = cells.Length > config.tachieColumn ? cells[config.tachieColumn] : config.defaultTachieName;

        return new DialogueLine(
            cells[config.indexColumn],
            cells[config.symbolColumn],
            cells[config.nameColumn],
            cells[config.contentColumn],
            cells[config.jumpColumn],
            tachie,
            config
        );
    }

    /// <summary>
    /// 验证CSV格式是否正确
    /// </summary>
    public static string ValidateCSVFormat(string csvText, DialogueConfig config)
    {
        if (string.IsNullOrEmpty(csvText))
            return "CSV文件为空";

        if (config == null)
            return "DialogueConfig配置对象为null";

        string[] rows = csvText.Split(new char[] { config.lineDelimiter, '\r' },
            StringSplitOptions.RemoveEmptyEntries);

        if (rows.Length < 2)
            return "CSV文件至少需要包含表头和一行数据";

        // 检查表头
        string[] headers = rows[config.headerRowIndex].Split(config.csvDelimiter);
        if (headers.Length < config.minRequiredColumns)
            return $"表头列数不足，至少需要{config.minRequiredColumns}列";

        return "CSV格式验证通过";
    }
}

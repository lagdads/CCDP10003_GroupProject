using UnityEngine;

/// <summary>
/// 对话系统配置类 - 替代全局变量，使用ScriptableObject进行配置管理
/// </summary>
[CreateAssetMenu(fileName = "DialogueConfig", menuName = "Dialogue System/Dialogue Config")]
public class DialogueConfig : ScriptableObject
{
    [Header("CSV列索引配置")]
    public int indexColumn = 0;
    public int symbolColumn = 1;
    public int nameColumn = 2;
    public int contentColumn = 3;
    public int jumpColumn = 4;
    public int tachieColumn = 5;
    public int finalColumn = 6;

    [Header("CSV解析配置")]
    public char csvDelimiter = ',';
    public char lineDelimiter = '\n';
    public int headerRowIndex = 0;
    public int minRequiredColumns = 5;

    [Header("对话标志配置")]
    public string characterSymbol = "W";
    public string narrationSymbol = "T";
    public string optionSymbol = "O";
    public string endSymbol = "END";

    [Header("默认值配置")]
    public string defaultTachieName = "default";
    public string emptyField = "";

    [Header("打字机效果配置")]
    [Range(0f, 1f)]
    public float typeWriterSpeed = 0.025f;

    [Header("自动播放配置")]
    public float autoPlayDelayAfterAudio = 1f;
    public float autoPlayTextSpeedMultiplier = 6f;
}

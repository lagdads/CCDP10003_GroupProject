using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    /*变量区*/

    //存放csv文件
    public TextAsset dataFile;

    //对话系统配置对象 - 替代全局变量
    [Header("对话系统配置")]
    public DialogueConfig dialogueConfig;

    //代表人物立绘的UI图像组件
    public Image portrait;

    //代表人物头像的UI图像组件
    public Image avatar;

    //代表人物名称的TMP文本组件
    public TMP_Text characterName;

    //代表对话内容的TMP文本组件
    public TMP_Text content;

    [Header("打字机效果设置")] [Range(0, 1)] public float intervalTime = 0.025f;

    [Header("出场人物列表")]
    //存放人物列表
    public List<Character> characters;
    [HideInInspector] public int dialogueIndex;

    [Header("分支选项父组件及按钮预制件")]
    //分支选项
    public Transform parentGroup;
    public GameObject optionPref;

    //连接一个按钮管理器（对话历史的更新需要其方法）
    [HideInInspector] public ButtonManager buttonManager;

    //一个管理器对应一个DifferentSymbol
    private DifferentSymbols df;

    //对话行列表（字典版）索引<-->对话行类
    public Dictionary<int, DialogueLine> dialogueLines = new();

    //维护观察者列表
    private ObserverInterface[] observers;
    public Coroutine typeTextCoroutine;

    /*生命周期区*/
    private void Awake()
    {
        // 验证配置对象
        if (dialogueConfig == null)
        {
            Debug.LogError("DialogueManager: dialogueConfig 未设置! 请在Inspector中分配DialogueConfig对象");
            return;
        }

        // 如果Inspector中的intervalTime被设置了，使用它覆盖配置中的值
        if (intervalTime != 0.025f) // 如果不是默认值
        {
            dialogueConfig.typeWriterSpeed = intervalTime;
        }
        else
        {
            intervalTime = dialogueConfig.typeWriterSpeed;
        }

        // 使用新的CSV解析器来处理文件
        InitializeDialogueData();

        //初始化differentSymbols，传入配置对象
        df = new DifferentSymbols(this, dialogueConfig);

        //初始化（注册）观察者
        observers = GetComponents<ObserverInterface>();

        //初始化按钮管理器
        buttonManager = GetComponent<ButtonManager>();

        //寻找所有角色立绘
        foreach (Character character in characters)
        {
            character.FindTachie();
        }

    }

    /// <summary>
    /// 初始化对话数据
    /// </summary>
    private void InitializeDialogueData()
    {
        if (dataFile == null)
        {
            Debug.LogError("DialogueManager: dataFile 未设置!");
            return;
        }

        // 验证CSV格式
        string validationResult = CSVParser.ValidateCSVFormat(dataFile.text, dialogueConfig);
        if (!validationResult.Contains("验证通过"))
        {
            Debug.LogError($"CSV格式验证失败: {validationResult}");
            return;
        }

        // 解析CSV数据
        dialogueLines = CSVParser.ParseDialogueCSV(dataFile.text, dialogueConfig);

        if (dialogueLines.Count == 0)
        {
            Debug.LogError("未能解析到任何有效的对话数据!");
            return;
        }

        dialogueIndex = 0;
        Debug.Log($"对话数据初始化完成，共加载 {dialogueLines.Count} 条对话");
    }

    private void Start()
    {
        Advance();
    }

    private void Update()
    {

    }

    /*方法区*/
    //注意：每行索引对应意义
    //编号--0  标志--1  人物名称--2  文本--3  跳转编号--4  结束--5
    //根据索引推进对话的内容
    public void Advance()
    {
        if (!dialogueLines.ContainsKey(dialogueIndex))
        {
            Debug.LogError($"对话索引 {dialogueIndex} 不存在!");
            return;
        }

        DialogueLine line = dialogueLines[dialogueIndex];

        //通知观察者
        notify();

        df.DialogueLineAnalysis(line);
    }

    /*通知观察者方法*/
    //主题
    private void notify()
    {
        //如果是列表，需要遍历
        foreach (ObserverInterface observer in observers)
        {
            observer.executeUpdate();
        }
    }

    //
    //发现（待尝试）：这样一来，似乎角色立绘的变化更适合使用观察者模式
    //

    //通过名字字符串寻找列表中对应人物对象
    public Character FindCha(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        foreach (Character character in characters)
        {
            if (character.name == name)
            {
                return character;
            }
        }

        Debug.LogWarning($"未找到名为 '{name}' 的角色");
        return null;
    }

    /// <summary>
    /// 根据立绘名称查找对应的立绘精灵
    /// </summary>
    /// <param name="character">角色对象</param>
    /// <param name="tachieName">立绘名称</param>
    /// <returns>立绘精灵，未找到返回null</returns>
    public Sprite FindTachieSprite(Character character, string tachieName)
    {
        if (character == null || string.IsNullOrEmpty(tachieName)) return null;

        foreach (var tachieDialogue in character.TachieList)
        {
            if (tachieDialogue.name == tachieName)
            {
                return tachieDialogue.tachie;
            }
        }

        // 如果找不到指定立绘，尝试使用默认立绘
        string defaultTachieName = dialogueConfig?.defaultTachieName ?? "default";
        if (tachieName != defaultTachieName && character.TachieList.Count > 0)
        {
            Debug.LogWarning($"角色 '{character.name}' 未找到立绘 '{tachieName}'，使用默认立绘");
            return character.TachieList[0].tachie;
        }

        return null;
    }

    public void BoxClick()
    {
        Advance();
    }

    //执行打字机效果
    public void ExecuteTypeText()
    {
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
            typeTextCoroutine = null;
        }
        typeTextCoroutine = StartCoroutine(TypeText());
    }

    //打字机效果
    private IEnumerator TypeText()
    {
        //刷新网格
        content.ForceMeshUpdate();
        int total = content.textInfo.characterCount;
        int current = 0;
        float currentIntervalTime = dialogueConfig != null ? dialogueConfig.typeWriterSpeed : intervalTime;

        while (current <= total)
        {
            content.maxVisibleCharacters = current;
            current++;
            yield return new WaitForSeconds(currentIntervalTime);
        }
        //置空协程
        typeTextCoroutine = null;
    }
}
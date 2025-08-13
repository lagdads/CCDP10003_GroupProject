//策略接口

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal interface DialogueUpdate
{
    public void LineUpdate(DialogueLine line, DialogueManager manager, DialogueConfig config);
}

//上下文类
public class DifferentSymbols
{
    //字典类，symbol对应一个DifferentSymbol脚本中的DialogueUpdate类
    private readonly Dictionary<string, DialogueUpdate> differentSymbolAnalysis;
    private readonly DialogueManager manager;
    private readonly DialogueConfig config;

    /*构造函数*/
    //一个DifferentSymbols绑定一个manager类和配置对象
    public DifferentSymbols(DialogueManager _manager, DialogueConfig _config)
    {
        //让我们的manager指向一个DialogueManager类
        manager = _manager;
        config = _config;

        // 使用配置对象初始化策略字典
        differentSymbolAnalysis = new Dictionary<string, DialogueUpdate>
        {
            { config.characterSymbol, new UpdateW() },
            { config.narrationSymbol, new UpdateT() },
            { config.optionSymbol, new UpdateO() },
            { config.endSymbol, new UpdateEND() }
        };
    }

    //C#声明接口时，get和set表示外部可读可写该属性
    private DialogueUpdate DialogueUpdate { get; set; }

    //用于执行的方法
    public void DialogueLineAnalysis(DialogueLine line)
    {
        if (differentSymbolAnalysis.ContainsKey(line.symbol))
        {
            DialogueUpdate = differentSymbolAnalysis[line.symbol];
            DialogueUpdate.LineUpdate(line, manager, config);
        }
        else
        {
            Debug.LogError($"未知的对话标志: {line.symbol}");
        }
    }
}

/*具体策略*/
public class UpdateW : DialogueUpdate
{
    public void LineUpdate(DialogueLine line, DialogueManager manager, DialogueConfig config)
    {
        Character character = manager.FindCha(line.name);
        UpdateCharacterDialogue(character, line, manager, config);
        manager.dialogueIndex = line.jump;

        //更新对话历史（对话历史的可操作区域与按钮管理器绑定）
        manager.buttonManager.historyUpdate(line);

        //将打字机效果附加在TMP_Text上面
        manager.ExecuteTypeText();
    }

    private void UpdateCharacterDialogue(Character character, DialogueLine line, DialogueManager manager, DialogueConfig config)
    {
        if (character == null)
        {
            Debug.LogWarning($"未找到角色: {line.name}");
            manager.avatar.enabled = false;
            if (manager.portrait != null) manager.portrait.enabled = false;
            manager.characterName.text = $"【{line.name}】";
            manager.content.text = "  " + line.content;
            return;
        }

        // 更新头像
        if (!character.havePortrait)
        {
            manager.avatar.enabled = false;
        }
        else
        {
            manager.avatar.enabled = true;
        }

        // 更新立绘
        UpdatePortrait(character, line.tachieName, manager, config);

        // 更新名称和内容
        manager.characterName.text = "【" + character.name + "】";
        manager.content.text = "  " + line.content;
    }

    private void UpdatePortrait(Character character, string tachieName, DialogueManager manager, DialogueConfig config)
    {
        if (manager.portrait == null) return;

        Sprite tachieSprite = manager.FindTachieSprite(character, tachieName);
        if (tachieSprite != null)
        {
            manager.portrait.sprite = tachieSprite;
            manager.portrait.enabled = true;
            Debug.Log($"更新立绘: {character.name} - {tachieName}");
        }
        else
        {
            manager.portrait.enabled = false;
            Debug.LogWarning($"未找到角色 {character.name} 的立绘: {tachieName}");
        }
    }
}

public class UpdateT : DialogueUpdate
{
    public void LineUpdate(DialogueLine line, DialogueManager manager, DialogueConfig config)
    {
        UpdateNarration(line.content, manager, config);
        manager.dialogueIndex = line.jump;

        //更新对话历史
        manager.buttonManager.historyUpdate(line);

        //将打字机效果附加在TMP_Text上面
        manager.ExecuteTypeText();
    }

    private void UpdateNarration(string newContent, DialogueManager manager, DialogueConfig config)
    {
        manager.avatar.enabled = false;
        if (manager.portrait != null)
        {
            manager.portrait.enabled = false; // 旁白时隐藏立绘
        }
        manager.characterName.text = config.emptyField;
        manager.content.text = "  " + newContent;
    }
}

public class UpdateO : DialogueUpdate
{
    public void LineUpdate(DialogueLine line, DialogueManager manager, DialogueConfig config)
    {
        manager.parentGroup.gameObject.SetActive(true);
        GenerateOptions(manager.dialogueIndex, manager, config);
    }

    private void GenerateOptions(int index, DialogueManager manager, DialogueConfig config)
    {
        //如果处于自动播放，停下
        if (manager.buttonManager.isAuto)
        {
            manager.buttonManager.autoEnd();
        }

        if (!manager.dialogueLines.ContainsKey(index))
        {
            Debug.LogError($"选项索引 {index} 不存在!");
            return;
        }

        DialogueLine line = manager.dialogueLines[index];
        if (line.symbol != config.optionSymbol)
        {
            return;
        }

        //如果类不继承于MonoBehavior，则使用UnityEngine.Object.Instantiate
        GameObject option = Object.Instantiate(manager.optionPref, manager.parentGroup);
        option.GetComponentInChildren<TMP_Text>().text = line.content;
        option.GetComponent<Button>().onClick.AddListener(
            delegate
            {
                //更新对话历史
                manager.buttonManager.historyUpdate(line);

                //如果还处于自动播放，继续
                if (manager.buttonManager.isAuto)
                {
                    manager.buttonManager.autoStart();
                }

                OptionJump(line.jump, manager);
            }
        );
        GenerateOptions(index + 1, manager, config);
    }

    private void OptionJump(int target, DialogueManager manager)
    {
        manager.parentGroup.gameObject.SetActive(false);
        manager.dialogueIndex = target;
        manager.Advance();
    }
}

public class UpdateEND : DialogueUpdate
{
    public void LineUpdate(DialogueLine line, DialogueManager manager, DialogueConfig config)
    {
        Debug.Log("对话结束");
        Application.Quit();
    }
}
// csv文件每一行的信息
// 编号--0  标志--1  角色名--2  文本--3  跳转索引--4  立绘--5  备用--6

public class DialogueLine
{
    //文本
    public string content = "";
    //索引
    public int index;
    //跳转索引
    public int jump;
    //角色名
    public string name = "";
    //标志
    public string symbol = "";
    //立绘名称
    public string tachieName = "";

    /*构造函数*/
    //用于初始化数据
    public DialogueLine(string _index, string _symbol, string _name, string _content, string _jump, string _tachie = "", DialogueConfig config = null)
    {
        // 使用配置对象或默认值
        string defaultTachie = config?.defaultTachieName ?? "default";
        string emptyField = config?.emptyField ?? "";
        string endSymbol = config?.endSymbol ?? "END";

        // 解析基本字段
        if (int.TryParse(_index, out int parsedIndex))
        {
            index = parsedIndex;
        }

        symbol = _symbol.Trim().ToUpperInvariant(); // 移除可能的\r和空格，统一大写
        name = _name?.Trim() ?? emptyField;
        content = _content?.Trim() ?? emptyField;
        tachieName = _tachie?.Trim() ?? defaultTachie;

        // 特殊处理END标志
        if (symbol == endSymbol)
        {
            // END时jump字段为空,不进行解析
            return;
        }

        // 解析跳转索引
        if (int.TryParse(_jump, out int parsedJump))
        {
            jump = parsedJump;
        }
    }
}
using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ConsoleJsonViewer : EditorWindow
{
    private const string UXML_GUID = "8e772ffe6d7e36e4da273906ac1b149e";
    private const string USS_GUID = "f2d35262c1d2e664db1849a1fd770f04";
    
    private bool isLinkConsoleWindow = false;
    private bool isConvertJson = false;
    private bool isAutoCheck = false;

    private TextField textField;
    private object consoleWindow;

    [SerializeField] private string originMessage;
    
    [MenuItem("CubicSystem/Editor/Util/ConsoleJsonViewer")]
    public static void ShowExample()
    {
        ConsoleJsonViewer wnd = GetWindow<ConsoleJsonViewer>();
        wnd.titleContent = new GUIContent("ConsoleJsonViewer");
    }

    public void CreateGUI()
    {
        string uxmlPath = AssetDatabase.GUIDToAssetPath(UXML_GUID);
        string ussPath = AssetDatabase.GUIDToAssetPath(USS_GUID);
        
        // Import & Setup UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        visualTree.CloneTree(rootVisualElement);

        // Import & Setup USS
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        rootVisualElement.styleSheets.Add(styleSheet);

        consoleWindow = GetConsoleWindow();
        
        InitializeControl();
    }

    private void InitializeControl()
    {
        var linkConsole = this.rootVisualElement.Q<ToolbarToggle>("LinkConsole");
        var convertJson = this.rootVisualElement.Q<ToolbarToggle>("ConvertJson");
        var autoCheck = this.rootVisualElement.Q<ToolbarToggle>("AutoCheck");

        isLinkConsoleWindow = linkConsole.value;
        isConvertJson = convertJson.value;
        isAutoCheck = autoCheck.value;

        linkConsole.RegisterValueChangedCallback(SetupLinkConsole);
        convertJson.RegisterValueChangedCallback(SetupConvertToJson);
        autoCheck.RegisterValueChangedCallback(SetupAutoCheck);

        textField = rootVisualElement.Q<TextField>();
        
        this.rootVisualElement.schedule
            .Execute(UpdateConsoleEvent)
            .Every(100);
    }

    private void SetupLinkConsole(ChangeEvent<bool> evt)
    {
        if (evt.newValue != evt.previousValue)
        {
            isLinkConsoleWindow = evt.newValue;
        }
    }
    
    private void SetupConvertToJson(ChangeEvent<bool> evt)
    {
        if (evt.newValue != evt.previousValue)
        {
            isConvertJson = evt.newValue;
            textField.value = FormatJson(originMessage);
        }
    }
    
    private void SetupAutoCheck(ChangeEvent<bool> evt)
    {
        if (evt.newValue != evt.previousValue)
        {
            isAutoCheck = evt.newValue;

            if (originMessage != "")
            {
                IsValidJson(originMessage);    
            }
        }
    }
    

    private void UpdateConsoleEvent()
    {
        if (!isLinkConsoleWindow)
        {
            return;
        }
        
        if (IsUnityNull(consoleWindow))
        {
            consoleWindow = GetConsoleWindow();
        }
        else
        {
            UpdateTextField(GetLogMessage());
        }
    }

    private bool IsUnityNull(object target)
    {
        if (target == null)
        {
            return true;
        }
        
        if (target.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
        {
            UnityEngine.Object unityObj = target as UnityEngine.Object;
            return unityObj == null || unityObj.Equals(null);
        }

        return false;
    }
    

    private void UpdateTextField(string newMessage)
    {
        if (newMessage == null || originMessage == newMessage)
        {
            return;
        }

        originMessage = newMessage;
        textField.value = FormatJson(newMessage);
    }
    
    private bool IsValidJson(string message)
    {
        bool res = true;
        try
        {
            // JsonUtility.FromJsonOverwrite 메서드를 이용하여 유효한 JSON인지 테스트합니다.
            JsonUtility.FromJsonOverwrite(message, new object());
        }
        catch (ArgumentException)
        {
            // JSON 파싱 중 에러가 발생하면 유효하지 않은 것으로 간주합니다.
            res = false;
        }

        var jsonState = this.rootVisualElement.Q<Label>("StateText");
        jsonState.text = "";
        
        if (isAutoCheck)
        {
            jsonState.text = res ? "Is Json!" : "Not Json";
            jsonState.style.color = res ? Color.green : Color.red;
        }

        return res;
    }
    
    private string FormatJson(string json)
    {
        if (!IsValidJson(json) || !isConvertJson)
        {
            return json;
        }
        
        int indentation = 0;
        int quoteCount = 0;
        var result = new System.Text.StringBuilder();

        for (int i = 0; i < json.Length; i++)
        {
            char ch = json[i];

            if (ch == '\"') quoteCount++;

            if (quoteCount % 2 == 0)
            {
                if (ch == '{' || ch == '[')
                {
                    indentation++;
                    result.Append(ch);
                    result.Append('\n');
                    result.Append(' ', indentation * 2);
                    continue;
                }

                if (ch == '}' || ch == ']')
                {
                    indentation--;
                    result.Append('\n');
                    result.Append(' ', indentation * 2);
                    result.Append(ch);
                    continue;
                }

                if (ch == ',')
                {
                    result.Append(ch);
                    result.Append('\n');
                    result.Append(' ', indentation * 2);
                    continue;
                }
            }

            result.Append(ch);
        }

        return result.ToString();
    }
    
    private string GetLogMessage()
    {
        var textField = consoleWindow.GetType().GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
        string message = (string)textField.GetValue(consoleWindow);

        if (message == "")
        {
            return null;
        }
        
        int removeIndex = message.IndexOf("UnityEngine.Debug:Log (object)");
        if (removeIndex >= 0)
        {
            message = message.Substring(0, removeIndex);
        }
        
        return message;
    }
    
    // ConsoleWindow 객체 가져오기
    private object GetConsoleWindow()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(Editor));
        System.Type consoleWindowType = assembly.GetType("UnityEditor.ConsoleWindow");
        FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
        return fieldInfo.GetValue(null);
    }
}
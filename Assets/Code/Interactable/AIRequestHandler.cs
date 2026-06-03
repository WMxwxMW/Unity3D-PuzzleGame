using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text; 

[System.Serializable]
public class OllamaResponse
{
    public Message message;
    public bool done;

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}

public class AIRequestHandler : MonoBehaviour
{
    public static AIRequestHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator GetAIResponse(string prompt, System.Action<string> callback)
    {
        // 1. 手动构建 JSON 字符串
        string jsonPayload = "{" +
            $"\"model\": \"qwen:7b-chat-q4_0\"," +
            $"\"messages\": [" +
                $"{{\"role\": \"system\", \"content\": \"十年前发生一起失踪案件，有一个女人和两个孩子失踪了。有尖树的地方是X地，X地还有一个特色小吃。回答需简洁，不超过50字。\"}}," +
                $"{{\"role\": \"user\", \"content\": \"{prompt}\"}}" +
            $"]," +
            $"\"stream\": false" +
            "}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // 2. 创建请求
        using (UnityWebRequest req = new UnityWebRequest("http://localhost:11434/api/chat", "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            // 3. 显式指定方法（防止默认变为 PUT）
            req.method = UnityWebRequest.kHttpVerbPOST;

            Debug.Log("发送请求 payload: " + jsonPayload); // 调试：打印发送的内容

            // 4. 发送请求
            yield return req.SendWebRequest();

            // 5. 处理结果
            if (req.result == UnityWebRequest.Result.Success)
            {
                string responseText = req.downloadHandler.text;
                Debug.Log("收到响应: " + responseText); // 调试：打印收到的内容

                // 检查是否为空
                if (string.IsNullOrEmpty(responseText))
                {
                    callback?.Invoke("AI 返回了空响应");
                    yield break;
                }

                try
                {
                    var response = JsonUtility.FromJson<OllamaResponse>(responseText);
                    string aiResponse = response?.message?.content;
                    callback?.Invoke(string.IsNullOrEmpty(aiResponse) ? "AI 返回了空内容" : aiResponse);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("JSON 解析失败: " + e.Message);
                    callback?.Invoke("解析 AI 回复失败");
                }
            }
            else
            {
                // 打印详细错误
                Debug.LogError($"API Error: {req.error}\nCode: {req.responseCode}\nBody: {req.downloadHandler.text}");
                callback?.Invoke($"连接 AI 失败: {req.error}");
            }
        }
    }
}
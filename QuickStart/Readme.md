# Quick Start

I have used the pakcage "CommandLine" for command line parsing. You can try this code by this:

```bash
dotnet run - -m "gpt-4.1" -k "{your Key}" -d "{your endpoint url}" -l "None"
```

and the parameters are:

```powershell
  Required option 'm, modelId' is missing.
  Required option 'k, apiKey' is missing.
  Required option 'd, endpoint' is missing.

  -m, --modelId     Required. The AI Model Id, e.g., gpt-3.5-turbo, gpt-4, etc.

  -k, --apiKey      Required. The OpenAI API Key.

  -d, --endpoint    Required. The OpenAI API Endpoint. you can use other endpoints, such as Azure OpenAI Service.

  -l, --logLevel    (Default: Trace) The log level, e.g., Trace, Debug, Information, Warning, Error, Critical.

  --help            Display this help screen.

  --version         Display version information.

```

Running Result:

```cmd
User > 你好。
Assistant > 你好！有什么可以帮您的吗？
User > 你在哪里？
Assistant > 我在中国上海。有什么可以为您提供帮助的吗？
User > 你是谁？
Assistant > 我是一个由人工智能驱动的助手，可以帮助你解答问题、提供信息、完成任务或者聊天互动。如果你有任何需要，随时告诉我！你还想了解些什么呢？
User > 你那边天气好吗？
Assistant > 现在上海的天气很好，晴天，气温大约25°C。你有什么出行或户外计划吗？
User > 没有呢。
Assistant > 没关系，有时候在家休息或者静静享受好天气也是一种幸福。如果你需要推荐室内活动、影音娱乐，或者想聊聊别的话题，随时告诉我！
User > 你能做些什么？
Assistant > 我可以帮助你做很多事情，比如：

1. 回答各种知识性问题，如历史、科技、文化等。
2. 提供实时信息，比如天气、新闻等。
3. 帮你管理智能家居设备（如果有接入的话），如开关灯、查询状态等。
4. 帮你制定计划或提醒事项。
5. 推荐电影、音乐、书籍等娱乐内容。
6. 辅导学习，比如数学、英语、写作等。
7. 和你聊天、倾听你的想法，进行轻松互动。

你现在有具体想要体验或者需要帮助的事情吗？
User > 哦。你能管理智能家居么？
Assistant > 是的，我可以帮助你管理智能家居，比如：

- 控制灯光的开关
- 查询家里灯光的当前状态
- 了解家居环境的一些实时信息（如果有相应设备接入）

如果你的其他智能家居设备（如空调、窗帘、安防等）已与系统集成，我也可以协助控制。

你想试试管理哪部分智能家居呢？比如要控制灯光吗？
User > 哦。控制灯光么？
Assistant > 对，我可以帮你控制灯光，比如开灯、关灯，或者查询当前灯光的状态。
你想现在试一试吗？比如了解家里的灯现在是什么状态，或者让某个房间的灯亮起来/关掉？如果有具体的需求，可以告诉我！
User > 行啊。家里有几盏灯啊？
Assistant > 你家里目前有三盏灯：

1. 台灯（Table Lamp）——现在是关着的
2. 门廊灯（Porch Light）——现在是关着的
3. 吊灯（Chandelier）——现在是开着的

你想操作哪一盏灯？
User > 全部开起来
Assistant > 三盏灯已经全部打开：

1. 台灯（Table Lamp）——已打开
2. 门廊灯（Porch Light）——已打开
3. 吊灯（Chandelier）——已打开

还有其他需要帮忙的吗？
User > 将门廊等关掉
Assistant > 门廊灯（Porch Light）已经关闭。

台灯和吊灯仍然是亮着的。如果还需要调整其他灯光，请告诉我！
User > 好的嘛。
Assistant > 好的，已经帮你完成啦！如果还有其他需要，随时可以找我。我随时都在，有什么想问、想聊或者想尝试的都可以告诉我哦！
User >


```
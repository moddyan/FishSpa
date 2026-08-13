# Unity 原型经验记录 - 2026-06-12

背景：Fish Spa 3D 灰盒原型第一次实现，主要涉及 `FishSpaClient` 中 `Assets/FishSpa/Prototype` 下的脚本。

## 避免重复踩这些坑

### Unity batchmode 会被已打开的 Editor 阻塞

现象：运行 Unity `-batchmode -executeMethod ...` 时立刻中止，日志提示：

`It looks like another Unity instance is running with this project open.`

原因：同一个 Unity 项目已经在 Editor 中打开，batchmode 无法获取项目锁。

预防：

- 如果 Unity 已经打开，优先使用当前 Editor 里的菜单项执行生成逻辑。
- 只有确认没有 Unity 进程打开同一项目时，再用 batchmode 做生成或验证。
- batchmode 方法里不要无条件弹窗；需要用 `if (!Application.isBatchMode)` 包住 `EditorUtility.DisplayDialog`，避免无界面流程卡住。

### 安装了 Unity MCP 包，不等于当前 Codex 会话能调用 MCP 工具

现象：Unity 项目里已经安装 Unity MCP 相关 package，但当前 Codex 会话没有暴露可直接调用的 Unity MCP 工具。

原因：Unity 项目内 package 安装状态，和当前 assistant 运行时可用工具，是两回事。

预防：

- 不要因为 `manifest.json` 里有 Unity MCP 包，就假设 Codex 能直接调用 Unity MCP。
- 先检查当前会话暴露了哪些工具；如果没有 Unity MCP 工具，就回退到脚本、Editor 菜单项、batchmode 等方式。

### 灰盒场景优先用 Editor 生成器

观察：代码生成场景比手工搭场景更适合 AI 辅助迭代。

决策：

- 原型场景构建逻辑放在 Editor 脚本中。
- 生成内容放在 `Assets/FishSpa/Prototype` 下。
- Fish Spa 原型工作不要修改 `Assets/ThirdParty` 或 More Mountains demo 内容。

### Unity API 兼容细节要保守

这一轮捕获到的问题：

- `Scene.name` 是只读的；场景名应由 `EditorSceneManager.SaveScene(scene, path)` 的保存路径决定。
- Unity Editor 脚本里不要使用 `text[start..]` 这类 C# range 语法；为了 Mono/编译器兼容性，优先用 `Substring`。
- Unity 6.4 中，如果不需要对象顺序，优先用 `FindAnyObjectByType<T>()`，避免使用已废弃的 `FindFirstObjectByType<T>()`。
- 查找对象时优先使用当前重载，例如 `FindObjectsByType<T>(FindObjectsInactive.Exclude)`，避免使用已废弃的 sort-mode 旧重载。

### 离线编译 Unity 脚本需要补齐 Unity/Mono 引用

现象：直接调用 standalone `csc` 编译 Unity 脚本时，最开始出现缺少 `System`、`netstandard`、`mscorlib`、`FontStyle`、`TextAnchor` 等错误。

原因：离开 Unity 编译管线后，需要显式引用 Unity 的 Mono facades 和 UnityEngine 模块。

本项目里有用的引用包括：

- Unity Mono：`Editor/Data/MonoBleedingEdge/lib/mono/4.8-api/mscorlib.dll`
- `System.dll`、`System.Core.dll`、`Facades/netstandard.dll`
- Unity 模块：`UnityEngine.CoreModule.dll`、`UnityEngine.PhysicsModule.dll`、`UnityEngine.IMGUIModule.dll`、`UnityEngine.TextRenderingModule.dll`
- package 程序集，例如 `Library/ScriptAssemblies/Unity.InputSystem.dll`

这种离线编译能抓 C# 层面的错误，但不能替代真正的 Unity 导入和 Play Mode 验证。

### URP 里谨慎使用大面积透明调试几何体

观察：原计划加一个巨大的透明清理半径球，但 URP 透明材质设置不完整时，很容易显示成醒目的实心球。

决策：

- 第一版先跳过大型透明辅助 mesh。
- 优先用 HUD 提示和目标自身反馈表达“是否可清理”；后续需要时再加调试可视化。

### 在 Editor 外创建 Unity 资源时要补 `.meta`

风险：如果在 Unity Editor 外创建脚本/文件夹，Unity 后续会自动生成 `.meta`，但在导入前 GUID 不受控。

预防：

- 如果无法立刻让 Unity 导入资源，手动新增脚本/文件夹时同步添加 `.meta`。
- 只有在明确需要时，才让 Unity 重新生成 `.meta`。

## 工作规则

实现过程中如果暴露了可复用的工作流坑，要在上下文还新鲜时记录。每条记录尽量短，包含：背景、现象、原因、预防方式，以及关键路径或命令。


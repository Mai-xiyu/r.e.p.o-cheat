# r.e.p.o-cheat

一个开源的 C# Mono 作弊工具，面向 Unity 游戏 [R.E.P.O.](https://store.steampowered.com/app/2594580/REPO/)，通过 SharpMonoInjector 运行时注入，使用 Harmony 打补丁。

> 本项目开源且免费，仅供个人学习与研究使用。请在私人房间中使用，尊重其他玩家。作者不对使用方式负责。

- [English Documentation](README.md)

## 兼容性

本项目基于以下已安装游戏基线适配：

| 组件 | 版本 |
|---|---|
| 游戏 | v0.4.4.3 |
| Assembly-CSharp.dll SHA-256 | `CE995A182DDC884EA965E87786F1986248D9616300FA825BCC04BCA671EE6526` |
| Unity | 2022.3.67f2（Mono） |
| Photon | PUN 2.52 |
| TextMeshPro | 1.4.0 |

兼容性以程序集指纹 + 能力检测判定，不做版本号硬比较。游戏更新后请按 [.api-compatibility.md](.api-compatibility.md) 中的流程重新验证所用 API 面。

## 功能

- **自身**：无敌、无限生命/体力、穿墙飞行、速度/力量/跳跃/重力/抓取距离自定义、无后坐力/无冷却、隔墙抓取、自定义 FOV、去雾、自动闪避、自我复活、治疗/复活队友、创造模式、一键满级、游戏速度控制。
- **视觉**：基于 TMP 的 ESP（敌人、物品、玩家、撤离点），支持方框/名称/距离/血量/价值过滤，上色（Chams）、小地图雷达、追踪线、ESP 预设、地图总价值统计、玩家状态列表。
- **战斗与玩家**：伤害/治疗/击杀/复活玩家、传送玩家、名字伪装、附身控制、自瞄（可调平滑度与距离）。
- **敌人**：致盲/冻结/击杀敌人、禁用陷阱、传送敌人、敌人生成（仅房主）。
- **物品**：物品生成（可设价值，仅房主）、物品传送、价值翻倍、复制手中物品、远程出售、自动拾取、自动出售。
- **传送**：准星传送、撤离点传送、随机传送、命名路径点。
- **世界**：撤离目标归零、一键完成回合、聊天指令（`!help`）、商店工具。
- **房间**：房主接管、大厅发现/创建、RPC 注入、防踢、防崩溃保护。这些网络模块按原样提供，请仅在私人房间中验证。
- **游戏汉化（新增，默认关闭）**：通过游戏自带的 Unity.Localization 字符串表渲染简体中文，附中文字体回退。见[游戏汉化](#游戏汉化)。

## 使用

1. 自行构建（见下文），或从 [Releases](https://github.com/Mai-xiyu/r.e.p.o-cheat/releases) 下载 `r.e.p.o.cheat.dll`。
2. 将 `r.e.p.o.cheat.dll` 与 SharpMonoInjector 的 `smi.exe` 放在同一文件夹。
3. 启动 R.E.P.O. 后，在管理员终端中执行：

   ```
   .\smi.exe inject -p repo -a r.e.p.o.cheat.dll -n r.e.p.o_cheat -c Loader -m Init
   ```

4. 按 **Delete** 打开/关闭菜单（可在热键页改键）。**F5** 重载菜单，**F10** 卸载。
5. `C:\temp\inject_debug.txt` 记录初始化过程与每个 Harmony 补丁类的打补丁结果。

## 游戏汉化

游戏界面简体中文渲染是一项可选功能，**默认关闭**。它经由游戏自身的本地化管线实现，纯本地生效：不触碰 Photon 状态、RPC、玩家数据或大厅状态；玩家名、聊天消息、大厅名永不被翻译。

- 在作弊菜单的 **菜单 > 游戏汉化** 中开启（开关、语言模式：自动/简体中文/英文、缺失翻译扫描）。
- 内嵌词库覆盖官方 603 个字符串表键（HUD、Menu、Game）。
- 外部覆盖文件会合并覆盖内嵌词库：`<游戏根目录>\REPOChinese\zh-CN.json`。
- 缺失翻译一律回退英文。开启扫描后，未翻译的游戏文本会写入 `<游戏根目录>\localization-missing.txt`。

## 配置

- 开关与滑条通过 Unity PlayerPrefs 持久化。
- 扩展配置（`DarkCheat_Config.json`）保存路径点与完整配置。

## 从源码构建

依赖：.NET SDK 8 或更高（`libs/` 中已附带所需的游戏引用程序集）。

```
dotnet build "r.e.p.o cheat.sln" -c Release
```

构建产物为 `r.e.p.o cheat\bin\Release\r.e.p.o cheat.dll`。0Harmony 以内嵌资源形式打入，DLL 自包含。

## 测试

单元测试覆盖纯 C# 汉化组件（翻译数据库、占位符/富文本校验）与兼容性指纹逻辑：

```
powershell -ExecutionPolicy Bypass -File run-tests.ps1
```

## 项目结构

| 目录 | 用途 |
|---|---|
| `r.e.p.o cheat\Core` | Loader 入口、IMGUI 菜单、配置、热键、主题 |
| `r.e.p.o cheat\Compatibility` | 游戏指纹与能力检测 |
| `r.e.p.o cheat\Localization` | 游戏汉化模块与翻译数据 |
| `r.e.p.o cheat\Player / Combat / Visuals / Items / World` | 功能模块 |
| `r.e.p.o cheat\Network` | 房间与网络模块 |
| `r.e.p.o cheat\Patches` | Harmony 补丁 |
| `r.e.p.o cheat\Utils` | 公共工具与场景缓存 |
| `r.e.p.o cheat.Tests` | 单元测试 |
| `libs` | 游戏引用程序集（见上兼容性） |
| `.api-compatibility.md` | 所用 API 兼容矩阵与更新流程 |

## 贡献

欢迎贡献。适配新游戏版本时请同步维护兼容层：更新 `Compatibility\GameVersionInfo.cs` 中的指纹，按 `.api-compatibility.md` 的流程重跑 API diff，并保持测试通过。

## 致谢

本项目源自 [D4rkks/r.e.p.o-cheat](https://github.com/D4rkks/r.e.p.o-cheat) 及其社区，源码经反编译回填并重构，在此致谢原作者。

## 许可证

[MIT](LICENSE)

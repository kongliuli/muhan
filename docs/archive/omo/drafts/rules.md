# ModernBoxes 全局规则

> 本文件记录 ModernBoxes 项目开发中遇到的问题和教训，适用于所有后续 OpenCode 会话。
> 破坏这些规则的任何工作被视为不合格，必须回退后重做。
>
> **部署**：将此文件内容复制到 `.opencode/rules.md` 使其成为 OpenCode 自动加载的项目规则。

## R1. 代理完成声明零信任原则

**规则**：任何代理（agent）返回的"Done"/"Build 0/0"声明，在父进程独立验证前，一律视为未经验证的声明（Claim），不得标记为 todo 完成。

**验证门控**：每个 todo 标记完成前，必须：
1. `dotnet build -f net10.0-windows` 在项目根目录运行，确认 exit code 0 且无 error/warning（NU 包警告除外）
2. 代理声称创建/修改的每一个文件，用 `Test-Path` 或 `Read` 确认存在且内容非空
3. 代理声称删除的旧文件，用 `grep` 或 `glob` 确认旧路径/命名空间引用的清理状态

**发生事故**：2026-06-23 Todo 1 代理声明 "Build: 0 errors 0 warnings"，实际所有 .cs/.xaml 源文件被删除。父进程未独立验证即标记完成，导致 Todo 2-10 在空项目上继续执行，累计约 40 个代理任务单位的工作全基于幻影构建通过声明，实际全部无效。

**措施**：验证门控作为 todo 完成的前置条件，不得跳过。

## R2. 源文件保护规则（重构/移动操作）

**规则**：任何涉及文件移动、重命名、删除的操作，必须满足：
1. 原始文件在操作前通过 `Get-ChildItem -Recurse` 获取完整快照（记录到 evidence 或 ledger）
2. 目标路径的所有文件在操作后通过相同命令验证存在
3. 如果操作后出现文件丢失或残余，回退整个 batch，重新检查

**特别保护目录**：
- `Resource/` — 二进制资源文件（.png, .svg, .ico, .ttf），不可删除，不可移动（仅命名空间引用可改）
- `*.xaml` — WPF 视图定义，移动时须同步更新 `clr-namespace` 引用和 `x:Class` 属性
- `*.cs` — 源文件，移动时须同步更新 `using` 语句和命名空间声明
- `*.json` — 配置文件，移动时须同步更新 `.csproj` 中的 `Content Include` 路径

**发生事故**：2026-06-23 Todo 1 代理违反此规则，删除了 Resource/ 全部文件和所有源文件。

## R3. 压缩摘要不可作为事实来源

**规则**：`compress` 工具生成的摘要 MAY 包含过时或不准确的声明。每次恢复执行前，应：
1. 重新读取 `.omo/plans/<slug>.md` 确认未完成 todo 的准确数量和内容
2. 运行 `dotnet build` 确认当前代码库的实际状态
3. 如发现摘要与实际不符，立即将矛盾记录到 draft，以实际状态为准

**原因**：压缩摘要是摘要作者的主观总结，可能错误地声称"Build 通过"或"todo 完成"，而实际情况相反。

## R4. 重构前基线快照规则

**规则**：任何涉及 >5 个文件的批量重构前，必须：
1. 对重构范围内的所有文件做备份（copy 到 `.omo/backups/<YYYY-MM-DD>/`）
2. 基线 `dotnet build` 记录 exit code、文件数、警告数
3. 重构后对比验证：build exit code 相同或更好，文件总数减少但关键文件全部存在

## R5. 禁止静默删除的项目文件

**规则**：`Remove-Item -ErrorAction SilentlyContinue` 不得用于可能包含项目源码的目录。如需清理，使用 `-ErrorAction Stop` 并显式检查错误，或移动文件到临时目录而非删除。

**发生事故**：2026-06-23 执行了 `Remove-Item -Recurse -Force "modern-box-master" -ErrorAction SilentlyContinue`，险些删除包含所有原始源码的目录。幸好命令因未知原因静默失败，文件得以保留。

## R6. 用户消息优先规则

**规则**：当用户在多个回合中表示"让我来做X"或"我会提供Y"，应等待用户提供，不要继续自主推进大规模变更。

---

*最后更新: 2026-06-23 — 基于 Todo 1 严重事故和 modern-box-master 恢复经验*

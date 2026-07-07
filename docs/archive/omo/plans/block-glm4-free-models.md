# 屏蔽 glm4 免费模型自动回调 — 配置计划

## 问题
`zhipu` provider 下的 `glm-4-flash`（永久免费）和 `glm-4.7-flash`（免费）模型在子代理回调时卡在 "reserved" gate，导致计划中的 3 次 Momus/Metis 审查、以及后续的 Todo 并行任务全部挂起。

## 解决方案

编辑 `C:\Users\yf\.config\opencode\opencode.jsonc`，在文件末尾 `}` 前插入：

```jsonc
  "experimental": {
    "policies": [
      { "action": "provider.use", "effect": "deny", "resource": "zhipu/glm-4-flash" },
      { "action": "provider.use", "effect": "deny", "resource": "zhipu/glm-4.7-flash" }
    ]
  }
```

## 完整文件结构（只改结尾）
```jsonc
{
  "$schema": "https://opencode.ai/config.json",
  "plugin": [ ... ],
  "provider": {
    "deepseek": { ... },
    "aker": { ... },
    "zhipu": {
      ...,
      "models": {
        "glm-4-flash":   { "name": "GLM-4-Flash (永久免费)" },
        "glm-4.7-flash": { "name": "GLM-4.7-Flash (免费)" },
        "glm-4.7":       { "name": "GLM-4.7" },
        "glm-4-plus":    { "name": "GLM-4-Plus" }
      }
    }
  },
  "experimental": {
    "policies": [
      { "action": "provider.use", "effect": "deny", "resource": "zhipu/glm-4-flash" },
      { "action": "provider.use", "effect": "deny", "resource": "zhipu/glm-4.7-flash" }
    ]
  }
}
```

## 效果
- OpenCode 在选择模型时会跳过这两个被 deny 的免费模型
- sub-agent (general/explore/Momus/Metis) 不会再用 glm-4-flash 执行自动回调
- 其他 zhipu 模型（glm-4.7、glm-4-plus）不受影响
- 你手动在聊天中指定使用 glm-4-flash 也不受影响（policy 只影响自动选择）

## 操作
1. 用记事本打开 `C:\Users\yf\.config\opencode\opencode.jsonc`
2. 在最后一个 `}` 前粘贴上述 `experimental` 段
3. 保存后重启 OpenCode
# Milky Native AOT 请求绑定修复

## 原因

`RequestBinder<T>` 在第一次请求时初始化字段解析器。仅注册 JSON 源生成上下文不能替代 FastEndpoints 的 DTO / scalar 绑定缓存；缓存缺失时，数值等类型会走运行时表达式解析器。该路径使用 `Expression.Convert(StringValues, string)`，Native AOT 裁剪后可能找不到隐式转换运算符，最终表现为请求绑定器的 `TypeInitializationException`，尚未进入发送消息业务。

参考：[FastEndpoints 8.3 BinderExtensions](https://github.com/FastEndpoints/FastEndpoints/blob/v8.3/Src/Library/Binder/BinderExtensions.cs)。当前修复统一通过 `ConfigureMilkyBinding()` 注册生成 JSON、生成 DTO 缓存，以及静态编译的 `int/uint/long/ulong/bool/DateTime` 解析器。可空字段使用其底层类型的解析器，不依赖反射发现 `TryParse` 或转换运算符。不关闭裁剪，不抑制 AOT 警告。

日志能确认运行中的程序进入了该回退路径，不能仅凭日志确认其部署版本。更新源码后仍需重新发布并重启进程；只重发请求不能恢复已经失败的泛型类型初始化。

## 同类问题

- 修复群公告的两个布尔选项、图片宽高在省略时被源生成反序列化覆盖为零值的问题。
- 修复闪传提交的默认索引、格式代码丢失。
- 修复图文卡片五个默认空字符串变成 null。
- 上述11个属性保留 JSON 名称及属性类型，改用 setter 保留未提供字段的初始化值；显式 false / 0 仍有效。
- Docker restore 阶段补入 Codec 项目；排除宿主机 bin / obj，避免把不同平台的生成文件带进 Linux 构建。

## 验证

`Lagrange.Milky.Test/ExistingRequestDefaultsTests.cs` 的修复前基线为3失败、2通过，修复后 Milky 全部25项测试通过。

独立 `Lagrange.Milky.AotSmoke` 使用生产绑定配置，直接调用实际 `RequestBinder<T>.BindAsync`。它不启动 HTTP 监听，不创建机器人，不向 QQ 发消息。覆盖群与私聊发送、消息段多态集合、字符串、可空数值、布尔、无符号整数、日期以及非法数值拒绝。

```powershell
dotnet test Lagrange.Milky.Test -c Release
dotnet publish Lagrange.Milky.AotSmoke -c Release -r win-x64
./Lagrange.Milky.AotSmoke/bin/Release/net10.0/win-x64/publish/Lagrange.Milky.AotSmoke.exe --require-native
docker build --target binding-smoke -f Lagrange.Milky/Resources/Dockerfile -t lagrange-milky-binding-smoke:local .
```

负向复现仅作用于 smoke 项目，详见该项目 README；生产程序不提供关闭绑定配置的开关。2026-10-05 验证结果：Windows win-x64 负向原生程序在 SendGroupMessage 请求绑定器初始化时复现与用户日志一致的 `No coercion operator ... StringValues ... String` 异常；Linux Alpine linux-musl-x64 修复版 Native AOT 发布并执行12项绑定场景全部通过。Linux 主程序 Native AOT 发布同时通过，未产生 IL 裁剪/AOT 告警。仅构建本地测试镜像，未替换正在部署的容器。

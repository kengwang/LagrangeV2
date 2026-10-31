# LagrangeV2 能力迁移与验收

## Core 与 Milky 能力更新

实施日期：2026-10-31。LagrangeV2 比较基线：`ec0b3ff4`。参考范围：`fb5f9b21558134db8803d216dfc19c6bce11f2c0..11d42b4e98d4b6ce32c78c04bb2ee3e92d2f34dd`，共 196 个非合并提交。

Core 保持 net8.0/net9.0；Milky、Codec 与验证程序使用 net10.0。新增功能通过既有协议事件、Service、消息实体和 FastEndpoints 提供，并登记 OpenAPI 与 MCP 元数据。

### 协议与行为

| 能力 | 上游来源 | 当前行为 |
|---|---|---|
| 设备会话 | `dac3deb7` | 固定设备联系人加入好友列表，私聊文本通过 Trans0x211/7 发送；529/7 接收文本和文件通知，保留设备身份及方向 |
| 群分类标签、分享链接 | `f0203823`、`f85b28d2` | 分类编号允许 0；标签保留编号、名称、类型；分享支持短链接及附加参数 |
| 管理员权限 | `d30cd1ba`、`8b8a9df8` | 查询撤回、移除成员、每日移除上限及验证通知；未知值保留 null；更新仅发送指定权限并校验目标身份 |
| Qzone 资料与回复 | `4d55608b`、`d95d3bf7` | 名称和描述支持部分修改及空字符串清空；详情返回评论与回复；回复关联原始评论并检查确认编号 |
| 好友动态分页 | `9bf4b910` | 保留既有字段，新增正文、图片、完整性、互动数量与账号关联游标；按页码读取时逐页携带服务器状态 |
| 首页、公开动态与分享 | `2f1aadff`、`eb119ecb` | 完整动态对象用于详情、互动、关注、转发和卡片生成；保留分享可见性提示 |
| 发送确认及私聊序号 | `2c7d9b63`、`bd14b0a9` | 检查审核结果、业务结果和真实确认；私聊区分请求序号与服务器序号；图文卡片校验逐项投递结果 |
| 文件回执、转发媒体 | `4379e483`、`075a27cc`、`9d8df593` | 下载回执不产生新文件事件；保留可用兼容图片；XML 转发从根元素读取资源标识，继续使用目标会话准备嵌套媒体 |
| 系统表情、收藏表情 | `e05d3176`、`efb32ed0`、`93b5c1b4`、`0cc06b6b` | 保留全部目录分组；增加单个表情查询；保留动画参数与结果；图片上传携带真实子类型和摘要 |
| 语音 | `0a6c5b7d`、`368becd0`、`a557bf72` | 从真实 PCM 计算 30 个幅度值；私聊上传省略通用标志；发送保留服务器原始播放兼容数据，接收避免重复语音实体 |
| Highway 上传 | `44679beb`、`5c932820` | 保留完整上传地址，校验分片响应，执行有界并发，失败时取消同次上传并释放资源；不自动重试 |
| 群相册评论 | `131a3093`、`c88c8e75`、`ceb8387e`、`20f48d11` | 先分页定位媒体，再查询动态详情；原样复制动态公共、所有者和媒体字段；支持视频封面及 64 位批次编号 |
| 过滤好友申请、单向好友 | `41b92263`、`797a4d75`、`26639ffe` | 读取数字或 UID 形式的申请人，修正时间和群号字段；批准使用 0xd72_0；单向好友使用专用封装并检查 JSON 业务状态 |
| 用户资料 | `3068d3ef`、`d8763c98` | 增加注册时间、会员信息及生日年月日，支持隐藏年份的二月二十九日；保留已有 Birthday 属性 |
| 邀请及历史 | `bb4443b1`、`299fa85a`、`304f3e77` | 接收直接群邀请，按群及双方身份匹配唯一待处理申请并去重；私聊历史业务错误继续传播 |

既有临时会话上传已经携带来源群；继续使用相关协议字段测试验证。既有 message_style、转发元数据、临时会话和媒体准备规则保留。

### 新增接口

全部接口位于 `/api/`，使用 POST 和既有 Milky 鉴权；启用 MCP 后使用相同路由名称提供工具。

- 群：`get_group_tags`、`add_group_tag`、`delete_group_tag`、`get_group_share_link`、`get_group_admin_permissions`、`set_group_admin_permissions`、`set_group_admin_receive_verify`。
- 表情：`get_system_face`。既有目录和搜索结果增加 `animation_type`、`animation_pack_id`、`animation_sticker_id`。
- 空间资料与评论：`get_qzone_profile`、`set_qzone_profile`、`get_qzone_message_detail`、`reply_qzone_comment`。
- 动态读取：`get_qzone_home_feeds`、`get_qzone_public_capabilities`、`get_qzone_public_feeds`、`get_qzone_public_feed_detail`、`get_qzone_public_comments`、`get_qzone_public_replies`。
- 动态操作：`set_qzone_public_upvote`、`set_qzone_public_like`、`set_qzone_public_comment_like`、`comment_qzone_public_feed`、`reply_qzone_public_feed`、`delete_qzone_public_comment`、`delete_qzone_public_reply`、`set_qzone_public_follow`、`forward_qzone_public_feed`、`get_qzone_share_ark`。

群分类接口接受 `group_class`；添加和删除另需 `name`。管理员接口接受 `group_id`、`user_id`；启用 `kick_member` 时必须提供 `kick_member_per_day`，取值为 0、5、10、20、50、100 或 200，0 表示不限制每日数量。查询返回的 null 表示服务器没有提供有效状态。

设备编号为 `0x7E000001`（电脑）、`0x7E000002`（手机）、`0x7E000003`（Pad），通过 `get_friend_list`、`send_private_message` 和好友消息事件使用。设备发送要求非空文本；接收文件通知提供原始文件标识与元数据。

Qzone 公开动态接口的 `feed`、`comment`、`reply` 接受读取接口返回的完整对象。`content` 接受文本或 QQ 富文本对象数组。主页游标关联当前账号及主页；推荐游标关联当前账号。普通好友动态继续支持 `page_num`、`position`、`count`，并接受 `cursor`；下一页使用 `next_cursor`。说说详情和回复沿用 `target_uin`、`message_id`；回复另需 `comment_id`、`comment_uin`，可选 `reply_uin`、`reply_name` 用于指定回复对象。

`get_qzone_share_ark` 返回 `ark`、`requires_confirmation`、`notice`，由调用方阅读提示后使用已有消息接口发送卡片。

### 配置与依赖

普通 QQ 视频的 Highway 分片并发由 `Lagrange.Server.VideoUploadConcurrency` 控制，默认 1，范围 1–4；环境变量使用 `Lagrange__Server__VideoUploadConcurrency`。每次上传保存其并发设置、地址及票据，失败后阻止后续消息发送。

Qzone HTML 使用 AngleSharp 1.9.0，JavaScript 字面量使用 Acornima 1.9.1，JSON 使用 System.Text.Json。JavaScript 仅接受数据节点，拒绝执行表达式；64 位整数保留精度。

Milky 准备语音时通过既有 LagrangeCodec 转换音频并计算波形；发送已编码 SILK 时也需要该动态库用于解码波形。Core 调用方可以设置 `RecordEntity.Waveform` 的 30 个幅度值，或调用 Codec 的 `VoiceWaveform.FromPcm` / `FromAudioAsync` 生成。

### 自动化验证

Core：400 项通过，1 项因缺少本机 LagrangeCodec 动态库跳过。Milky：93 项通过，包含全部 28 个新增接口的鉴权覆盖。Proto：221 项通过。Core net8.0/net9.0 Release 构建均通过，零警告、零错误。

win-x64 Native AOT 冒烟程序实际运行通过，覆盖生产配置绑定、请求默认值、开放 JSON 对象、Qzone JavaScript/HTML 解析、OpenAPI 全部 API 路由、MCP 鉴权、工具目录、Schema、真实本机调用及错误处理。Milky win-x64 Native AOT 发布通过，产物位于 `.artifacts/capability-update-milky-win-x64`。

验证覆盖独立协议字段、脱敏好友申请报文、真实 PNG 输入、PCM 波形、生日日期、权限参数、序号、Qzone 数据解析和游标、动态原始字段、生成 JSON 元数据及真实本机 HTTP/MCP 传输。

```powershell
dotnet test Lagrange.Core.Test/Lagrange.Core.Test.csproj
dotnet test Lagrange.Milky.Test/Lagrange.Milky.Test.csproj
dotnet test Lagrange.Proto.Test/Lagrange.Proto.Test.csproj
dotnet build Lagrange.Core/Lagrange.Core.csproj -c Release
dotnet publish Lagrange.Milky.AotSmoke -c Release -r win-x64
./Lagrange.Milky.AotSmoke/bin/Release/net10.0/win-x64/publish/Lagrange.Milky.AotSmoke.exe --require-native
./Lagrange.Milky.AotSmoke/bin/Release/net10.0/win-x64/publish/Lagrange.Milky.AotSmoke.exe --require-native --mcp
dotnet publish Lagrange.Milky -c Release -r win-x64 -o .artifacts/capability-update-milky-win-x64
```

按本次要求没有执行 QQ 实机操作；QQ 服务端接受情况及最终客户端显示没有实机验收。Native AOT 发布中的 FastEndpoints.Mcp、ASP.NET Core 既有反射与裁剪告警继续保留。

## 2026-10-05 能力补全验收

项目基线：LagrangeV2 `dcc3d11`。实施日期：2026-10-05。

范围为 Core + Milky，音视频复用 Lagrange.Codec。未添加 OneBot 服务、action、消息 ID 映射或兼容别名，也未扩展 NativeAPI。下表“接通”表示已有公开调用或消息事件链路，不代表 QQ 服务端已接受请求。新增路由是 QQ 扩展，不宣称为 Milky 标准。

## 能力状态

| 批次 | 能力 | 代码接通 | 自动化验证 | QQ 实机 |
|---|---|---|---|---|
| 1 | 临时会话身份、来源群、发送 | 是；`SendTempMessage` / `send_temp_message` | 路由字段3/4、双方身份、缺失CommonC2C、DTO | 服务器拒绝，返回1；需已有临时会话继续验收 |
| 1 | 群名片事件、好友备注缓存、设备快照 | 是；先更新缓存再派发，首次名片观察不发变化事件 | 名片首次/变化、快照未知/空、生成序列化 | 待执行 |
| 1 | flash_file | 是；旧Markdown URI JSON及新extType/extInfo | 两种卡片、场景、普通Markdown不误判 | 待执行 |
| 1 | 动画/超级表情、闪照、键盘字段 | 是；保留服务37结果及键盘稀疏字段 | 独立协议字节、非法表情、闪照属性、序列化 | 待执行 |
| 1 | 转发文件、转发元数据、缺失回复 | 是；file仅接受转发内引用；实时文件走上传API | 输入限制、转发标题/文件、缺失目标处理 | 待执行 |
| 2 | 群待办列表、精确完成/取消 | 是；旧方法保留，新方法携带序号 | 身份匹配、请求序号与命令 | 列表成功（空）；完成/取消待验 |
| 2 | 群相册分页、视频上传 | 是；真实SSO游标，视频及封面两阶段上传 | 请求包、响应样本、重复游标、失败/取消/分片 | 上传1秒视频及封面成功，列表匹配视频ID |
| 2 | 闪传完整上传、下载、分享 | 是；多文件，专用主文件下载协议 | 抓包样本、编排、秒传、分片、失败与取消 | 两个文件上传、下载内容校验、分享码解析通过 |
| 2 | 历史同步探测、序号与时间游标 | 是；只读探测，不引入数据库 | 省略已读确认字段、游标推进、错误/身份检查 | 群探测与序号页、私聊漫游页成功 |
| 2 | 在线设备、用户状态、RKey | 是；设备快照区分未知与空 | 快照、状态解码、RKey范围与错误 | 状态及2个RKey成功；设备尚未收到快照 |
| 2 | Qzone权限、小程序分享、数据库密钥 | 是；参数化密钥盐；权限更新保留原文及图片 | 权限内容保留/身份保护、稀疏协议字段、DTO | 待执行 |
| 3 | 私聊/群聊转写 | 是；同步文本或0x210/61异步关联 | 提前推送、并发、重复、超时/取消/断线/错误 | 群转写成功；私聊异步结果等待超时，待验 |
| 3 | AI语音列表、合成下载、真实发送回执 | 是；媒体UUID关联，不伪造序号、不超时重发 | 回执先到、群/发送者/UUID匹配、重复与中断 | 30个角色；发送得到真实消息序号1274 |
| 3 | 内联按钮点击 | 是；群/应用/按钮/回调/序号 | 命令、字段、返回值及路由鉴权 | 待执行 |
| 3 | PCM/WAV/SILK转换 | 是；24kHz单声道S16，显式输出格式 | 托管转换、WAV头、SILK校验、取消；原生样本跳过 | 缺原生库 |
| 3 | MP3/FLAC输出编码 | 未完成；原生编码器依赖 | 明确返回不支持 | 未执行 |

## 接口与方向

`Lagrange.Core/Message/MessageElementManifest.cs` 是方向清单。测试核对实体集合、Milky多态输入与输出集合，并对迁移涉及的协议样本和转换路径做行为验证。flash_file仅入站；闪照只保留入站语义；file输入只允许在转发节点内。发送闪传走专用业务API。

新增Milky路由（全部POST并使用既有Milky鉴权）：

- 消息：`send_temp_message`、`transcribe_voice`、`get_ai_voice_list`、`synthesize_ai_voice`、`send_ai_voice`。
- 交互及群：`click_inline_keyboard_button`、`get_group_todo_list`、`upload_group_album_video`。
- 文件：`get_download_rkeys`、`upload_flash_files`、`get_flash_download`、`get_flash_share_link`、`resolve_flash_share_code`、`convert_record`。
- 状态：`get_online_devices`、`get_user_status`、`get_mini_app_ark`、`request_database_key`、`set_qzone_message_visibility`。
- 历史：`probe_history_sync_state`、`get_history_sync_page`、`get_private_history_roam_page`。

以上均位于 `/api/`。旧 `get_group_album_list` 返回真实 `attach_info` / `has_more`；旧待办完成/取消路由增加可选 `message_seq`。新增事件为 `group_member_card_change`，含群、成员及新旧名片。临时消息使用 `message_scene: temp` 和 `group_id`；转写接口用现有场景/会话/消息序号定位原消息。

## 使用限制

- 在线设备是当前登录会话收到的服务器快照；`observed=false`与`devices=[]`含义不同，断线后清空观察状态。
- 相册视频需要真实封面 `cover_uri`。可提供精确 `duration_ms`；省略时使用Codec时长探测（原生接口精度为整秒）。现有原生首帧提取有分配器契约问题，未自动调用该路径。
- 闪传上传会临时暂存输入并自动清理；单文件上限为32位长度。下载接口仅返回描述，不下载到服务器磁盘。多文件失败会抛错，不把未完成文件报告为成功。
- 历史探测每类最多100个目标，页大小最多20；调用方保存游标并控制请求频率。私聊历史需要已知UID。
- AI语音合成操作本身可能在QQ群发布语音。`send_ai_voice`等待对应自身消息的真实序号；超时或取消不自动重新发布。
- 数据库密钥只接受参数 `db_salt`，不读取QQ服务器文件路径。签名配置、密钥和RKey不得写入测试报告。
- PCM/WAV/SILK原生实测需要与进程架构匹配的LagrangeCodec动态库。MP3/FLAC编码未完成，未引入FFmpeg服务。

## 自动化及发布

复现命令：

```powershell
dotnet test Lagrange.Core.Test/Lagrange.Core.Test.csproj
dotnet test Lagrange.Milky.Test/Lagrange.Milky.Test.csproj
dotnet publish Lagrange.Milky/Lagrange.Milky.csproj -c Release -r win-x64 --self-contained true
```

Core NUnit：214通过、1项因缺少原生Codec跳过；Milky NUnit：20通过。Core net8.0 Release构建0警告0错误，net9.0通过Core/Milky构建验证。Native AOT win-x64发布通过，最终产物登录后验证了设备快照、用户状态、默认参数AI角色列表、历史探测、群转写及相册视频读取；已按Web SDK裁剪默认值禁用增强MVC模型元数据，消除IL2026/IL3053，不抑制分析告警。原生音频样本测试在缺少动态库时明确跳过。HTTP契约测试使用临时本地监听端口，不启动QQ登录；覆盖22个扩展接口未授权拒绝以及已授权设备快照响应。

## 实机记录

用户授权测试群 `1045113659`、好友 `1136772134`，并创建“测试相册”。配置和密钥复制到独立临时目录，使用仅回环监听和独立访问令牌，原文件未修改。远程签名服务 TLS 失败后，按用户指示使用本地 UniQsign，登录成功。

- 群/好友身份查询成功。用户在线状态、RKey、待办列表、相册列表、历史同步探测及群消息分页成功；在线设备尚未收到快照，正确返回 `observed=false`。
- 闪传上传两个33字节文本文件，分别下载到内存校验内容完全一致；分享链接和分享码解析回同一文件集。
- AI语音角色列表返回4类30个角色；发送测试语音得到真实回执序号 `1274`，没有生成本地伪造序号。测试语音保留在授权群中。
- 临时会话请求返回协议错误1，服务器未提供说明。已对照路由与控制字段；未将服务端拒绝推断为某种已确认的权限原因，仍需已建立临时会话的目标验收。
- 相册视频首次分片超时；对齐浏览器 multipart 格式后复测成功，视频与封面均返回有效结果，媒体列表包含相同视频ID。进一步修复了列表按固定type值误判视频的问题。测试视频保留于用户建立的“测试相册”。
- 群语音转写返回“语音回执迁移验证。”，内容正确。修复了AI历史中关联字段误用MsgUid的问题。同一语音也已发送至授权好友，真实回执26799；按该序号读取私聊历史返回QQ空历史错误。私聊漫游页返回2条消息，引用不存在的消息正常保留；对漫游历史中机器人自己发出的语音，转写请求进入异步等待，但30秒内未收到匹配结果；仍需真实私聊入站语音验收。
- 未修改说说可见范围；数据库密钥、真实按钮点击及入站事件等缺少对应实机场景，仍待验收。

实测中另修复了 AOT 请求绑定的生成缓存注册、JSON 源生成对省略参数默认值的覆盖、用户状态与既有 `0xfe1_2` 服务注册冲突，以及群历史返回成员接收方时的身份检查。回复转换对 QQ 明确的空历史错误 `100000301` 保留引用元数据，其他错误和取消继续传播。自动化测试覆盖这些回归点。

测试进程及本地签名辅助进程已停止。自动审批拒绝清理临时目录（仅返回 blocked by policy），因此隔离目录仍含测试凭据副本和临时响应，未提交到仓库；需由用户手动清理。原始配置未改动；群语音、好友测试语音、测试相册视频保留供人工核对。

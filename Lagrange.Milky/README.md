<div align="center">

![Lagrange.Milky](./Resources/banner.svg)

_[Milky](https://github.com/SaltifyDev/milky) protocol implementation based on [Lagrange.Core V2](https://github.com/LagrangeDev/LagrangeV2)_

</div>

## Document

https://lagrangedev.github.io/Lagrange.Milky.Document

## Contribute

### api

Write an implementation of `IApiHandler<TRequest, TResult>`/`INoRequestApiHandler<TResult>`/`INoResultApiHandler<TRequest>` in the folder corresponding to the `Lagrange.Milky\Api\Handlers` category, and add `TRequest` and `TResult` to the collection of `[JsonSerializable]` attributes above `JsonContext` in `Lagrange.Milky\Serialization\Serializer.cs`.

### event

Write an implementation of `IEventConverter<TEvent, TData>` in `Lagrange.Milky\Events\Converters`, and add `TData` to the `[JsonSerializable]` attributes above `JsonContext` in `Lagrange.Milky\Serialization\Serializer.cs`.

## Feature List

### Logging

Milky uses Serilog for text console logs and an optional JSON Lines file sink.
Existing `Logging:LogLevel` defaults and category overrides apply to both outputs.
To enable file logging, add this to the `Logging` section of `appsettings.json`:

```json
"JsonFile": {
  "Enabled": true,
  "Path": "logs/milky-.jsonl",
  "RetainedFileCountLimit": 7,
  "FileSizeLimitBytes": 104857600
}
```

File logging is disabled by default. Relative paths resolve against the application's
content root; absolute paths are supported. Files roll daily (for example,
`milky-20261012.jsonl`) and on reaching the size limit (100 MiB by default).
Retention counts files, including size-based rolls, rather than days.
Each line is an independent JSON object containing timestamp, level, message template,
rendered message, structured properties (including logger category and scopes), and
exception details when present. Writes are flushed per event; the logger is disposed
after the host on shutdown. File sink failures are reported directly to standard error;
console logging continues.

Restart Milky after changing file settings. Standard configuration overrides work,
for example `Logging__JsonFile__Enabled=true`. The Banner, QR code graphics and scan
links for login, new-device verification and online captcha use `Console.WriteLine`
directly and are excluded from the log sinks. Scan status, login results, SMS prompts
and other captcha prompts remain log events.

### Raw SSO packets (Lagrange extension)

`POST /api/send_packet` sends an arbitrary binary SSO payload over the bot's
authenticated connection. Authentication uses the usual Milky access token.
The payload is not required to be Protobuf; JCE and other binary formats also work.

```json
{"cmd":"Example.Command","data":"0801aB00","rsp":true}
```

`cmd` is required and must not be blank. `data` defaults to `""` and accepts
continuous, even-length hexadecimal text in either case. `rsp` defaults to `true`.
Explicit null values are invalid. With `rsp=true`, a received response looks like:

```json
{"status":"ok","retcode":0,"message":null,"data":{"cmd":"Example.Command","data":"0801AB00","sequence":5000001,"ret_code":0,"extra":""}}
```

`status=ok` means a response was received, even when `data.ret_code` is nonzero.
The error payload and `extra` are preserved. With `rsp=false`, the endpoint returns
the usual success response with `data={}` after the complete frame is written;
this does not confirm acceptance by QQ. Operations time out after 10 seconds.
Invalid input returns `retcode=-400`, timeout returns `-408`, and signing,
connection or sending failure returns `-500`. Packets are never automatically retried.

To receive raw packets, explicitly enable this setting in `appsettings.json`
and restart Milky:

```json
{"Milky":{"Event":{"EnableRawPacketEvents":true,"RawPacketCommandBlacklist":["Heartbeat.Alive"]}}}
```

The default is `false`. The bundled configuration sets `RawPacketCommandBlacklist`
to `["Heartbeat.Alive"]`; when omitted it defaults to `[]`. It matches
complete command names case-sensitively. Listed commands produce no raw packet
events or packet send/receive diagnostics (including unknown-command diagnostics).
Their responses and normal QQ business processing remain active. The blacklist
also suppresses packet diagnostics when raw events are disabled.

When enabled, all other successfully parsed incoming SSO packets
(responses, heartbeats, server pushes and unknown commands) produce
`packet_received` events on the enabled WebSocket, SSE and WebHook transports:

```json
{"time":1791792000,"self_id":123456,"event_type":"packet_received","data":{"cmd":"Example.Command","data":"0801AB00","sequence":5000001,"ret_code":0,"extra":""}}
```

`time` is the local receipt time in Unix seconds. Data is decrypted, decompressed
and encoded as uppercase hexadecimal. Original encrypted TCP frames, outbound
packet events and historical capture are not included. Events use the existing
bounded queues (4096 events per subscriber by default): delivery is best effort and does not guarantee lossless capture
or strict packet order. Enabling this option forwards all packet contents to
your configured event consumers.
Raw event handlers run asynchronously; receiving packets and matching API responses
never wait for event delivery. The original SSO `sequence` is preserved; no additional
packet ordering field is introduced.

- Api
  - [x] Http
- Event
  - [x] SSE
  - [x] WebSocket
  - [x] WebHook

### api

#### system

[x] get_login_info
[x] get_impl_info
[x] get_user_profile
[x] get_friend_list
[x] get_friend_info
[x] get_group_list
[x] get_group_info
[x] get_group_detail_info
[x] get_group_member_list
[x] get_group_member_info
[x] get_group_ext_info
[x] get_peer_pins (explicit unsupported error when QQ wire endpoint is unavailable)
[x] set_peer_pin
[x] set_avatar
[x] set_nickname
[x] set_bio
[x] set_diy_online_status
[x] get_custom_face_url_list
[x] get_cookies
[x] get_csrf_token

#### message

[x] send_private_message
[x] send_group_message
[x] recall_private_message
[x] recall_group_message
[x] get_message
[x] get_history_messages
  - request
    - [x] start_message_seq - Friend history derives the latest sequence when omitted.
[x] get_resource_temp_url
[x] get_forwarded_messages
[x] mark_message_as_read
[x] mark_all_read

#### friend

[x] send_friend_nudge
[x] send_profile_like
[x] get_like
[x] translate_en_to_zh
[x] image_ocr
[x] delete_friend
[x] get_friend_requests
[x] accept_friend_request
[x] reject_friend_request

#### group

[x] set_group_name
[x] set_group_avatar
[x] set_group_member_card
[x] set_group_member_special_title
[x] set_group_member_admin
[x] set_group_member_mute
[x] set_group_whole_mute
[x] kick_group_member
[x] get_group_announcements
[x] send_group_announcement
[x] delete_group_announcement
[x] upload_group_announcement_image
[x] get_group_honor
[x] get_group_sign_in
[x] get_group_essence_messages
[x] set_group_essence_message
[x] quit_group
[x] send_group_message_reaction
  - result
    - reaction_type
[x] send_group_nudge
[x] get_group_notifications
[x] accept_group_request
[x] reject_group_request
[x] accept_group_invitation
[x] reject_group_invitation
[x] set_group_add_option
[x] set_group_search
[x] set_group_new_member_history_visibility
[x] set_group_new_member_history
[x] set_group_member_invite_policy
[x] set_group_invite_policy
[x] set_group_robot_add_option
[x] get_group_admin_settings

#### file

[x] upload_private_file
  - result
    - [x] file_id
[x] upload_group_file
  - result
    - [x] file_id
[x] get_private_file_download_url
[x] get_group_file_download_url
[x] get_group_files
[x] move_group_file
[x] rename_group_file
[x] delete_group_file
[x] create_group_folder
  - result
    - [x] file_id
[x] rename_group_folder
[x] delete_group_folder

### event

- [x] bot_offline
- [x] message_receive
- [x] message_recall
- [x] peer_pin_change
- [x] friend_request
- [x] group_join_request
- [x] group_invited_join_request
- [x] group_invitation
- [x] friend_nudge
- [x] friend_file_upload
- [x] group_admin_change
- [x] group_essence_message_change
- [x] group_member_increase
- [x] group_member_decrease
- [x] group_name_change
- [x] group_message_reaction
  - [x] reaction_type
- [x] group_mute
- [x] group_whole_mute
- [x] group_nudge
- [x] group_file_upload

### models

- [x] Friend
- [x] FriendCategory
- [x] Group
- [x] GroupMember
- [x] GroupAnnouncement
- [x] GroupFile
- [x] GroupFolder
- [x] FriendRequest
- [x] GroupNotification
- [x] IncomingMessage
  - [x] friend
  - [x] group
- [x] IncomingForwardedMessage
- [x] GroupEssenceMessage
- [x] [IncomingSegment](#imcoming-segment)
- [x] OutgoingForwardedMessage
- [x] [OutgoingSegment](#outgoing-segment)

### imcoming segment

- [x] text
- [x] mention
- [x] mention_all
- [x] face
- [x] reply
- [x] image
- [x] record
- [x] video
- [x] file
- [x] forward
  - [x] title
  - [x] preview
  - [x] summary
- [x] market_face
- [x] light_app
- [x] xml

### outgoing segment

- [x] text
- [x] mention
- [x] mention_all
- [x] face
- [x] reply
- [x] image
- [x] record
- [x] video
- [x] forward
  - [x] title
  - [x] preview
  - [x] summary
  - [x] prompt
- [x] light_app
- [x] market_face
- [x] xml

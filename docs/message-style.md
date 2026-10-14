# Message styles

Core and Milky support optional message presentation parameters. `message_style`
is a Lagrange extension to Milky. NativeAPI is unchanged.

| Core property | Milky JSON field | Type |
| --- | --- | --- |
| BubbleId | bubble_id | ulong |
| PendantId | pendant_id | ulong |
| FontId | font_id | ushort |
| FontEffectId | font_effect_id | uint |
| IsCsFontEffectEnabled | is_cs_font_effect_enabled | bool |
| BubbleDiyTextId | bubble_diy_text_id | uint |

Omission or `null` means no style override. An explicit `{}` uses zero/false for
all six fields. Incoming messages omit `message_style` when no style metadata is
present. QQ's acceptance of a resource ID determines its final display; this API
does not look up resources or change account decorations.

## Core

```csharp
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;

var chain = new MessageBuilder()
    .Text("Hello")
    .Style(new MessageStyle { BubbleId = 123, PendantId = 456, FontId = 789 })
    .Build();
await bot.SendGroupMessage(groupUin, chain);
```

`MessageChain.Style` is mutable; `BotMessage.Style` refers to the same property.
Received messages, history messages and forwarded nodes retain their own styles.

## Milky sends

The three endpoints `send_group_message`, `send_private_message` and
`send_temp_message` accept `message_style` beside `message`:

```json
{
  "group_id": 123456,
  "message": [{ "type": "text", "data": { "text": "Hello" } }],
  "message_style": {
    "bubble_id": 123,
    "pendant_id": 456,
    "font_id": 789,
    "font_effect_id": 2000,
    "is_cs_font_effect_enabled": true,
    "bubble_diy_text_id": 0
  }
}
```

Private sends use `user_id`; temporary sends use both `group_id` and `user_id`.
IDs are JSON numbers; clients must preserve unsigned 64-bit values without
rounding. Style also appears in message events, message/history queries and
forwarded-message queries.

## Forwarded nodes

Each outgoing node may specify its own style:

```json
{
  "group_id": 123456,
  "message": [{
    "type": "forward",
    "data": {
      "messages": [{
        "user_id": 10001,
        "sender_name": "Alice",
        "message_style": { "bubble_id": 123, "pendant_id": 456 },
        "segments": [{ "type": "text", "data": { "text": "Styled node" } }]
      }]
    }
  }]
}
```

Nested nodes retain their own style. They do not inherit the outer message's
style. Incoming forwarded nodes and expanded long-message nodes expose the same
optional `message_style` object.

## Verification

The implementation is covered by Core protocol replay and Milky conversion /
generated JSON tests, including unsigned boundaries, font encoding, damaged
reserve data, legacy voice and nested forward uploads/downloads. The Windows
`win-x64` Native AOT smoke executable was published and run with
`--require-native`, covering the actual production request binding paths.

QQ online display has not been verified. Tests use synthetic contacts and local
protocol replies; they do not send messages to QQ.

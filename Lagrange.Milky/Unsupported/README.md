# Unsupported Milky 1.2 APIs

The following Milky APIs are intentionally not registered because this tree does not currently have a reliable LagrangeV2-compatible protocol implementation to call. They should not be implemented with guessed packets or empty placeholder responses.

- `set_nickname`
- `set_bio`
- `get_group_announcements`
- `send_group_announcement`
- `delete_group_announcement`
- `get_group_essence_messages`

The group announcement APIs and `get_group_essence_messages` have OneBot-side references in `E:\Clones\Lagrange.Core`, but those references use web endpoints through `TicketService`/cookies rather than a Core protocol service that is currently present in LagrangeV2. They remain unregistered until that HTTP ticket capability is ported deliberately.

## Unsupported Milky events and segments

The following Milky events are intentionally not emitted because this tree does not currently expose matching Core event args or a reliable parser entry point:

- `peer_pin_change`
- `friend_nudge`
- `friend_file_upload`
- `group_admin_change`
- `group_essence_message_change`
- `group_name_change`
- `group_mute`
- `group_whole_mute`
- `group_file_upload`

The `keyboard` message segment is also intentionally not registered. It is not present in the Milky 1.2.2 schema or markdown roadmap used for this alignment pass; it should only be added after a reliable Milky document or protocol reference is available. The `markdown` segment is kept as a Lagrange extension because Core has a verified entity implementation.
